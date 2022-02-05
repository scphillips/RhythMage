// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RhythMage
{
    public class DungeonBuilder : MonoBehaviour
    {
        [System.Serializable]
        public class Settings
        {
            public int segmentWidth;
            public int segmentDepth;
            public int minSegmentConnectionDistance;
            public int minStepCountInSegment;

            public int minSegmentLength;
            public int maxSegmentLength;
            public int brazierSpacing;

            public GameObject prefabBrazier;
            public GameObject prefabFloor;
            public GameObject prefabPortal;
            public GameObject prefabWall;

            public List<Material> corridorFloorMaterials;
            public List<Material> roomFloorMaterials;
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
        Enemy.Factory m_enemyFactory;

        [Zenject.Inject]
        SoundManager m_sound;


        void Start()
        {
            BuildDungeon();
        }

        public void BuildDungeon()
        {
            // Cleanup existing dungeon (if any)
            m_dungeon.Reset();
            
            int tileCount = m_sound.GetTotalBeatsInTrack();

            // First generate path for the floor and block out all surrounding walls
            Direction currentDirection = Direction.Forward;
            Cell currentPosition = Cell.Zero;

            HashSet<Cell> wallCells = new HashSet<Cell>();
            AddCorridorFloorAtCell(in currentPosition, wallCells);
            m_dungeon.AddToPath(currentPosition);

            // Build path to traverse dungeon up to tileCount
            while (m_dungeon.GetCellCount() < tileCount)
            {
                Defs.facings.TryGetValue(currentDirection, out CoordinateOffset offset);

                int length = m_rng.Next(m_settings.minSegmentLength, m_settings.maxSegmentLength);
                length = System.Math.Min(length, tileCount - m_dungeon.GetCellCount());
                for (int i = 0; i < length; ++i)
                {
                    offset.ApplyTo(ref currentPosition);
                    AddCorridorFloorAtCell(in currentPosition, wallCells);
                    m_dungeon.AddToPath(currentPosition);
                }
                
                int directionChange = m_rng.Next(2) * 2 - 1;
                var nextDirection = ChangeDirection(currentDirection, directionChange);
                // Test if end cell will land in on existing path
                if (WillIntersectPortal(tileCount, currentPosition, nextDirection))
                {
                    nextDirection = ChangeDirection(currentDirection, -directionChange);
                }
                if (WillIntersectPortal(tileCount, currentPosition, nextDirection) == false)
                {
                    currentDirection = nextDirection;
                }
            }

            // Find valid locations to spawn enemies
            float range = m_difficultySettings.maxEnemyPopulation - m_difficultySettings.minEnemyPopulation;
            float enemyPopulation = m_difficultySettings.minEnemyPopulation + m_rng.NextSingle() * range;
            List<Cell> enemyLocationChoices = new List<Cell>();
            foreach (var entry in m_dungeon.Floors.ActiveEntities)
            {
                enemyLocationChoices.Add(entry.Key);
            }

            // Remove start and end tiles from list of location choices
            for (int i = 0; i < System.Math.Min(3, m_dungeon.GetCellCount()); ++i)
            {
                var cell = m_dungeon.GetPathAtIndex(i);
                enemyLocationChoices.Remove(cell);
            }
            for (int i = System.Math.Max(0, m_dungeon.GetCellCount() - 3); i < m_dungeon.GetCellCount(); ++i)
            {
                var cell = m_dungeon.GetPathAtIndex(i);
                enemyLocationChoices.Remove(cell);
            }

            // Spawn Enemies
            int enemiesToSpawn = System.Convert.ToInt32(m_dungeon.Floors.Count * enemyPopulation);
            enemiesToSpawn = System.Math.Min(enemiesToSpawn, enemyLocationChoices.Count);

            List<Cell> targetCells = new List<Cell>();
            for (int i = 0; i < enemiesToSpawn; ++i)
            {
                int index = m_rng.Next(enemyLocationChoices.Count);
                var cell = enemyLocationChoices[index];
                targetCells.Add(cell);
                enemyLocationChoices.RemoveAt(index);
            }

            targetCells.Sort((Cell lhs, Cell rhs) =>
            {
                int leftIndex = m_dungeon.Path.IndexOf(lhs);
                int rightIndex = m_dungeon.Path.IndexOf(rhs);
                return leftIndex.CompareTo(rightIndex);
            });

            foreach (Cell cell in targetCells)
            {
                var type = (EnemyType)m_rng.Next(Defs.enemyTypeCount);
                var enemy = CreateEnemy(cell, type);
                int cellIndex = m_dungeon.Path.IndexOf(cell);
                enemy.name = "Enemy" + enemy.EnemyType.ToString() + " [" + cellIndex + "]";
            }

            List<Cell> roomLocationChoices = new List<Cell>();
            foreach (var entry in m_dungeon.Floors.ActiveEntities)
            {
                roomLocationChoices.Add(entry.Key);
            }

            List<SegmentModelDef> segments = new List<SegmentModelDef>();
            // Decide where to build rooms around path
            while (roomLocationChoices.Any())
            {
                int roomOriginIndex = m_rng.Next(roomLocationChoices.Count);
                Cell roomOrigin = roomLocationChoices[roomOriginIndex];
                int segmentWidth = m_rng.Next(2, 4) * 2 + 1;
                int segmentDepth = m_rng.Next(2, 4) * 2 + 1;

                int insetCorners = m_rng.Next(System.Math.Min(segmentWidth, segmentDepth) / 2 + 1);
                SegmentModelDef segment = new SegmentModelDef(roomOrigin, segmentWidth, segmentDepth, insetCorners);
                CoordinateOffset roomCentre = CoordinateOffset.Create(segment.width / 2, segment.depth / 2);
                foreach (Cell roomCell in segment.Cells)
                {
                    Cell coord = roomOrigin + CoordinateOffset.Distance(Cell.Zero, roomCell - roomCentre);
                    AddRoomFloorAtCell(coord, wallCells);
                }

                // Remove all room placement candidates that may overlap this one
                for (int i = roomLocationChoices.Count - 1; i >= 0; --i)
                {
                    Cell cell = roomLocationChoices[i];
                    CoordinateOffset distance = CoordinateOffset.Distance(roomOrigin, cell);
                    if (distance.x < 8 + segmentWidth - System.Math.Min(System.Math.Abs(distance.y), insetCorners) && distance.y < 8 + segmentDepth - System.Math.Min(System.Math.Abs(distance.x), insetCorners))
                    {
                        roomLocationChoices.RemoveAt(i);
                    }
                }

                segments.Add(segment);
            }

            // Generate walls
            foreach (var entry in wallCells)
            {
                if (m_dungeon.Floors.Contains(entry) == false)
                {
                    CreateWall(entry);
                }
            }

            // Spawn braziers along the player path attached to walls
            for (int i = m_settings.brazierSpacing; i < m_dungeon.GetCellCount(); i += m_settings.brazierSpacing)
            {
                Cell cell = m_dungeon.GetPathAtIndex(i);
                CreateBrazier(cell);
            }

            // Spawn Portal at start and end of dungeon
            if (m_dungeon.GetCellCount() > 0)
            {
                CreatePortal(m_dungeon.Path.First(), Direction.Forward);
                CreatePortal(m_dungeon.Path.Last(), currentDirection);
            }
        }

        bool WillIntersectPortal(int tileCount, Cell currentCell, Direction direction)
        {
            int tilesRemaining = tileCount - m_dungeon.GetCellCount();
            Defs.facings.TryGetValue(direction, out CoordinateOffset offset);
            for (int i = 0; i < tilesRemaining; ++i)
            {
                offset.ApplyTo(ref currentCell);
                if (currentCell == m_dungeon.Path.First())
                {
                    return true;
                }
            }
            offset.x *= tilesRemaining;
            offset.y *= tilesRemaining;
            offset.ApplyTo(ref currentCell);
            return m_dungeon.Path.Contains(currentCell);
        }

        Direction ChangeDirection(Direction currentDirection, int change)
        {
            int dirInt = System.Convert.ToInt32(currentDirection);
            dirInt = (dirInt + Defs.facings.Count + change) % Defs.facings.Count;
            return (Direction)dirInt;
        }

        GameObject AddCorridorFloorAtCell(in Cell cell, HashSet<Cell> wallCells)
        {
            return AddFloorAtCell(cell, wallCells, m_settings.corridorFloorMaterials);
        }

        GameObject AddRoomFloorAtCell(in Cell cell, HashSet<Cell> wallCells)
        {
            return AddFloorAtCell(cell, wallCells, m_settings.roomFloorMaterials);
        }

        GameObject AddFloorAtCell(in Cell cell, HashSet<Cell> wallCells, IReadOnlyList<Material> floorMaterialsList)
        {
            // Fill all surrounding walls
            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    wallCells.Add(Cell.Create(cell.x + i, cell.y + j));
                }
            }

            GameObject floor = CreateFloor(cell);
            if (floorMaterialsList.Count > 0)
            {
                int materialIndex = m_rng.Next(floorMaterialsList.Count);
                var renderer = floor.GetComponentInChildren<MeshRenderer>();
                renderer.material = floorMaterialsList[materialIndex];
            }
            return floor;
        }

        GameObject CreateFromPool(Cell cell, DungeonEntityTracker pool, GameObject prefab)
        {
            GameObject entity = null;
            if (pool.Contains(cell) == false)
            {
                entity = pool.TryGetNext();
                if (entity == null)
                {
                    entity = Instantiate(prefab);
                }
                entity.transform.SetParent(transform, false);
                entity.transform.localPosition = new Vector3(cell.x, 0.0f, cell.y);
                pool.AddToCell(cell, entity);
            }
            else
            {
                pool.ActiveEntities.TryGetValue(cell, out entity);
            }

            return entity;
        }

        GameObject CreateFloor(Cell cell)
        {
            return CreateFromPool(cell, m_dungeon.Floors, m_settings.prefabFloor);
        }

        GameObject CreatePortal(Cell cell, Direction direction)
        {
            var portal = CreateFromPool(cell, m_dungeon.Portals, m_settings.prefabPortal);
            float angle = 90.0f * System.Convert.ToInt32(direction);
            portal.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.up);
            return portal;
        }

        GameObject CreateWall(Cell cell)
        {
            GameObject wall = CreateFromPool(cell, m_dungeon.Walls, m_settings.prefabWall);
            if (wall != null)
            {
                int materialIndex = (cell.x % 3 == 0 && cell.y % 3 == 0) ? 0 : 1;
                wall.GetComponentInChildren<MeshRenderer>().material = m_settings.wallMaterials[materialIndex];
            }
            return wall;
        }

        Enemy CreateEnemy(Cell cell, EnemyType type)
        {
            Enemy enemy = null;
            var gameObject = m_dungeon.Enemies.TryGetNext();
            if (gameObject != null)
            {
                enemy = gameObject.GetComponent<Enemy>();
                enemy.EnemyType = type;
                enemy.Reset(cell);
            }
            else
            {
                enemy = m_enemyFactory.Create(cell, type);
            }
            enemy.transform.SetParent(transform, false);
            m_dungeon.AddEnemyAtCell(cell, enemy);
            m_dungeon.Enemies.AddToCell(cell, enemy.gameObject);
            return enemy;
        }

        GameObject CreateBrazier(Cell cell)
        {
            GameObject brazier = null;
            if (m_dungeon.Braziers.Contains(cell) == false)
            {
                // Find orthogonally adjacent walls (if any)
                List<Cell> adjacentWallCells = new List<Cell>();
                foreach (var entry in Defs.facings)
                {
                    Cell test = cell + entry.Value;
                    if (m_dungeon.Walls.Contains(test))
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
                    Direction direction = Defs.GetOffsetDirection(in offset);

                    brazier = m_dungeon.Braziers.TryGetNext();
                    if (brazier == null)
                    {
                        brazier = Instantiate(m_settings.prefabBrazier);
                    }
                    brazier.transform.SetParent(transform, false);
                    float angle = 90.0f * System.Convert.ToInt32(direction);
                    brazier.transform.localPosition = new Vector3(cell.x, 0.0f, cell.y);
                    brazier.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.up);
                    m_dungeon.Braziers.AddToCell(cell, brazier);
                }
            }
            return brazier;
        }
    }
}
