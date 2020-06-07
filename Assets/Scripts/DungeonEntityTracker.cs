// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;
using UnityEngine;

namespace RhythMage
{
    public class DungeonEntityTracker
    {
        public IReadOnlyDictionary<Cell, GameObject> ActiveEntities { get => m_activeEntities; }

        Dictionary<Cell, GameObject> m_activeEntities;
        ObjectPool<GameObject> m_entityPool;

        public DungeonEntityTracker()
        {
            m_activeEntities = new Dictionary<Cell, GameObject>();
            m_entityPool = new ObjectPool<GameObject>();
        }

        public bool Contains(Cell cell)
        {
            return m_activeEntities.ContainsKey(cell);
        }

        public int Count
        {
            get
            {
                return m_activeEntities.Count;
            }
        }

        public void AddToCell(Cell cell, GameObject entity)
        {
            entity.SetActive(true);
            m_activeEntities.Add(cell, entity);
        }

        public bool RemoveFromCell(Cell cell)
        {
            bool exists = m_activeEntities.TryGetValue(cell, out GameObject entity);
            if (exists)
            {
                entity.SetActive(false);
                m_entityPool.Add(entity);
                m_activeEntities.Remove(cell);
            }
            return exists;
        }

        public void RemoveAll()
        {
            foreach (var entry in m_activeEntities)
            {
                entry.Value.SetActive(false);
                m_entityPool.Add(entry.Value);
            }
            m_activeEntities.Clear();
        }

        public GameObject TryGetNext()
        {
            return m_entityPool.TryGet();
        }
    }
}
