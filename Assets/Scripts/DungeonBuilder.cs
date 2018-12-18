using System;
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
        
        DungeonEntityTracker braziers;
        DungeonEntityTracker enemies;
        DungeonEntityTracker floors;
        DungeonEntityTracker walls;

        void Start()
        {
            braziers = new DungeonEntityTracker();
            enemies = new DungeonEntityTracker();
            floors = new DungeonEntityTracker();
            walls = new DungeonEntityTracker();

            BuildDungeon();
        }

        public void BuildDungeon()
        {
            // Cleanup existing dungeon (if any)
            var trackers = new List<DungeonEntityTracker> { braziers, enemies, floors, walls };
            foreach (var entry in trackers)
            {
                entry.RemoveAll();
            }
            m_dungeon.Reset();

            // First generate path for the floor and block out all surrounding walls
            Direction currentDirection = Direction.Forwards;
            Cell currentPosition = Cell.zero;
            HashSet<Cell> wallCells = new HashSet<Cell>();
            AddPathAtCell(ref currentPosition, wallCells);

            for (int i = 0; i < m_settings.segmentCount; ++i)
            {
                CoordinateOffset offset;
                Defs.Facings.TryGetValue(currentDirection, out offset);

                int length = m_rng.Next(m_settings.minSegmentLength, m_settings.maxSegmentLength);
                for (int j = 0; j < length; ++j)
                {
                    offset.Apply(ref currentPosition);
                    AddPathAtCell(ref currentPosition, wallCells);
                }

                if (i < m_settings.segmentCount - 1)
                {
                    int directionChange = m_rng.Next(2) * 2 - 1;
                    currentDirection = ChangeDirection(currentDirection, directionChange);
                }
            }
            
            // Generate walls
            foreach (var entry in wallCells)
            {
                if (floors.Contains(entry) == false)
                {
                    CreateWall(entry);
                }
            }

            // Spawn braziers along the player path attached to walls
            for (int i = m_settings.brazierSpacing; i < m_dungeon.GetCellCount(); i += m_settings.brazierSpacing)
            {
                Cell cell = m_dungeon.GetCellAtIndex(i);
                CreateBrazier(cell);
            }

            // Find valid locations to spawn enemies
            float range = m_difficultySettings.maxEnemyPopulation - m_difficultySettings.minEnemyPopulation;
            float enemyPopulation = m_difficultySettings.minEnemyPopulation + m_rng.NextSingle() * range;
            int enemiesToSpawn = System.Convert.ToInt32(floors.Count * enemyPopulation);
            List<Cell> enemyLocationChoices = new List<Cell>();
            foreach (var entry in floors.activeEntities)
            {
                enemyLocationChoices.Add(entry.Key);
            }

            // Remove start and end tiles from list of location choices
            for (int i = 0; i < System.Math.Min(5, m_dungeon.GetCellCount()); ++i)
            {
                var cell = m_dungeon.GetCellAtIndex(i);
                enemyLocationChoices.Remove(cell);
            }
            for (int i = System.Math.Max(0, m_dungeon.GetCellCount() - 5); i < m_dungeon.GetCellCount(); ++i)
            {
                var cell = m_dungeon.GetCellAtIndex(i);
                enemyLocationChoices.Remove(cell);
            }

            // Spawn Enemies
            for (int i = 0; i < enemiesToSpawn; ++i)
            {
                int index = m_rng.Next(enemyLocationChoices.Count);
                var type = (Enemy.EnemyType)m_rng.Next(Enemy.enemyTypeCount);
                var enemy = m_enemyFactory.CreateEnemy(enemyLocationChoices[index], type);
                enemy.transform.SetParent(transform, false);
                m_dungeon.AddEnemyAtCell(enemyLocationChoices[index], enemy);
                enemyLocationChoices.RemoveAt(index);
            }
        }

        Direction ChangeDirection(Direction currentDirection, int change)
        {
            int dirInt = System.Convert.ToInt32(currentDirection);
            dirInt = (dirInt + Defs.Facings.Count + change) % Defs.Facings.Count;
            return (Direction)dirInt;
        }

        void AddPathAtCell(ref Cell cell, HashSet<Cell> wallCells)
        {
            CreateFloor(cell);
            m_dungeon.AddToPath(cell);

            // Fill all surrounding walls
            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    Cell wallCell;
                    wallCell.x = cell.x + i;
                    wallCell.y = cell.y + j;
                    wallCells.Add(wallCell);
                }
            }
        }

        GameObject CreateFloor(Cell cell)
        {
            GameObject floor = null;
            if (floors.Contains(cell) == false)
            {
                floor = floors.TryGetNext();
                if (floor == null)
                {
                    floor = Instantiate(m_settings.prefabFloor);
                }
                floor.transform.SetParent(transform, false);
                floor.transform.localPosition = new Vector3(cell.x, -0.5f, cell.y);
                floors.AddToCell(cell, floor);
            }
            return floor;
        }

        GameObject CreateWall(Cell cell)
        {
            GameObject wall = null;
            if (walls.Contains(cell) == false)
            {
                wall = walls.TryGetNext();
                if (wall == null)
                {
                    wall = Instantiate(m_settings.prefabWall);
                }
                wall.transform.SetParent(transform, false);
                wall.transform.localPosition = new Vector3(cell.x, 4.5f, cell.y);
                if (cell.x % 3 == 0 && cell.y % 3 == 0)
                {
                    wall.GetComponent<MeshRenderer>().material = m_settings.wallMaterials[0];
                }
                walls.AddToCell(cell, wall);
            }
            return wall;
        }

        GameObject CreateBrazier(Cell cell)
        {
            GameObject brazier = null;
            if (braziers.Contains(cell) == false)
            {
                // Find orthogonally adjacent walls (if any)
                List<Cell> adjacentWallCells = new List<Cell>();
                foreach (var entry in Defs.Facings)
                {
                    Cell test = cell + entry.Value;
                    if (walls.Contains(test))
                    {
                        adjacentWallCells.Add(test);
                    }
                }

                // Pick one wall to spawn a brazier
                if (adjacentWallCells.Count > 0)
                {
                    int index = m_rng.Next(adjacentWallCells.Count);
                    Cell wallCell = adjacentWallCells[index];
                    CoordinateOffset offset = CoordinateOffset.Create(wallCell.x - cell.x, wallCell.y - cell.y);
                    Direction direction = Defs.GetOffsetDirection(ref offset);

                    brazier = braziers.TryGetNext();
                    if (brazier == null)
                    {
                        brazier = Instantiate(m_settings.prefabBrazier);
                    }
                    brazier.transform.SetParent(transform, false);
                    float angle = 90.0f * (System.Convert.ToInt32(direction) - 1);
                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                    brazier.transform.localPosition = (rotation * new Vector3(0.25f, 0.75f, 0.0f)) + new Vector3(cell.x, 0.0f, cell.y);
                    brazier.transform.localRotation = rotation;
                    braziers.AddToCell(cell, brazier);
                }
            }
            return brazier;
        }
    }
}
