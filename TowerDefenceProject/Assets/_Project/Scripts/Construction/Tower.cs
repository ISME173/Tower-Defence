using _Project.Scripts.Audio;
using _Project.Scripts.EnemiesManagement;
using _Project.Scripts.Utilities;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Project.Scripts.Construction
{
    public abstract class Tower : MonoBehaviour, IDisposable
    {
        private readonly Dictionary<Enemy, IDisposable> EnemiesInAttackZone = new();

        [Header("References")]
        [SerializeField] private Trigger _attackZone;
        [SerializeField] private Transform _projectilePointInWeapon;

        [Header("Settings")]
        [SerializeField] private string _name;
        [SerializeField] private List<UpgradeLevelDatas> _upgradeLevelDatas;

        [Header("SFX")]
        [SerializeField] private AudioEvent _shootAudioEvent;

        private IAudioService _audioService;
        private int _upgradeLevelIndex;
        private TowerData _currentTowerData;
        private Enemy _targetEnemy;
        private float _attackDelayTimer;
        private ObjectPoolWithQueue<Projectile> _projectilesPool;

        protected ObjectPoolWithQueue<Projectile> ProjectilesPool => _projectilesPool;
        protected Transform ProjectilePointInWeapon => _projectilePointInWeapon;
        protected Enemy TargetEnemy => _targetEnemy;
        protected TowerData TowerData => _currentTowerData;

        public string Name => _name;
        public int BuildPrice => _upgradeLevelDatas[_upgradeLevelIndex].TowerData.BuildPrice;
        public int RefundAfterDestruction => _upgradeLevelDatas[_upgradeLevelIndex].TowerData.RefundAfterDestruction;
        public Sprite TowerIconSprite => _upgradeLevelDatas[_upgradeLevelIndex].TowerData.TowerIconSprite;

        private void OnValidate()
        {
            if (_upgradeLevelDatas == null || _upgradeLevelDatas.Count == 0)
                return;

            for (int i = 0; i < _upgradeLevelDatas.Count; i++)
            {
                TowerData towerData = _upgradeLevelDatas[i].TowerData;

                if (towerData != null && towerData.GetType() != GetTowerDataType())
                {
                    Debug.LogError($"Incorrect tower settings! Needed type: {GetTowerDataType()}. Current type: {towerData.GetType()}");
                    _upgradeLevelDatas.Clear();
                    return;
                }
            }
        }

        protected virtual void Update()
        {
            if (_targetEnemy == null || _targetEnemy.Transform == null)
                return;

            _attackDelayTimer += Time.deltaTime;

            if (_attackDelayTimer >= _currentTowerData.DelayBetweenAttacks)
            {
                _audioService.PlayOneShot(_shootAudioEvent);

                AttackEnemy(_targetEnemy);
                _attackDelayTimer = 0;
            }

            if (_targetEnemy != null)
                RotateWeaponToTarget(_targetEnemy.Transform);
        }

        public bool CanUpgrade()
        {
            return _upgradeLevelIndex + 1 < _upgradeLevelDatas.Count;
        }

        public int GetBuildPriceForNextLevel()
        {
            if (CanUpgrade() == false)
            {
                throw new Exception($"Tower '{gameObject.name}' already in last upgrade level!");
            }

            return _upgradeLevelDatas[_upgradeLevelIndex + 1].TowerData.BuildPrice;
        }

        public Sprite GetTowerIconForNextLevel()
        {
            if (CanUpgrade() == false)
            {
                throw new Exception($"Tower '{gameObject.name}' already in last upgrade level!");
            }

            return _upgradeLevelDatas[_upgradeLevelIndex + 1].TowerData.TowerIconSprite;
        }

        public virtual void Upgrade()
        {
            if (CanUpgrade() == false)
            {
                Debug.LogWarning($"Tower '{gameObject.name}' already in last upgrade level!");
                return;
            }

            _upgradeLevelDatas[_upgradeLevelIndex].UpgradeTowerView.SetActive(false);
            _upgradeLevelDatas[_upgradeLevelIndex + 1].UpgradeTowerView.SetActive(true);

            _currentTowerData = _upgradeLevelDatas[_upgradeLevelIndex + 1].TowerData;
            UpdateCurrentTowerData();

            _upgradeLevelIndex++;
        }

        public virtual void Initialize(IAudioService audioService)
        {
            _audioService ??= audioService;

            _upgradeLevelIndex = 0;
            _currentTowerData = _upgradeLevelDatas[_upgradeLevelIndex].TowerData;
            UpdateCurrentTowerData();

            _projectilesPool = new ObjectPoolWithQueue<Projectile>(_currentTowerData.ProjectilePrefab, transform);

            if (TowerData.GetType() != GetTowerDataType())
            {
                Debug.LogError($"Invalid tower data type: {TowerData.GetType()}. Needed type: {GetTowerDataType()}");
            }

            _attackZone.OnTriggerEnterEvent += OnTriggerEnterInAttackZone;
            _attackZone.OnTrggerExitEvent += OnTriggerExitFromAttackZone;

            UpdateWeaponProjectileView();
        }

        public virtual void Dispose()
        {
            _attackZone.OnTriggerEnterEvent -= OnTriggerEnterInAttackZone;
            _attackZone.OnTrggerExitEvent -= OnTriggerExitFromAttackZone;

            ClearSpawnedProjectiles();
            _targetEnemy = null;
        }

        protected abstract void ClearSpawnedProjectiles();
        protected abstract void UpdateCurrentTowerData();
        protected abstract Type GetTowerDataType();
        protected abstract void AttackEnemy(Enemy enemy);
        protected abstract void RotateWeaponToTarget(Transform targetTransform);
        protected abstract void UpdateWeaponProjectileView();

        private void OnTriggerEnterInAttackZone(Collider collider)
        {
            if (collider.TryGetComponent(out Enemy enemy))
            {
                if (EnemiesInAttackZone.ContainsKey(enemy) == false && enemy.Alive)
                {
                    EnemiesInAttackZone.Add(enemy, enemy.OnDied.Subscribe(OnDiedEnemy));

                    if (_targetEnemy == null)
                    {
                        _attackDelayTimer = 0;
                        _targetEnemy = enemy;
                    }
                }
            }
        }

        private void OnTriggerExitFromAttackZone(Collider collider)
        {
            if (collider.TryGetComponent(out Enemy enemy))
            {
                if (EnemiesInAttackZone.ContainsKey(enemy))
                {
                    EnemiesInAttackZone[enemy].Dispose();
                    EnemiesInAttackZone.Remove(enemy);

                    if (_targetEnemy == enemy)
                        _targetEnemy = EnemiesInAttackZone.FirstOrDefault().Key;

                    if (_targetEnemy == null)
                        UpdateWeaponProjectileView();
                }
            }
        }

        private void OnDiedEnemy(Enemy enemy)
        {
            if (EnemiesInAttackZone.ContainsKey(enemy))
            {
                EnemiesInAttackZone.Remove(enemy);

                if (enemy == _targetEnemy)
                    _targetEnemy = EnemiesInAttackZone.FirstOrDefault().Key;
            }
        }

        [Serializable]
        protected struct UpgradeLevelDatas
        {
            [SerializeField] private GameObject _upgradeTowerView;
            [Space]
            [SerializeField, Min(1)] private int _upgradeLevel;
            [SerializeField] private TowerData _towerData;

            public GameObject UpgradeTowerView => _upgradeTowerView;
            public int UpgradeLevel => _upgradeLevel;
            public TowerData TowerData => _towerData;
        }
    }
}
