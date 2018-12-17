using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class DungeonBuilder : MonoBehaviour
    {
        [System.Serializable]
        public class Settings
        {
            public int segmentCount;
            public int minSegmentLength;
            public int maxSegmentLength;
            public int brazierSpacing;

            public GameObject prefabBrazier;
            public GameObject prefabFloor;
            public GameObject prefabWall;

            public List<Material> wallMaterials;
        }

        [Zenject.Inject]
        readonly Settings m_settings;

        [Zenject.Inject]
        readonly GameDifficulty.Settings m_difficultySettings;

        [Zenject.Inject]
        RandomNumberProvider m_rng;

        [Zenject.Inject]
        DungeonModel m_dungeon;

        [Zenject.Inject]
        EnemyFactory m_enemyFactory;

        HashSet<Cell> m_brazierCells = new HashSet<Cell>();
        HashSet<Cell> m_floorCells = new HashSet<Cell>();
        HashSet<Cell> m_wallCells = new HashSet<Cell>();

        Dictionary<Defs.Direction, CoordinateOffset> m_ordinals = new Dictionary<Defs.Direction, CoordinateOffset>
        {
            { Defs.Direction.Forwards, CoordinateOffset.Create(0, 1) },
            { Defs.Direction.Right, CoordinateOffset.Create(1, 0) },
            { Defs.Direction.Backwards, CoordinateOffset.Create(0, -1) },
            { Defs.Direction.Left, CoordinateOffset.Create(-1, 0) }
        };

        Defs.Direction ChangeDirectionLeft(Defs.Direction currentDirection)
        {
            int dirInt = System.Convert.ToInt32(currentDirection);
            dirInt = (dirInt + Defs.Facings.Count - 1) % Defs.Facings.Count;
            return (Defs.Direction)dirInt;
        }

        Defs.Direction ChangeDirectionRight(Defs.Direction currentDirection)
        {
            int dirInt = System.Convert.ToInt32(currentDirection);
            dirInt = (dirInt + 1) % Defs.Facings.Count;
            return (Defs.Direction)dirInt;
        }

        void AddCellAtPosition(int x, int y)
        {
            Cell cell;
            cell.x = x;
            cell.y = y;
            m_floorCells.Add(cell);
            m_dungeon.AddToPath(cell);

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
            Defs.Direction currentDirection = Defs.Direction.Forwards;
            Cell currentPosition;
            currentPosition.x = 0;
            currentPosition.y = 0;
            AddCellAtPosition(currentPosition.x, currentPosition.y);

            for (int i = 0; i < m_settings.segmentCount; ++i)
            {
                CoordinateOffset offset;
                Defs.Facings.TryGetValue(currentDirection, out offset);

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
                CreateFloor(entry);
                m_wallCells.Remove(entry);
            }

            // Generate walls
            foreach (var entry in m_wallCells)
            {
                CreateWall(entry);
            }

            // Find valid locations to spawn enemies
            float range = m_difficultySettings.maxEnemyPopulation - m_difficultySettings.minEnemyPopulation;
            float enemyPopulation = m_difficultySettings.minEnemyPopulation + System.Convert.ToSingle(m_rng.NextDouble()) * range;
            int enemiesToSpawn = System.Convert.ToInt32(m_floorCells.Count * enemyPopulation);
            List<Cell> enemyLocationChoices = new List<Cell>();
            foreach (var cell in m_floorCells)
            {
                enemyLocationChoices.Add(cell);
            }

            // Remove starting tiles from list of location choices
            for (int i = 0; i < 5; ++i)
            {
                var cell = m_dungeon.GetCellAtIndex(i);
                enemyLocationChoices.Remove(cell);
            }

            // Spawn braziers along the player path attached to walls
            for (int i = m_settings.brazierSpacing; i < m_dungeon.GetCellCount(); i += m_settings.brazierSpacing)
            {
                Cell floorCell = m_dungeon.GetCellAtIndex(i);
                // Check we have not already added this cell
                if (m_brazierCells.Contains(floorCell))
                {
                    continue;
                }

                // Do not test this cell again
                m_brazierCells.Add(floorCell);

                // Find orthogonal walls (if any)
                List<Cell> wallCells = new List<Cell>();
                foreach (var entry in m_ordinals)
                {
                    Cell test = floorCell;
                    entry.Value.Apply(ref test);
                    if (m_wallCells.Contains(test))
                    {
                        wallCells.Add(test);
                    }
                }

                // Pick one wall to spawn a brazier
                if (wallCells.Count > 0)
                {
                    int index = m_rng.Next(wallCells.Count);
                    Cell wallCell = wallCells[index];
                    CoordinateOffset offset = CoordinateOffset.Create(wallCell.x - floorCell.x, wallCell.y - floorCell.y);
                    Defs.Direction direction = Defs.GetOffsetDirection(ref offset);
                    GameObject brazier = Instantiate(m_settings.prefabBrazier);
                    brazier.transform.SetParent(transform, false);
                    float angle = 90.0f * (System.Convert.ToInt32(direction) - 1);
                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                    brazier.transform.localPosition = (rotation * brazier.transform.localPosition) + new Vector3(floorCell.x, 0, floorCell.y);
                    brazier.transform.localRotation = rotation;
                }
            }
            
            // Spawn Enemies
            for (int i = 0; i < enemiesToSpawn; ++i)
            {
                int index = m_rng.Next(enemyLocationChoices.Count);
                var type = (m_rng.Next(2) == 0) ? Enemy.EnemyType.Magic : Enemy.EnemyType.Melee;
                var enemy = m_enemyFactory.CreateEnemy(enemyLocationChoices[index], type);
                enemy.transform.SetParent(transform, false);
                m_dungeon.AddEnemyAtCell(enemyLocationChoices[index], enemy);
                enemyLocationChoices.RemoveAt(index);
            }
        }
        
        void CreateFloor(Cell cell)
        {
            var floor = (GameObject)Instantiate(m_settings.prefabFloor);
            floor.transform.SetParent(transform, false);
            floor.transform.localPosition += new Vector3(cell.x, 0, cell.y);
        }

        void CreateWall(Cell cell)
        {
            GameObject wall = Instantiate(m_settings.prefabWall);
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition += new Vector3(cell.x, 0, cell.y);
            if (cell.x % 3 == 0 && cell.y % 3 == 0)
            {
                wall.GetComponent<MeshRenderer>().material = m_settings.wallMaterials[0];
            }
        }
    }
}
