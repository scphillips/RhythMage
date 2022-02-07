// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, February 2022

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RhythMage
{
    public class PathBuilder
    {
        [System.Serializable]
        public class Settings
        {
            [Range(0.0f, 1.0f)]
            public float maxRoomDensity;
        }

        [Zenject.Inject]
        readonly Settings m_settings;

        [Zenject.Inject]
        readonly LevelBuilder.Settings m_levelBuilderSettings;

        [Zenject.Inject]
        readonly DungeonAmbientController.Settings m_dungeonAmbientSettings;

        [Zenject.Inject]
        readonly RandomNumberProvider m_rng;

        [Zenject.Inject(Id = "dungeon_root")] readonly Transform m_dungeonRoot;

        private GameObject m_pathOutline;

        public void BuildPath(DungeonModel dungeon, List<Room> allRooms, List<Cell> waypoints)
        {
            Debug.AssertFormat(allRooms.Any(), "Attempting to build path with no rooms");
            List<Cell> fullPath = new List<Cell>();
            Object.Destroy(m_pathOutline);
            m_pathOutline = new GameObject(string.Format("Path"));
            // Pick random entry node for first room
            var (firstNode, firstDirection) = PickRandomEntryNode(allRooms.First());
            fullPath.Add(firstNode);
            HashSet<Cell> visitedCells = new HashSet<Cell>();
            List<Cell> currentWaypoints = new List<Cell>();
            Cell entryNode = firstNode;
            Direction entryDirection = firstDirection;
            for (int i = 0; i < allRooms.Count; ++i)
            {
                Room currentRoom = allRooms[i];
                visitedCells.Clear();
                
                Cell lastCell = entryNode;
                Direction lastDirection = entryDirection;
                currentWaypoints.Clear();
                GenerateWaypointNodesForRoom(currentRoom, currentWaypoints);

                // Arrange waypoints according to most optimal route
                SortForShortestRoute(currentWaypoints, entryNode);
                waypoints.AddRange(currentWaypoints);

                // Perform pathfinding through list of waypoints
                for (int j = 0; j < currentWaypoints.Count; ++j)
                {
                    Cell chosenCell = currentWaypoints[j];
                    (lastCell, lastDirection) = BuildPathBetweenNodes(lastCell, chosenCell, lastDirection, currentRoom, fullPath);
                }

                if (i < allRooms.Count - 1)
                {
                    var (exitNode, exitDirection) = FindDoorwayLeadingToNextRoom(currentRoom);
                    (lastCell, lastDirection) = BuildPathBetweenNodes(lastCell, exitNode, lastDirection, currentRoom, fullPath);

                    // Extend path to cross boundary into next room
                    Cell positionAfterMove = lastCell + Defs.GetFacing(exitDirection);
                    fullPath.Add(positionAfterMove);
                    entryNode = positionAfterMove;
                    entryDirection = exitDirection;
                }
            }
            dungeon.SetPath(fullPath);

            //Debug.Log(string.Format("Generated {0} steps in path", fullPath.Count));
            //LineRenderer renderer = m_pathOutline.AddComponent<LineRenderer>();
            //renderer.material = m_levelBuilderSettings.regionDebugOutlineMaterial;
            //int index = 0;
            //Direction previousDirection = firstDirection;
            //Vector3[] pathLines = fullPath.Select(entry => GetLineRendererCoordinate(entry, fullPath, ref index, ref previousDirection)).ToArray();
            //renderer.positionCount = fullPath.Count;
            //renderer.startWidth = renderer.endWidth = 0.125f;
            //renderer.SetPositions(pathLines);
            //renderer.startColor = renderer.endColor = new Color(1.0f, 1.0f, 0.0f);
            //m_pathOutline.transform.SetParent(m_dungeonRoot, false);
            //
            //foreach (Cell waypoint in waypoints)
            //{
            //    SpriteRenderer node = Object.Instantiate(m_dungeonAmbientSettings.tilePulsePrefab, m_pathOutline.transform);
            //    node.name = string.Format("Node {0}", waypoint);
            //    node.transform.localPosition = new Vector3(waypoint.x, 0.5f, waypoint.y);
            //}
        }

        private Vector3 GetLineRendererCoordinate(in Cell currentCell, IList<Cell> allCells, ref int index, ref Direction previousDirection)
        {
            ++index;
            Direction offsetDirection = Defs.RotateDirection(previousDirection, RotationDirection.CounterClockwise);
            CoordinateOffset rendererOffset = Defs.GetFacing(offsetDirection);
            if (index < allCells.Count)
            {
                Cell testCell = allCells[index];
                CoordinateOffset realOffset = CoordinateOffset.Distance(currentCell, testCell);
                Direction realDirection = Defs.GetOffsetDirection(realOffset);
                if (previousDirection != realDirection)
                {
                    Direction addedOffsetDirection = Defs.RotateDirection(realDirection, RotationDirection.CounterClockwise);
                    CoordinateOffset addedRendererOffset = Defs.GetFacing(addedOffsetDirection);
                    rendererOffset += addedRendererOffset;
                    previousDirection = realDirection;
                }
            }
            return new Vector3(currentCell.x + rendererOffset.x * 0.125f, 0.25f, currentCell.y + rendererOffset.y * 0.125f);
        }

        private void SortForShortestRoute(List<Cell> route, in Cell entry)//, in Cell exit)
        {
            List<Cell> remainingCells = route.ToList();
            route.Clear();
            Cell lastCell = entry;
            while (remainingCells.Any())
            {
                int closestCellIndex = -1;
                int shortestPath = int.MaxValue;
                for (int i = 0; i < remainingCells.Count; ++i)
                {
                    CoordinateOffset distance = CoordinateOffset.Distance(lastCell, remainingCells[i]);
                    if (distance.Magnitude < shortestPath)
                    {
                        // Best candidate found so far
                        closestCellIndex = i;
                        shortestPath = distance.Magnitude;
                    }
                }

                Cell closestCell = remainingCells[closestCellIndex];
                route.Add(closestCell);
                remainingCells.RemoveAt(closestCellIndex);
                lastCell = closestCell;
            }
        }

        private (Cell, Direction) PickRandomEntryNode(Room firstRoom)
        {
            // Pick a direction other than where the exit is
            Direction exitDirection = Direction.None;
            foreach (var (dir, otherRoom) in firstRoom.Doorways)
            {
                if (otherRoom.index == firstRoom.index + 1)
                {
                    exitDirection = GetDirectionOfExitNode(firstRoom, dir);
                    break;
                }
            }
            Direction startDirection = m_rng.Pick(Defs.ForEachDirection().Where(dir => dir != exitDirection));
            bool isVertical = startDirection == Direction.Forward || startDirection == Direction.Backward;
            int coordX = 0;
            int coordZ = 0;
            if (isVertical)
            {
                coordX = m_rng.Next(firstRoom.Left, firstRoom.Right);
            }
            else
            {
                coordZ = m_rng.Next(firstRoom.Back, firstRoom.Front);
            }

            switch (startDirection)
            {
                case Direction.Forward:
                    coordZ = firstRoom.Back;
                    break;
                case Direction.Right:
                    coordX = firstRoom.Left;
                    break;
                case Direction.Backward:
                    coordZ = firstRoom.Front - 1;
                    break;
                case Direction.Left:
                    coordX = firstRoom.Right - 1;
                    break;
            }
            return (Cell.Create(coordX, coordZ), startDirection);
        }

        private void GenerateWaypointNodesForRoom(Room room, IList<Cell> waypoints)
        {
            float mag = m_rng.NextSingle();
            mag = 1.0f - (mag * mag);
            int nodeCountForCurrentRoom = (int)System.Math.Ceiling(room.Cells.Count() * m_settings.maxRoomDensity * mag);
            List<Cell> availableCells = room.Cells.ToList();
            for (int i = 0; i < nodeCountForCurrentRoom; ++i)
            {
                int chosenCellIndex = m_rng.Next(availableCells.Count);
                Cell chosenCell = availableCells[chosenCellIndex];
                availableCells.RemoveAt(chosenCellIndex);
                waypoints.Add(chosenCell);
            }
        }

        private Direction GetDirectionOfExitNode(Room currentRoom, in Cell exitNode)
        {
            if (exitNode.x >= currentRoom.Left && exitNode.x < currentRoom.Right)
            {
                if (exitNode.y == currentRoom.Front) return Direction.Forward;
                if (exitNode.y == currentRoom.Back - 1) return Direction.Backward;
            }
            else
            {
                if (exitNode.x == currentRoom.Left - 1) return Direction.Left;
                if (exitNode.x == currentRoom.Right) return Direction.Right;
            }
            Debug.LogErrorFormat("Exit node at coordinate {0} does not sit on border of room {1}", exitNode, currentRoom);
            return Direction.None;
        }

        private (Cell, Direction) FindDoorwayLeadingToNextRoom(Room currentRoom)
        {
            var (exitNode, _) = currentRoom.Doorways.First(entry => entry.Item2.index == currentRoom.index + 1);
            Direction exitDir = GetDirectionOfExitNode(currentRoom, exitNode);
            // Step back from doorway one space to find end cell of room
            CoordinateOffset offset = Defs.GetFacing(Defs.InverseDirection(exitDir));
            return (exitNode + offset, exitDir);
        }

        private (Cell, Direction) BuildPathBetweenNodes(in Cell from, in Cell to, Direction sourceDirection, Room room, List<Cell> fullPath)
        {
            Cell currentPosition = from;
            CoordinateOffset currentOffset = CoordinateOffset.Distance(currentPosition, to);
            int maxIterations = currentOffset.Magnitude + 50;
            int currentIteration = 0;
            Direction currentDirection = sourceDirection;
            while (currentOffset.Magnitude > 0 && ++currentIteration < maxIterations)
            {
                //Debug.Log(string.Format("Generating {0}-facing path from {1} to {2} ({3})", sourceDirection, from, to, currentOffset));
                // Favour maintaining source direction
                Cell positionAfterMove = currentPosition + Defs.GetFacing(currentDirection);
                CoordinateOffset newOffset = CoordinateOffset.Distance(positionAfterMove, to);
                if (newOffset > currentOffset || !IsValidCellForRoom(positionAfterMove, room))
                {
                    // Moving past target, find direction to rotate towards target
                    Direction cw = Defs.RotateDirection(currentDirection, RotationDirection.Clockwise);
                    Cell positionAfterCWMove = currentPosition + Defs.GetFacing(cw);
                    Direction ccw = Defs.RotateDirection(currentDirection, RotationDirection.CounterClockwise);
                    Cell positionAfterCCWMove = currentPosition + Defs.GetFacing(ccw);
                    if (CoordinateOffset.Distance(positionAfterCWMove, to) < currentOffset || !IsValidCellForRoom(positionAfterCCWMove, room))
                    {
                        currentDirection = cw;
                    }
                    else if (CoordinateOffset.Distance(positionAfterCCWMove, to) < currentOffset || !IsValidCellForRoom(positionAfterCWMove, room))
                    {
                        currentDirection = ccw;
                    }
                    else
                    {
                        // We are facing directly away from the target, pick one
                        currentDirection = m_rng.NextBool() ? cw : ccw;
                    }
                    positionAfterMove = currentPosition + Defs.GetFacing(currentDirection);
                }
                fullPath.Add(positionAfterMove);
                currentPosition = positionAfterMove;
                currentOffset = CoordinateOffset.Distance(currentPosition, to);
            }
            Debug.AssertFormat(currentOffset.Magnitude == 0, "Failed to reach target from {0} to {1}", from, to);
            return (currentPosition, currentDirection);
        }

        private bool IsValidCellForRoom(in Cell cell, Room room)
        {
            return cell.x >= room.Left && cell.x < room.Right
                && cell.y >= room.Back && cell.y < room.Front;
        }
    }
}
