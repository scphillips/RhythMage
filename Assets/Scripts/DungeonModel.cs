// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;

namespace RhythMage
{
    public class DungeonModel
    {
        Dictionary<Cell, Enemy> m_enemies;
        public List<Cell> FloorCells { get; private set; }

        public event System.Action OnDungeonReset;
        public event System.Action<int> OnEnemyCountChange;

        public DungeonModel()
        {
            m_enemies = new Dictionary<Cell, Enemy>();
            FloorCells = new List<Cell>();
        }

        public void Reset()
        {
            m_enemies.Clear();
            FloorCells.Clear();
            OnDungeonReset?.Invoke();
        }

        public void AddToPath(Cell cell)
        {
            FloorCells.Add(cell);
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

        public Cell GetCellAtIndex(int index)
        {
            return FloorCells[index];
        }

        public int GetCellCount()
        {
            return FloorCells.Count;
        }
    }
}
