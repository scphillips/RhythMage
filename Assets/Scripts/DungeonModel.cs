using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class DungeonModel
    {
        Dictionary<Cell, Enemy> m_enemies;
        List<Cell> m_floorCells;

        public DungeonModel()
        {
            m_enemies = new Dictionary<Cell, Enemy>();
            m_floorCells = new List<Cell>();
        }

        public void SetPath(List<Cell> path)
        {
            m_floorCells = path;
        }

        public void AddToPath(Cell cell)
        {
            m_floorCells.Add(cell);
        }

        public void AddEnemyAtCell(Cell cell, Enemy enemy)
        {
            m_enemies.Add(cell, enemy);
        }

        public bool HasEnemyAtCell(Cell cell)
        {
            return m_enemies.ContainsKey(cell);
        }

        public Cell GetCellAtIndex(int index)
        {
            return m_floorCells[index];
        }

        public int GetCellCount()
        {
            return m_floorCells.Count;
        }
    }
}
