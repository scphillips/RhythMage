using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class DungeonModel
    {
        List<Cell> m_floorCells;

        public DungeonModel()
        {
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
