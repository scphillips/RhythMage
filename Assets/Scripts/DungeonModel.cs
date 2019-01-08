using System;
using System.Collections.Generic;

namespace Outplay.RhythMage
{
    public class DungeonModel
    {
        Dictionary<Cell, Enemy> m_enemies;
        public List<Cell> FloorCells { get; private set; }

        public event EventHandler OnDungeonReset;
        public event EventHandler OnEnemyCountChange;

        public DungeonModel()
        {
            m_enemies = new Dictionary<Cell, Enemy>();
            FloorCells = new List<Cell>();
        }

        public void Reset()
        {
            m_enemies.Clear();
            FloorCells.Clear();
            OnDungeonReset(this, null);
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
            OnEnemyCountChange(this, null);
        }

        public bool HasEnemyAtCell(Cell cell)
        {
            return m_enemies.ContainsKey(cell);
        }

        public Enemy GetEnemyAtCell(Cell cell)
        {
            Enemy enemy;
            m_enemies.TryGetValue(cell, out enemy);
            return enemy;
        }

        public bool RemoveEnemyAtCell(Cell cell)
        {
            bool success = m_enemies.Remove(cell);
            OnEnemyCountChange(this, null);
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
