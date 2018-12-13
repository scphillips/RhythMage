using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class DungeonBuilder : MonoBehaviour
    {
        [Zenject.Inject]
        readonly Settings m_settings;

        [Zenject.Inject]
        RandomNumberProvider m_rng;

        [System.Serializable]
        public class Settings
        {
            public int segmentCount;
            public int minSegmentLength;
            public int maxSegmentLength;
            public GameObject prefabFloor;
            public GameObject prefabWall;
        }

        enum Direction
        {
            Forwards,
            Right,
            Backwards,
            Left
        }

        Dictionary<Direction, CoordinateOffset> m_facings = new Dictionary<Direction, CoordinateOffset>()
        {
            { Direction.Forwards, CoordinateOffset.Create(0, 1) },
            { Direction.Right, CoordinateOffset.Create(1, 0) },
            { Direction.Backwards, CoordinateOffset.Create(0, -1) },
            { Direction.Left, CoordinateOffset.Create(-1, 0) }
        };

        HashSet<Cell> m_floorCells = new HashSet<Cell>();
        HashSet<Cell> m_wallCells = new HashSet<Cell>();

        Direction ChangeDirectionLeft(Direction currentDirection)
        {
            int dirInt = System.Convert.ToInt32(currentDirection);
            dirInt = (dirInt + m_facings.Count - 1) % m_facings.Count;
            return (Direction)dirInt;
        }

        Direction ChangeDirectionRight(Direction currentDirection)
        {
            int dirInt = System.Convert.ToInt32(currentDirection);
            dirInt = (dirInt + 1) % m_facings.Count;
            return (Direction)dirInt;
        }

        void AddCellAtPosition(int x, int y)
        {
            Cell cell;
            cell.x = x;
            cell.y = y;
            m_floorCells.Add(cell);

            // Fill all surrounding walls
            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    Cell wallCell;
                    wallCell.x = x + i;
                    wallCell.y = y + j;
                    m_wallCells.Add(wallCell);
                }
            }
        }

        void Start()
        {
            BuildDungeon();
        }

        void BuildDungeon()
        {
            // First generate path for the floor and block out all surrounding walls
            Direction currentDirection = Direction.Forwards;
            Cell currentPosition;
            currentPosition.x = 0;
            currentPosition.y = 0;
            for (int i = 0; i < m_settings.segmentCount; ++i)
            {
                CoordinateOffset offset;
                m_facings.TryGetValue(currentDirection, out offset);

                int length = m_rng.Next(m_settings.minSegmentLength, m_settings.maxSegmentLength);
                for (int j = 0; j < length; ++j)
                {
                    offset.Apply(ref currentPosition);
                    AddCellAtPosition(currentPosition.x, currentPosition.y);
                }

                if (i < m_settings.segmentCount - 1)
                {
                    int directionChange = m_rng.Next(2);
                    if (directionChange == 0)
                    {
                        currentDirection = ChangeDirectionLeft(currentDirection);
                    }
                    else if (directionChange == 1)
                    {
                        currentDirection = ChangeDirectionRight(currentDirection);
                    }
                }
            }

            // Generate floors, tunnelling through the walls with all overlapping cells
            foreach (var entry in m_floorCells)
            {
                CreateFloor(new Vector3(entry.x, 0, entry.y));
                m_wallCells.Remove(entry);
            }

            // Generate walls
            foreach (var entry in m_wallCells)
            {
                CreateWall(new Vector3(entry.x, 0, entry.y));
            }
        }
        
        void CreateFloor(Vector3 position)
        {
            var floor = (GameObject)Instantiate(m_settings.prefabFloor);
            floor.transform.SetParent(transform, false);
            floor.transform.localPosition = new Vector3(position.x, position.y - 0.5f, position.z);
        }

        void CreateWall(Vector3 position)
        {
            var wall = (GameObject)Instantiate(m_settings.prefabWall);
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition = position;
        }
    }
}
