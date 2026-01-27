using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Utilities
{
    public class ObjectPoolWithQueue<T> where T : MonoBehaviour
    {
        private readonly T Prefab;
        private readonly Queue<T> Queue;
        private Transform Container;

        public ObjectPoolWithQueue(T prefab, Transform container)
        {
            Prefab = prefab;
            Queue = new Queue<T>();
            Container = container;
        }

        public void AddObject(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(Container);
            Queue.Enqueue(obj);
        }

        public T GetObject()
        {
            T returnObject = null;

            if (Queue.Count > 0)
            {
                returnObject = Queue.Dequeue();
                returnObject.transform.SetParent(null);
                returnObject.gameObject.SetActive(true);
            }
            else
            {
                returnObject = GameObject.Instantiate(Prefab);
            }

            return returnObject;
        }
    }
}
