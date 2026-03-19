using _Project.Scripts.Audio;
using _Project.Scripts.LevelsManagement;
using _Project.Scripts.PauseManagement;
using _Project.Scripts.Training;
using _Project.Scripts.Utilities;
using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace _Project.Scripts.EnemiesManagement.Spawn
{
    public class EnemiesSpawner : IDisposable
    {
        private readonly Dictionary<Enemy, CompositeDisposable> EnemiesInLevel = new();
        private readonly Dictionary<string, ObjectPoolWithQueue<Enemy>> ObjectPoolsByEnemy = new();
        private readonly CompositeDisposable Disposables = new();

        private Transform _enemiesContainer;
        private Transform _currentSpawnEnemiesPoint;
        private IDisposable _tutorialDisposable;

        private IAudioService _audioService;
        private TrainingController _trainingController;
        private LevelCompletionManagement _levelCompletionManagement;
        private LevelsCreator _levelsCreator;
        private PauseController _pauseController;

        private MotionHandle _timerForStartWaveHandle;
        private List<Transform> _currentMovingPoints;
        private EnemysInLevelSpawnSeqence _currentEnemiesInLevelSpawnSequence;
        private CancellationTokenSource _spawnCancellationTokenSource;
        private bool _spawningProcessActive = false;

        // R3 events
        private readonly Subject<Enemy> EnemyMovedToLastPoint = new(), EnemyDied = new();
        private readonly Subject<Unit> AllEnemiesDefeated = new();
        private readonly ReplaySubject<int> SecondsRemainingUntilStartChanged = new();

        public Observable<int> ReadOnlySecondsRemainingUntilStartChanged => SecondsRemainingUntilStartChanged;
        public Observable<Enemy> ReadOnlyEnemyDied => EnemyDied;
        public Observable<Enemy> ReadOnlyEnemyMovedToLastPoint => EnemyMovedToLastPoint;
        public Observable<Unit> ReadOnlyAllEnemiedDefeated => AllEnemiesDefeated;

        public void Initialize(LevelsCreator levelsCreator, LevelCompletionManagement levelCompletionManagement, PauseController pauseController, Transform enemiesContainer, TrainingController trainingController,
            IAudioService audioService)
        {
            _enemiesContainer = enemiesContainer;
            _levelsCreator = levelsCreator;
            _levelCompletionManagement = levelCompletionManagement;
            _pauseController = pauseController;
            _trainingController = trainingController;
            _audioService = audioService;

            _levelsCreator.ReadOnlyLevelCreated
                .Subscribe(levelObject => OnLevelCreated(levelObject))
                .AddTo(Disposables);

            _levelCompletionManagement.ReadOnlyLevelFailed
                .Subscribe(_ => CancelSpawnProcess())
                .AddTo(Disposables);

            _levelCompletionManagement.ReadOnlyLevelCompleted
                .Subscribe(_ => CancelSpawnProcess())
                .AddTo(Disposables);

            _pauseController.ReadOnlyOnOpenMenuButtonClicked
                .Subscribe(_ => CancelSpawnProcess())
                .AddTo(Disposables);
        }

        public void Dispose()
        {
            Disposables.Dispose();
            CancelSpawnProcess();
        }

        private void OnLevelCreated(LevelObject levelObject)
        {
            if (_trainingController.TutorialIsFinished == false)
            {
                _tutorialDisposable = _trainingController.OnTutorialFinished
                    .Subscribe(_ =>
                    {
                        _tutorialDisposable?.Dispose();
                        OnLevelCreated(levelObject);
                    });

                return;
            }

            CancelSpawnProcess();
            UpdatePoolsByLevel(levelObject);

            _currentMovingPoints = levelObject.MovingPoints.ToList();
            _currentEnemiesInLevelSpawnSequence = levelObject.EnemysInLevelSpawnSeqence;
            _currentSpawnEnemiesPoint = levelObject.NpcSpawnPoint;

            StartSpawnProcess();

        }
        private void UpdatePoolsByLevel(LevelObject levelObject)
        {
            foreach (var enemyGroupSettings in levelObject.EnemysInLevelSpawnSeqence.EnemyGroupsSpawnSettings)
            {
                foreach (var enemySpawnSettings in enemyGroupSettings.EnemySpawnSettings)
                {
                    Enemy enemyPrefab = enemySpawnSettings.EnemyPrefab;

                    if (ObjectPoolsByEnemy.ContainsKey(enemyPrefab.EnemyName) == false)
                        ObjectPoolsByEnemy.Add(enemyPrefab.EnemyName, new ObjectPoolWithQueue<Enemy>(enemyPrefab, _enemiesContainer));
                }
            }
        }

        private float GetSecondsUntilFirstWaveStart()
        {
            if (_currentEnemiesInLevelSpawnSequence == null)
                return 0f;

            var groups = _currentEnemiesInLevelSpawnSequence.EnemyGroupsSpawnSettings;

            if (groups == null || groups.Count == 0)
                return 0f;

            return Mathf.Max(0f, groups[0].SecondsDelayForSpawnAfterPreviousSpawn);
        }

        private async void RunTimerUntilFirstWaveStart(CancellationToken cancellationToken)
        {
            int secondsRemaining = Mathf.CeilToInt(GetSecondsUntilFirstWaveStart());

            SecondsRemainingUntilStartChanged?.OnNext(secondsRemaining);

            try
            {
                while (secondsRemaining > 0)
                {
                    await UniTask.Delay(1000, cancellationToken: cancellationToken);
                    secondsRemaining--;

                    SecondsRemainingUntilStartChanged?.OnNext(secondsRemaining);
                }
            }
            catch (OperationCanceledException ex)
            {
                // Its normal
            }
        }

        private int GetEnemiesCountToSpawn()
        {
            if (_currentEnemiesInLevelSpawnSequence == null)
                return 0;

            var groups = _currentEnemiesInLevelSpawnSequence.EnemyGroupsSpawnSettings;

            if (groups == null)
                return 0;

            int totalEnemiesCount = 0;

            foreach (var enemyGroupSettings in groups)
            {
                if (enemyGroupSettings.EnemySpawnSettings == null)
                    continue;

                foreach (var enemySpawnSettings in enemyGroupSettings.EnemySpawnSettings)
                    totalEnemiesCount += enemySpawnSettings.SpawnCount;
            }

            return totalEnemiesCount;
        }

        private void TryNotifyAllEnemiesDefeated()
        {
            if (_spawningProcessActive == false && EnemiesInLevel.Count == 0)
                AllEnemiesDefeated?.OnNext(Unit.Default);
        }

        private async void StartSpawnProcess()
        {
            if (Application.isPlaying == false)
            {
                Debug.LogWarning($"Cannot start process when application is not playing!");
                return;
            }

            if (_spawningProcessActive)
            {
                Debug.LogWarning("Spawn process already activated!");
                return;
            }

            CancelSpawnProcess();
            _spawnCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _spawnCancellationTokenSource.Token;

            Vector3[] movingPositions = new Vector3[_currentMovingPoints.Count];

            for (int i = 0; i < movingPositions.Length; i++)
                movingPositions[i] = _currentMovingPoints[i].position;

            int enemiesRemainingToSpawn = GetEnemiesCountToSpawn();

            _spawningProcessActive = true;

            RunTimerUntilFirstWaveStart(cancellationToken);

            if (enemiesRemainingToSpawn == 0)
            {
                _spawningProcessActive = false;
                TryNotifyAllEnemiesDefeated();
                return;
            }

            try
            {
                foreach (var enemysGroupSettings in _currentEnemiesInLevelSpawnSequence.EnemyGroupsSpawnSettings)
                {
                    await UniTask.Delay(
                        Mathf.RoundToInt(enemysGroupSettings.SecondsDelayForSpawnAfterPreviousSpawn * 1000),
                        cancellationToken: cancellationToken);

                    foreach (var enemySpawnSettings in enemysGroupSettings.EnemySpawnSettings)
                    {
                        for (int i = 0; i < enemySpawnSettings.SpawnCount; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            Enemy spawnedEnemy = ObjectPoolsByEnemy[enemySpawnSettings.EnemyPrefab.EnemyName].GetObject();
                            spawnedEnemy.transform.position = _currentSpawnEnemiesPoint.position;
                            spawnedEnemy.Initialize(movingPositions, _audioService);

                            EnemiesInLevel.Add(spawnedEnemy, new CompositeDisposable());

                            EnemiesInLevel[spawnedEnemy].Add(spawnedEnemy.OnDied.Subscribe(OnEnemyDied));
                            EnemiesInLevel[spawnedEnemy].Add(spawnedEnemy.OnMovedToLastPoint.Subscribe(OnEnemyMovedToLastPoint));

                            enemiesRemainingToSpawn--;

                            if (enemiesRemainingToSpawn == 0)
                            {
                                _spawningProcessActive = false;
                                TryNotifyAllEnemiesDefeated();
                                return;
                            }

                            try
                            {
                                await UniTask.Delay(
                                    Mathf.RoundToInt(enemySpawnSettings.SecondsDelayBetweenSpawn * 1000),
                                    cancellationToken: cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                _spawningProcessActive = false;
                                return;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Its normal
                return;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            _spawningProcessActive = false;
            TryNotifyAllEnemiesDefeated();
        }

        private void OnEnemyMovedToLastPoint(Enemy enemy)
        {
            OnEnemyDied(enemy);

            EnemyMovedToLastPoint?.OnNext(enemy);
        }

        private void OnEnemyDied(Enemy enemy)
        {
            enemy.Dispose();

            EnemiesInLevel[enemy].Dispose();
            EnemiesInLevel.Remove(enemy);

            EnemyDied?.OnNext(enemy);

            if (ObjectPoolsByEnemy[enemy.EnemyName].ContainsObject(enemy) == false)
                ObjectPoolsByEnemy[enemy.EnemyName].AddObject(enemy);

            TryNotifyAllEnemiesDefeated();
        }

        private void CancelSpawnProcess()
        {
            if (_spawnCancellationTokenSource == null)
                return;

            _spawnCancellationTokenSource.Cancel();
            _spawnCancellationTokenSource.Dispose();
            _spawnCancellationTokenSource = null;

            List<Enemy> enemiesInLevel = new();

            foreach (var keyValuePair in EnemiesInLevel)
                enemiesInLevel.Add(keyValuePair.Key);

            for (int i = 0; i < enemiesInLevel.Count; i++)
            {
                Enemy enemy = enemiesInLevel[i];

                if (enemy != null)
                {
                    enemy.Dispose();

                    EnemiesInLevel[enemy].Dispose();
                    EnemiesInLevel.Remove(enemy);

                    if (ObjectPoolsByEnemy[enemy.EnemyName].ContainsObject(enemy) == false)
                        ObjectPoolsByEnemy[enemy.EnemyName].AddObject(enemy);
                }
            }

            _spawningProcessActive = false;
            EnemiesInLevel.Clear();
        }
    }
}
