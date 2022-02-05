// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, January 2022

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RhythMage
{
    public class LevelBuilder
    {
        [System.Serializable]
        public class Settings
        {
            [Range(0.0f, 1.0f)]
            public float maxRoomDensity;

            [Range(2, 15)]
            public int minRoomSize;

            [Range(2, 15)]
            public int maxRoomSize;

            [Range(1, 100)]
            public int minRoomCount;

            [Range(1, 100)]
            public int maxRoomCount;

            [Range(0, 100)]
            public int fixedRoomCount;

            [Range(50, 500)]
            public int maxLevelSize;

            public int levelSeed;

            public Material regionDebugOutlineMaterial;
        }

        [Zenject.Inject]
        readonly Settings m_settings;

        [Zenject.Inject]
        readonly DungeonBuilder.Settings m_dungeonSettings;

        [Zenject.Inject]
        RandomNumberProvider m_rng;

        private List<GameObject> m_entities;

        public LevelBuilder()
        {
            m_entities = new List<GameObject>();
        }

        public void BuildLevel(DungeonModel dungeon, Transform rootTransform)
        {
            // Cleanup existing dungeon (if any)
            dungeon.Reset();
            if (m_settings.levelSeed != -1)
            {
                m_rng.SetSeed(m_settings.levelSeed);
            }

            foreach (GameObject entity in m_entities)
            {
                entity.transform.DetachChildren();
                Object.Destroy(entity);
            }
            m_entities.Clear();
            
            List<Room> allRooms = new List<Room>();

            List<Region> allRegions = new List<Region>();
            // Create full level as one region
            Region firstRegion = new Region(0, 0, m_settings.maxLevelSize, m_settings.maxLevelSize);

            // Determine number of rooms to fill level with
            int roomCount = m_settings.fixedRoomCount;
            if (roomCount < 1)
            {
                m_rng.Next(m_settings.minRoomCount, m_settings.maxRoomCount + 1);
            }

            // Set up first room
            Room firstRoom = TryAddRoomToRegion(null, Direction.Forward, firstRegion, allRooms, allRegions);
            Room previousRoom = firstRoom;
            bool hasReversedRooms = false;
            // Place remaining rooms - start at index 1
            for (int i = 1; i < roomCount; ++i)
            {
                // Pick direction from previous room based on available connected regions
                int startIndex = m_rng.Next(previousRoom.connections.Count);
                bool placedRoom = false;
                for (int j = 0; j < previousRoom.connections.Count && placedRoom == false; ++j)
                {
                    int connectionIndex = (j + startIndex) % previousRoom.connections.Count;
                    var (direction, region) = previousRoom.connections[connectionIndex];
                    if (region != null && !(region is Room) && region.Enabled)
                    {
                        Room newRoom = TryAddRoomToRegion(previousRoom, direction, region, allRooms, allRegions);
                        if (newRoom != null)
                        {
                            previousRoom = newRoom;

                            // Disable regions which are too small to house connections
                            foreach (var (connectedDirection, connectedRegion) in newRoom.connections)
                            {
                                if (!(connectedRegion is Room) && connectedRegion.Enabled)
                                {
                                    DisableSmallRegions(connectedRegion, connectedDirection);
                                }
                            }
                            placedRoom = true;
                        }
                    }
                }

                if (!placedRoom)
                {
                    if (!hasReversedRooms)
                    {
                        // Reverse the list of rooms built so far and continue from the first room
                        hasReversedRooms = true;
                        allRooms.Reverse();
                        for (int j = 0; j < i; ++j)
                        {
                            allRooms[j].index = j;
                        }
                        previousRoom = firstRoom;
                        --i;
                    }
                    else
                    {
                        Debug.Log(string.Format("Failed to place room after {0}", previousRoom));
                        break;
                    }
                }
            }

            Dictionary<Region, GameObject> regionEntities = new Dictionary<Region, GameObject>();

            int worldOffsetX = firstRoom.origin.x - firstRoom.size.x / 2;
            int worldOffsetZ = firstRoom.origin.y - firstRoom.size.y / 2;
            
            // Show placeholder doors where rooms join
            for (int i = 0; i < allRooms.Count - 1; ++i)
            {
                Room current = allRooms[i];
                Room next = allRooms[i + 1];
                Direction adjacencyDirection = TryGetAdjacency(current, next);
                bool isVertical = adjacencyDirection == Direction.Forward || adjacencyDirection == Direction.Backward;
                int coordX = 0;
                int coordZ = 0;
                if (adjacencyDirection == Direction.Forward)
                {
                    coordZ = current.Front;
                }
                else if (adjacencyDirection == Direction.Backward)
                {
                    coordZ = current.Back - 1;
                }
                else if (adjacencyDirection == Direction.Left)
                {
                    coordX = current.Left - 1;
                }
                else if (adjacencyDirection == Direction.Right)
                {
                    coordX = current.Right;
                }

                if (isVertical)
                {
                    int xMin = System.Math.Max(current.Left, next.Left);
                    int xMax = System.Math.Min(current.Right, next.Right);
                    coordX = m_rng.Next(xMin, xMax);
                }
                else
                {
                    int zMin = System.Math.Max(current.Back, next.Back);
                    int zMax = System.Math.Min(current.Front, next.Front);
                    coordZ = m_rng.Next(zMin, zMax);
                }

                Cell doorLocation = Cell.Create(coordX, coordZ);
                GameObject doorEntity = dungeon.Doors.GetOrCreateAtCell(doorLocation, m_dungeonSettings.prefabFloor);
                doorEntity.name = string.Format("Doorway [{0}:{1}]", current.index, next.index);
                Transform doorTransform = doorEntity.transform;
                doorTransform.SetParent(rootTransform, false);
                doorTransform.localPosition = new Vector3(coordX, 0.0f, coordZ);
                current.Doorways.Add((doorLocation, next));
                next.Doorways.Add((doorLocation, current));
            }

            HashSet<Cell> placedWalls = new HashSet<Cell>();
            foreach (Room room in allRooms)
            {
                GameObject roomEntity = new GameObject(string.Format("Room {0}", room));
                m_entities.Add(roomEntity);
                Transform roomTransform = roomEntity.transform;
                roomTransform.SetParent(rootTransform, false);
                roomTransform.localPosition = new Vector3(room.origin.x, 0.0f, room.origin.y);

                foreach (Cell cell in room.Cells)
                {
                    // Build floor within room
                    GameObject floor = dungeon.Floors.GetOrCreateAtCell(cell, m_dungeonSettings.prefabFloor);
                    floor.name = string.Format("Floor {0}", cell);
                    Transform transform = floor.transform;
                    transform.SetParent(roomTransform, false);
                    transform.localPosition = new Vector3(cell.x - room.origin.x, 0.0f, cell.y - room.origin.y);
                    if (m_dungeonSettings.corridorFloorMaterials.Count > 0)
                    {
                        var renderer = floor.GetComponentInChildren<MeshRenderer>();
                        renderer.material = m_rng.Pick(m_dungeonSettings.corridorFloorMaterials);
                    }
                }

                // Spawn walls around room
                for (int i = 0; i <= room.Width; ++i)
                {
                    for (int j = 0; j <= room.Depth; ++j)
                    {
                        if (i == 0 || j == 0 || i == room.Width || j == room.Depth)
                        {
                            Cell cell = Cell.Create(room.Left + i - 1, room.Back + j - 1);
                            if (!dungeon.Doors.Contains(cell) && !placedWalls.Contains(cell))
                            {
                                placedWalls.Add(cell);
                                GameObject wall = dungeon.Walls.GetOrCreateAtCell(cell, m_dungeonSettings.prefabWall);
                                wall.name = string.Format("Wall {0}", cell);
                                Transform transform = wall.transform;
                                transform.SetParent(roomTransform, false);
                                transform.localPosition = new Vector3(cell.x - room.origin.x, 0.0f, cell.y - room.origin.y);
                                transform.localScale = new Vector3(1.0f, 0.1f, 1.0f);
                                if (m_dungeonSettings.wallMaterials.Count > 0)
                                {
                                    var renderer = wall.GetComponentInChildren<MeshRenderer>();
                                    renderer.material = m_rng.Pick(m_dungeonSettings.wallMaterials);
                                }
                            }
                        }
                    }
                }

                regionEntities[room] = roomEntity;
            }

            //for (int i = 0; i < allRooms.Count - 1; ++i)
            //{
            //    Room current = allRooms[i];
            //    Room next = allRooms[i + 1];
            //    Direction adjacencyDirection = TryGetAdjacency(current, next);
            //    GameObject doorEntity = dungeon.Doors.ActiveEntities[doorLocations[i]];
            //    ConnectionListDisplay connectionList = doorEntity.AddComponent<ConnectionListDisplay>();
            //    connectionList.SetConnections(new List<(Direction, GameObject)> { (Defs.InverseDirection(adjacencyDirection), regionEntities[current]), (adjacencyDirection, regionEntities[next]) });
            //}

            // Show debug outline of all regions
            //allRegions.Sort((lhs, rhs) => (lhs.origin.x * firstRegion.size.y + lhs.origin.y).CompareTo(rhs.origin.x * firstRegion.size.y + rhs.origin.y));
            //foreach (Region region in allRegions.Where(item => !(item is Room)))
            //{
            //    GameObject regionOutline = new GameObject(string.Format("Region {0}", region));
            //    m_entities.Add(regionOutline);
            //    LineRenderer renderer = regionOutline.AddComponent<LineRenderer>();
            //    renderer.material = m_settings.regionDebugOutlineMaterial;
            //    float posX = region.origin.x + (region.size.x - 1) * 0.5f - 0.5f;
            //    float posZ = region.origin.y + (region.size.y - 1) * 0.5f - 0.5f;
            //    float left = posX - region.size.x * 0.5f;
            //    float back = posZ - region.size.y * 0.5f;
            //    float right = left + region.size.x;
            //    float front = back + region.size.y;
            //    float posY = region.Enabled ? 0.0f : -1.0f;
            //    Vector3[] positions = new Vector3[4] { new Vector3(left, posY, back), new Vector3(right, posY, back), new Vector3(right, posY, front), new Vector3(left, posY, front) };
            //    renderer.positionCount = 4;
            //    renderer.loop = true;
            //    renderer.startWidth = renderer.endWidth = 0.25f;
            //    renderer.SetPositions(positions);
            //    renderer.startColor = renderer.endColor = region.Enabled ? new Color(1.0f, 0.0f, 1.0f) : new Color(0.25f, 0.25f, 0.5f);

            //    Transform transform = regionOutline.transform;
            //    transform.SetParent(rootTransform, false);
            //    transform.localPosition = new Vector3(posX, 0.0f, posZ);
            //    transform.localScale = new Vector3(region.size.x, 1.0f, region.size.y);

            //    regionEntities[region] = regionOutline;
            //}

            //foreach (Region region in allRegions)
            //{
            //    GameObject entity = regionEntities[region];
            //    ConnectionListDisplay connectionList = entity.AddComponent<ConnectionListDisplay>();
            //    connectionList.SetConnections(region.connections.Select(entry => (entry.Item1, regionEntities[entry.Item2])).ToList());
            //}
        }

        private void DisableSmallRegions(Region region, Direction direction)
        {
            AggregateRegion testRegions = new AggregateRegion(region);
            FindRegionsInDirection(region, direction, testRegions);
            FindRegionsInDirection(region, Defs.RotateDirection(direction, RotationDirection.CounterClockwise), testRegions);
            FindRegionsInDirection(region, Defs.RotateDirection(direction, RotationDirection.Clockwise), testRegions);
            bool hasEnoughSpace = testRegions.Width >= m_settings.minRoomSize && testRegions.Depth >= m_settings.minRoomSize;
            region.Enabled = hasEnoughSpace;

            if (hasEnoughSpace == false)
            {
                // Find and disable any connections which have no enabled connections
                foreach (var (connectedDirection, connectedRegion) in region.connections)
                {
                    bool shouldTest = !(connectedRegion is Room) && connectedRegion.Enabled;
                    if (shouldTest)
                    {
                        connectedRegion.Enabled = connectedRegion.connections.Any(entry => !(entry.Item2 is Room) && entry.Item2.Enabled);
                        if (connectedRegion.Enabled)
                        {
                            DisableSmallRegions(connectedRegion, connectedDirection);
                        }
                    }
                }
            }
        }

        private Room TryAddRoomToRegion(Room previousRoom, Direction fromDirection, Region currentRegion, List<Room> allRooms, List<Region> allRegions)
        {
            // Aggregate regions in chosen direction, plus orthogonal regions cw and ccw from it
            AggregateRegion targetRegions = new AggregateRegion(currentRegion);
            FindOrthogonalRegions(currentRegion, fromDirection, targetRegions);

            int maxWidth = System.Math.Min(targetRegions.size.x, m_settings.maxRoomSize);
            int maxDepth = System.Math.Min(targetRegions.size.y, m_settings.maxRoomSize);
            if (maxWidth < m_settings.minRoomSize || maxDepth < m_settings.minRoomSize)
            {
                return null;
            }

            int roomWidth = m_rng.Next(m_settings.minRoomSize, System.Math.Min(targetRegions.size.x, m_settings.maxRoomSize) + 1);
            int roomDepth = m_rng.Next(m_settings.minRoomSize, System.Math.Min(targetRegions.size.y, m_settings.maxRoomSize) + 1);
            int originX = 0;
            int originY = 0;
            if (previousRoom == null)
            {
                // Place first room in the centre of level
                originX = (targetRegions.size.x - roomWidth) / 2;
                originY = (targetRegions.size.y - roomDepth) / 2;
            }
            else
            {
                // Compute range of fixed coordinates to sit adjacent to previous room
                bool isVertical = fromDirection == Direction.Forward || fromDirection == Direction.Backward;
                if (isVertical)
                {
                    int xMin = System.Math.Max(previousRoom.Left - roomWidth + 2, targetRegions.Left);
                    int xMax = System.Math.Min(previousRoom.Right - 1, targetRegions.Right - roomWidth + 1);
                    if (xMin > xMax)
                    {
                        return null;
                    }
                    originX = m_rng.Next(xMin, xMax);
                }
                else
                {
                    int zMin = System.Math.Max(previousRoom.Back - roomDepth + 2, targetRegions.Back);
                    int zMax = System.Math.Min(previousRoom.Front - 1, targetRegions.Front - roomDepth + 1);
                    if (zMin > zMax)
                    {
                        return null;
                    }
                    originY = m_rng.Next(zMin, zMax);
                }

                if (fromDirection == Direction.Forward)
                {
                    originY = previousRoom.Front + 1;
                }
                else if (fromDirection == Direction.Backward)
                {
                    originY = previousRoom.Back - roomDepth;
                }
                else if (fromDirection == Direction.Left)
                {
                    originX = previousRoom.Left - roomWidth;
                }
                else if (fromDirection == Direction.Right)
                {
                    originX = previousRoom.Right + 1;
                }
            }
            Room room = new Room(originX, originY, roomWidth, roomDepth, allRooms.Count);
            AddRoom(room, targetRegions, allRooms, allRegions);
            return room;
        }

        private void FindOrthogonalRegions(Region sourceRegion, Direction direction, AggregateRegion foundRegions)
        {
            Direction cwDirection = Defs.RotateDirection(direction, RotationDirection.Clockwise);
            FindRegionsInDirection(sourceRegion, cwDirection, foundRegions);
            Direction ccwDirection = Defs.RotateDirection(direction, RotationDirection.CounterClockwise);
            FindRegionsInDirection(sourceRegion, ccwDirection, foundRegions);
            // Iterate through all found regions and traverse connected regions in original direction
            int regionCount = foundRegions.Count;
            bool repeat = true;
            List<Region> frontierRegions = new List<Region>(foundRegions.regions);
            List<Region> candidateRegions = new List<Region>();
            while (repeat)
            {
                candidateRegions.Clear();
                for (int i = 0; i < regionCount; ++i)
                {
                    var currentRegion = frontierRegions[i];
                    bool foundAny = false;
                    foreach (var (connectedDirection, region) in currentRegion.connections)
                    {
                        if (connectedDirection == direction && !(region is Room))
                        {
                            if (foundAny == true)
                            {
                                // Multiple subdivisions out of line with current candidate
                                repeat = false;
                                break;
                            }
                            foundAny = true;
                            candidateRegions.Add(region);
                        }
                    }
                }

                repeat = repeat && candidateRegions.Count == regionCount;
                if (repeat)
                {
                    foundRegions.AddRange(candidateRegions);
                    frontierRegions.Clear();
                    frontierRegions.InsertRange(0, candidateRegions);
                }
            }
        }

        private void FindRegionsInDirection(Region sourceRegion, Direction direction, AggregateRegion foundRegions)
        {
            // Step through all regions in chosen direction
            foreach (var (connectedDirection, region) in sourceRegion.connections)
            {
                if (connectedDirection == direction && !(region is Room) && region.Enabled)
                {
                    foundRegions.Add(region);
                    FindRegionsInDirection(region, direction, foundRegions);
                }
            }
        }

        private void AddRoom(Room room, AggregateRegion aggregateRegion, List<Room> allRooms, List<Region> allRegions)
        {
            SubdivideRegionsAroundRoom(aggregateRegion, room, allRegions);
            allRooms.Add(room);
        }

        private bool RoomIntersectsRegion(Room room, Region region)
        {
            return room.Right >= region.Left && room.Left <= region.Right
                && room.Front >= region.Back && room.Back <= region.Front;
        }

        private void SubdivideRegionsAroundRoom(AggregateRegion aggregateRegion, Room room, List<Region> allRegions)
        {
            List<Region> intersectingRegions = aggregateRegion.regions.Where(entry => RoomIntersectsRegion(room, entry)).ToList();
            AggregateRegion subsetRegion = new AggregateRegion();
            subsetRegion.AddRange(intersectingRegions);

            // Generate new regions from each corner and side of room
            List<Region> newRegions = new List<Region>();
            // Keep fixed array coordinates for simpler adjacency checks below
            Region[] newRegionsFixed = new Region[9];
            int[] xCoords = new int[4] { subsetRegion.Left, room.Left, room.Right + 1, subsetRegion.Right + 1 };
            int[] zCoords = new int[4] { subsetRegion.Back, room.Back, room.Front + 1, subsetRegion.Front + 1 };

            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    int index = i * 3 + j;
                    if (i == 1 && j == 1)
                    {
                        newRegionsFixed[index] = room;
                        newRegions.Add(room);
                    }
                    else
                    {
                        newRegionsFixed[index] = CreateRegionIfValid(xCoords[i], zCoords[j], xCoords[i + 1] - xCoords[i], zCoords[j + 1] - zCoords[j], newRegions);
                    }
                }
            }

            // Generate connections for new regions
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    // All orthogonally adjacent regions are one step away
                    int regionIndex = i * 3 + j;
                    if (newRegionsFixed[regionIndex] != null)
                    {
                        int nextIndexH = regionIndex + 1;
                        int nextIndexV = regionIndex + 3;
                        if (j < 2 && newRegionsFixed[nextIndexH] != null)
                        {
                            ConnectRegions(newRegionsFixed[regionIndex], newRegionsFixed[nextIndexH]);
                        }
                        if (i < 2 && newRegionsFixed[nextIndexV] != null)
                        {
                            ConnectRegions(newRegionsFixed[regionIndex], newRegionsFixed[nextIndexV]);
                        }
                    }
                }
            }

            AddRegionRangeToList(newRegions, allRegions);

            // Stitch new regions into previously connected regions
            foreach (Region intersectingRegion in intersectingRegions)
            {
                RemoveRegion(intersectingRegion, allRegions);
                var intersectionConnections = intersectingRegion.connections.ToList();
                foreach (var (connectedDirection, connectedRegion) in intersectionConnections)
                {
                    // Ignore regions we intersected with - they are being reconstructed
                    if (intersectingRegions.Contains(connectedRegion) == false)
                    {
                        // Find newly created regions adjacent to connected region
                        List<Region> adjacentRegions = new List<Region>();
                        foreach (Region newRegion in newRegions)
                        {
                            Direction foundDirection = TryGetAdjacency(newRegion, connectedRegion);
                            if (foundDirection != Direction.None)
                            {
                                Debug.AssertFormat(connectedDirection == foundDirection, "Mismatched adjacency from {0} to {1} in direction {2}, expected {3}", newRegion, connectedRegion, foundDirection, connectedDirection);
                                adjacentRegions.Add(newRegion);
                            }
                        }
                        int adjacentRegionsCount = adjacentRegions.Count;
                        if (adjacentRegionsCount < 2 || connectedRegion is Room)
                        {
                            for (int i = 0; i < adjacentRegionsCount; ++i)
                            {
                                ConnectRegions(adjacentRegions[i], connectedRegion, connectedDirection);
                            }
                        }
                        else
                        {
                            SubdivideRegionToMatchAdjacentRegionsInDirection(connectedRegion, adjacentRegions, connectedDirection, allRegions);
                        }
                    }
                }
            }

            // Traverse new regions according to their relative direction around new room
            // First merge any convergent regions with no remaining subdivisions around rooms
            // Then subdivide to match any remaining connection divisions
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    int regionIndex = i * 3 + j;
                    Region region = newRegionsFixed[regionIndex];
                    if (region != null)
                    {
                        Direction direction = Direction.None;
                        if (i == 0)
                        {
                            direction = Direction.Left;
                        }
                        else if (i == 2)
                        {
                            direction = Direction.Right;
                        }

                        if (j == 0)
                        {
                            direction = Direction.Backward;
                        }
                        else if (j == 2)
                        {
                            direction = Direction.Forward;
                        }

                        if (direction != Direction.None)
                        {
                            MergeAdjacentRegionsInDirection(region, direction, allRegions);
                            SubdivideRegionToMatchAdjacentRegionsInDirection(region, direction, allRegions);
                        }
                    }
                }
            }
        }

        private void MergeAdjacentRegionsInDirection(Region sourceRegion, Direction direction, List<Region> allRegions)
        {
            AggregateRegion mergeCandidates = new AggregateRegion();
            bool allowedToMerge = true;
            foreach (var (connectedDirection, connectedRegion) in sourceRegion.connections)
            {
                if (connectedDirection == direction)
                {
                    if (connectedRegion is Room || DoesRegionHaveAnyRoomsInDirection(connectedRegion, direction))
                    {
                        allowedToMerge = false;
                        break;
                    }
                    else
                    {
                        mergeCandidates.Add(connectedRegion);
                    }
                }
            }

            if (allowedToMerge && mergeCandidates.regions.Count > 1)
            {
                bool isVertical = direction == Direction.Forward || direction == Direction.Backward;
                int sourceComponent = isVertical ? sourceRegion.Width : sourceRegion.Depth;
                int mergeComponent = isVertical ? mergeCandidates.Width : mergeCandidates.Depth;
                Region mergedRegion = new Region(mergeCandidates.Left, mergeCandidates.Back, mergeCandidates.Width, mergeCandidates.Depth);
                AddRegionToList(mergedRegion, allRegions);
                HashSet<Region> seenRegions = new HashSet<Region>();
                foreach (var candidate in mergeCandidates.regions)
                {
                    foreach (var (connectedDirection, connectedRegion) in candidate.connections)
                    {
                        // Ensure each connection is only counted once
                        if (!seenRegions.Contains(connectedRegion) && !mergeCandidates.regions.Contains(connectedRegion))
                        {
                            seenRegions.Add(connectedRegion);
                            ConnectRegions(mergedRegion, connectedRegion, connectedDirection);
                        }
                    }
                    RemoveRegion(candidate, allRegions);
                }
                
                // Inspect further adjacent regions for merging
                MergeAdjacentRegionsInDirection(mergedRegion, direction, allRegions);
            }
        }

        private void FindRegionsToSubdivideAlong(Region region, Direction direction, Dictionary<int, System.ValueTuple<Region, Region>> coordinatesToSubdivide)
        {
            List<Region> foundRegions = new List<Region>();
            foreach (var (connectedDirection, connectedRegion) in region.connections)
            {
                if (connectedDirection == direction)
                {
                    foundRegions.Add(connectedRegion);
                }
            }

            FindCoordinatesToSubdivideAlong(region, direction, foundRegions, coordinatesToSubdivide);
        }

        private void FindCoordinatesToSubdivideAlong(Region region, Direction direction, List<Region> adjacentRegions, Dictionary<int, System.ValueTuple<Region, Region>> coordinatesToSubdivide)
        {
            bool isVertical = direction == Direction.Forward || direction == Direction.Backward;

            foreach (var adjacentRegion in adjacentRegions)
            {
                if (isVertical)
                {
                    if (region.Left < adjacentRegion.Left && adjacentRegion.Left <= region.Right)
                    {
                        if (!coordinatesToSubdivide.ContainsKey(adjacentRegion.Left))
                        {
                            coordinatesToSubdivide.Add(adjacentRegion.Left, (null, adjacentRegion));
                        }
                        else
                        {
                            var pair = coordinatesToSubdivide[adjacentRegion.Left];
                            pair.Item2 = adjacentRegion;
                            coordinatesToSubdivide[adjacentRegion.Left] = pair;
                        }
                    }
                    else if (region.Left <= adjacentRegion.Right && adjacentRegion.Right < region.Right)
                    {
                        if (!coordinatesToSubdivide.ContainsKey(adjacentRegion.Right + 1))
                        {
                            coordinatesToSubdivide.Add(adjacentRegion.Right + 1, (adjacentRegion, null));
                        }
                        else
                        {
                            var pair = coordinatesToSubdivide[adjacentRegion.Right + 1];
                            pair.Item1 = adjacentRegion;
                            coordinatesToSubdivide[adjacentRegion.Right + 1] = pair;
                        }
                    }
                }
                else
                {
                    if (region.Back < adjacentRegion.Back && adjacentRegion.Back <= region.Front)
                    {
                        if (!coordinatesToSubdivide.ContainsKey(adjacentRegion.Back))
                        {
                            coordinatesToSubdivide.Add(adjacentRegion.Back, (null, adjacentRegion));
                        }
                        else
                        {
                            var pair = coordinatesToSubdivide[adjacentRegion.Back];
                            pair.Item2 = adjacentRegion;
                            coordinatesToSubdivide[adjacentRegion.Back] = pair;
                        }
                    }
                    else if (region.Back <= adjacentRegion.Front && adjacentRegion.Front < region.Front)
                    {
                        if (!coordinatesToSubdivide.ContainsKey(adjacentRegion.Front + 1))
                        {
                            coordinatesToSubdivide.Add(adjacentRegion.Front + 1, (adjacentRegion, null));
                        }
                        else
                        {
                            var pair = coordinatesToSubdivide[adjacentRegion.Front + 1];
                            pair.Item1 = adjacentRegion;
                            coordinatesToSubdivide[adjacentRegion.Front + 1] = pair;
                        }
                    }
                }
            }
        }

        private void SubdivideRegionToMatchAdjacentRegionsInDirection(Region region, Direction direction, List<Region> allRegions)
        {
            Dictionary<int, System.ValueTuple<Region, Region>> coordinatesToSubdivide = new Dictionary<int, (Region, Region)>();
            FindRegionsToSubdivideAlong(region, direction, coordinatesToSubdivide);
            if (coordinatesToSubdivide.Any())
            {
                // Enumerate in ascending order so we can treat second element of each subdivision as next source
                var sortedKeys = coordinatesToSubdivide.Keys.ToList();
                sortedKeys.Sort();
                Region regionToSubdivide = region;
                foreach (var coordinate in sortedKeys)
                {
                    var (lhs, rhs) = coordinatesToSubdivide[coordinate];
                    var (a, b) = SubdivideRegionsInDirectionAlongCoordinate(regionToSubdivide, direction, coordinate, allRegions);
                    regionToSubdivide = b;
                }
            }
        }

        private void SubdivideRegionToMatchAdjacentRegionsInDirection(Region region, List<Region> adjacentRegions, Direction adjacencyDirection, List<Region> allRegions)
        {
            Debug.AssertFormat(!(region is Room), "Attempting to subdivide room {0}", region);
            Region regionToSubdivide = region;
            for (int i = 0; i < adjacentRegions.Count - 1; ++i)
            {
                Region adjacentRegionA = adjacentRegions[i];
                Region adjacentRegionB = adjacentRegions[i + 1];
                int coordinate = 0;
                if (adjacencyDirection == Direction.Forward || adjacencyDirection == Direction.Backward)
                {
                    // Find x coordinate to subdivide along
                    coordinate = (adjacentRegionA.Left == adjacentRegionB.Right + 1) ? adjacentRegionA.Left : adjacentRegionA.Right + 1;
                    Debug.AssertFormat(coordinate == adjacentRegionB.Left || coordinate == adjacentRegionB.Right + 1, "Failed to find X coordinate between regions {0} and {1}", adjacentRegionA, adjacentRegionB);
                }
                else
                {
                    // Find z coordinate to subdivide along
                    coordinate = (adjacentRegionA.Back == adjacentRegionB.Front + 1) ? adjacentRegionA.Back : adjacentRegionA.Front + 1;
                    Debug.AssertFormat(coordinate == adjacentRegionB.Back || coordinate == adjacentRegionB.Front + 1, "Failed to find Z coordinate between regions {0} and {1}", adjacentRegionA, adjacentRegionB);
                }
                var (a, b) = SubdivideRegionsInDirectionAlongCoordinate(regionToSubdivide, adjacencyDirection, coordinate, allRegions);

                ConnectRegions(adjacentRegionA, a, adjacencyDirection);
                ConnectRegions(adjacentRegionB, b, adjacencyDirection);
                regionToSubdivide = b;
            }
        }

        private System.ValueTuple<Region, Region> SubdivideRegionsInDirectionAlongCoordinate(Region region, Direction direction, int coordinate, List<Region> allRegions)
        {
            RemoveRegion(region, allRegions);
            var (a, b) = SubdivideRegion(region, direction, coordinate);
            AddRegionToList(a, allRegions);
            AddRegionToList(b, allRegions);
            Direction invDirection = Defs.InverseDirection(direction);
            var connectionsCopy = region.connections.ToList();
            int connectionsCount = connectionsCopy.Count;
            for (int i = 0; i < connectionsCount; ++i)
            {
                var (connectedDirection, connectedRegion) = connectionsCopy[i];
                if (connectedDirection == direction)
                {
                    if (!(connectedRegion is Room) && ShouldSubdivideRegionAlongCoordinate(connectedRegion, direction, coordinate))
                    {
                        // Subdivide region and update connections
                        var (otherA, otherB) = SubdivideRegionsInDirectionAlongCoordinate(connectedRegion, direction, coordinate, allRegions);
                        ConnectRegions(a, otherA, direction);
                        ConnectRegions(b, otherB, direction);
                    }
                    else
                    {
                        // Update room connections
                        TryConnectRegions(a, connectedRegion);
                        TryConnectRegions(b, connectedRegion);
                    }
                }
            }
            return (a, b);
        }

        private bool ShouldSubdivideRegionAlongCoordinate(Region region, Direction direction, int coordinate)
        {
            bool isVertical = direction == Direction.Forward || direction == Direction.Backward;
            return (isVertical && coordinate > region.Left && coordinate <= region.Right)
                || (!isVertical && coordinate > region.Back && coordinate <= region.Front);
        }

        private System.ValueTuple<Region, Region> SubdivideRegion(Region region, Direction direction, int coordinate)
        {
            bool isVertical = direction == Direction.Forward || direction == Direction.Backward;
            int coordXPos = isVertical ? coordinate : region.Left;
            int coordZPos = isVertical ? region.Back : coordinate;
            int coordXSizeA = isVertical ? coordinate - region.Left : region.Width;
            int coordZSizeA = isVertical ? region.Depth : coordinate - region.Back;
            int coordXSizeB = isVertical ? region.Width - coordXSizeA : coordXSizeA;
            int coordZSizeB = isVertical ? coordZSizeA : region.Depth - coordZSizeA;
            Region a = new Region(region.Left, region.Back, coordXSizeA, coordZSizeA);
            Region b = new Region(coordXPos, coordZPos, coordXSizeB, coordZSizeB);
            ConnectRegions(a, b, isVertical ? Direction.Right : Direction.Forward);
            ApplyConnectionsToSubregions(region, a, b, direction);
            return (a, b);
        }

        private bool DoesRegionHaveAnyRoomsInDirection(Region region, Direction direction)
        {
            bool found = false;
            foreach (var (connectedDirection, connectedRegion) in region.connections)
            {
                if (connectedDirection == direction)
                {
                    if (connectedRegion is Room || DoesRegionHaveAnyRoomsInDirection(connectedRegion, direction))
                    {
                        found = true;
                        break;
                    }
                }
            }
            return found;
        }

        private void ConnectRegions(Region lhs, Region rhs, Direction direction = Direction.None)
        {
            bool success = TryConnectRegions(lhs, rhs, direction);
            Debug.AssertFormat(success, "Failed to connect regions {0} and {1}", lhs, rhs);
        }

        private bool TryConnectRegions(Region lhs, Region rhs, Direction direction = Direction.None)
        {
            if (lhs != null && rhs != null)
            {
                Direction adjacency = direction;
                if (adjacency == Direction.None)
                {
                    adjacency = TryGetAdjacency(lhs, rhs);
                }
                else
                {
                    Debug.AssertFormat(direction == TryGetAdjacency(lhs, rhs), "Unexpected connection direction {0} between {1} and {2}, expected {3}", direction, lhs, rhs, TryGetAdjacency(lhs, rhs));
                }

                if (adjacency != Direction.None)
                {
                    Direction invAdjacency = Defs.InverseDirection(adjacency);
                    if (!lhs.connections.Any(entry => entry.Item2 == rhs))
                    {
                        lhs.connections.Add((adjacency, rhs));
                    }
                    if (!rhs.connections.Any(entry => entry.Item2 == lhs))
                    {
                        rhs.connections.Add((invAdjacency, lhs));
                    }
                    return true;
                }
            }
            return false;
        }

        private void RemoveRegion(Region region, List<Region> allRegions)
        {
            foreach (var (direction, adjacentRegion) in region.connections)
            {
                RemoveConnection(adjacentRegion, region);
            }
            allRegions.Remove(region);
        }

        private void RemoveConnection(Region fromRegion, Region otherRegion)
        {
            int connectionsCount = fromRegion.connections.Count;
            for (int i = connectionsCount - 1; i >= 0; --i)
            {
                var (connectedDirection, connectedRegion) = fromRegion.connections[i];
                if (connectedRegion == otherRegion)
                {
                    fromRegion.connections.RemoveAt(i);
                    break;
                }
            }
        }

        private void ApplyConnectionsToSubregions(Region region, Region subregionA, Region subregionB, Direction direction)
        {
            foreach (var (adjacencyDirection, adjacentRegion) in region.connections)
            {
                if (Defs.IsOrthogonal(adjacencyDirection, direction))
                {
                    if (adjacencyDirection == Direction.Backward || adjacencyDirection == Direction.Left)
                    {
                        ConnectRegions(subregionA, adjacentRegion, adjacencyDirection);
                    }
                    else
                    {
                        ConnectRegions(subregionB, adjacentRegion, adjacencyDirection);
                    }
                }
                else if (adjacencyDirection != direction)
                {
                    bool connectedA = TryConnectRegions(subregionA, adjacentRegion);
                    bool connectedB = TryConnectRegions(subregionB, adjacentRegion);
                    Debug.AssertFormat(connectedA || connectedB, "Failed to connect subregions {0} or {1} to adjacent region {2}", subregionA, subregionB, adjacentRegion);
                }
            }
        }

        private Region CreateRegionIfValid(int x, int y, int w, int d, List<Region> regions)
        {
            if (w > 0 && d > 0)
            {
                Region region = new Region(x, y, w, d);
                regions.Add(region);
                return region;
            }
            return null;
        }

        private Direction TryGetAdjacency(Region source, Region other)
        {
            if (source.Left <= other.Right && source.Right >= other.Left)
            {
                if (source.Front == other.Back - 1) return Direction.Forward;
                if (source.Back == other.Front + 1) return Direction.Backward;
            }
            else if (source.Front >= other.Back && source.Back <= other.Front)
            {
                if (source.Left == other.Right + 1) return Direction.Left;
                if (source.Right == other.Left - 1) return Direction.Right;
            }
            
            return Direction.None;
        }

        private void AddRegionToList(Region newRegion, List<Region> allRegions)
        {
            Debug.AssertFormat(!allRegions.Contains(newRegion), "Adding region {0} multiple times", newRegion);
            Debug.AssertFormat(newRegion.Width > 0 && newRegion.Depth > 0, "Adding invalid region {0}", newRegion);
            allRegions.Add(newRegion);
        }

        private void AddRegionRangeToList(IEnumerable<Region> newRegions, List<Region> allRegions)
        {
            foreach (Region newRegion in newRegions)
            {
                AddRegionToList(newRegion, allRegions);
            }
        }
    }
}
