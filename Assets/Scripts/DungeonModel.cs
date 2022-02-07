// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;

namespace RhythMage
{
    public class DungeonModel
    {
        Dictionary<Cell, Enemy> m_enemies = new Dictionary<Cell, Enemy>();
        public List<Cell> Path { get; private set; } = new List<Cell>();

        public DungeonEntityTracker Braziers { get; } = new DungeonEntityTracker();
        public DungeonEntityTracker Doors { get; } = new DungeonEntityTracker();
        public DungeonEntityTracker Enemies { get; } = new DungeonEntityTracker();
        public DungeonEntityTracker Floors { get; } = new DungeonEntityTracker();
        public DungeonEntityTracker Portals { get; } = new DungeonEntityTracker();
        public DungeonEntityTracker Walls { get; } = new DungeonEntityTracker();

        public event System.Action OnDungeonReset;
        public event System.Action OnPathChanged;
        public event System.Action<int> OnEnemyCountChange;

        IEnumerable<DungeonEntityTracker> AllTrackers
        {
            get
            {
                yield return Braziers;
                yield return Doors;
                yield return Enemies;
                yield return Floors;
                yield return Portals;
                yield return Walls;
            }
        }

        public void Reset()
        {
            foreach (var tracker in AllTrackers)
            {
                tracker.RemoveAll();
            }

            m_enemies.Clear();
            Path.Clear();
            OnDungeonReset?.Invoke();
        }

        public void SetPath(IList<Cell> path)
        {
            Path.AddRange(path);
            OnPathChanged?.Invoke();
        }

        public int GetEnemyCount()
        {
            return m_enemies.Count;
        }

        public void AddEnemyAtCell(Cell cell, Enemy enemy)
        {
            m_enemies.Add(cell, enemy);
            OnEnemyCountChange?.Invoke(m_enemies.Count);
        }

        public bool HasEnemyAtCell(Cell cell)
        {
            return m_enemies.ContainsKey(cell);
        }

        public bool GetEnemyAtCell(Cell cell, out Enemy enemy)
        {
            return m_enemies.TryGetValue(cell, out enemy);
        }

        public bool RemoveEnemyAtCell(Cell cell)
        {
            bool success = m_enemies.Remove(cell);
            OnEnemyCountChange?.Invoke(m_enemies.Count);
            return success;
        }

        public Cell GetPathAtIndex(int index)
        {
            return (index < Path.Count) ? Path[index] : Cell.Zero;
        }

        public int GetCellCount()
        {
            return Path.Count;
        }
    }
}
