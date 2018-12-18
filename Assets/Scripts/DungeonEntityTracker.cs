using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class DungeonEntityTracker
    {
        public Dictionary<Cell, GameObject> activeEntities;
        public ObjectPool<GameObject> entityPool;

        public DungeonEntityTracker()
        {
            activeEntities = new Dictionary<Cell, GameObject>();
            entityPool = new ObjectPool<GameObject>();
        }

        public bool Contains(Cell cell)
        {
            return activeEntities.ContainsKey(cell);
        }

        public int Count
        {
            get
            {
                return activeEntities.Count;
            }
        }

        public void AddToCell(Cell cell, GameObject entity)
        {
            entity.SetActive(true);
            activeEntities.Add(cell, entity);
        }

        public bool RemoveFromCell(Cell cell)
        {
            GameObject entity;
            bool exists = activeEntities.TryGetValue(cell, out entity);
            if (exists)
            {
                entity.SetActive(false);
                entityPool.Add(entity);
                activeEntities.Remove(cell);
            }
            return exists;
        }

        public void RemoveAll()
        {
            foreach (var entry in activeEntities)
            {
                entry.Value.SetActive(false);
                entityPool.Add(entry.Value);
            }
            activeEntities.Clear();
        }

        public GameObject TryGetNext()
        {
            return entityPool.TryGet();
        }
    }
}
