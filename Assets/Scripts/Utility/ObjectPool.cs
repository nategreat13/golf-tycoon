using System.Collections.Generic;
using UnityEngine;

namespace GolfGame.Utility
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> pool = new();
        private readonly List<T> active = new();

        public ObjectPool(T prefab, Transform parent, int initialSize = 5)
        {
            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                CreateNew();
            }
        }

        public T Get()
        {
            T item;
            if (pool.Count > 0)
            {
                item = pool.Dequeue();
            }
            else
            {
                item = CreateNew();
            }

            item.gameObject.SetActive(true);
            active.Add(item);
            return item;
        }

        public void Return(T item)
        {
            item.gameObject.SetActive(false);
            active.Remove(item);
            pool.Enqueue(item);
        }

        public void ReturnAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                Return(active[i]);
            }
        }

        private T CreateNew()
        {
            T item = Object.Instantiate(prefab, parent);
            item.gameObject.SetActive(false);
            pool.Enqueue(item);
            return item;
        }
    }
}
