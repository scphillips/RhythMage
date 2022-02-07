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

        public void BuildPath(DungeonModel dungeon, List<Room> allRooms)
        {
            Debug.AssertFormat(allRooms.Any(), "Attempting to build path with no rooms");
            List<Cell> fullPath = new List<Cell>();
            Object.Destroy(m_pathOutline);
            m_pathOutline = new GameObject(string.Format("Path"));
            // Pick random entry node for first room
            var (entryNode, entryDirection) = PickRandomEntryNode(allRooms.First());
            HashSet<Cell> visitedCells = new HashSet<Cell>();
            for (int i = 0; i < allRooms.Count; ++i)
            {
                Room currentRoom = allRooms[i];
                visitedCells.Clear();

                int maxNodesForCurrentRoom = (int)(currentRoom.Cells.Count() * m_settings.maxRoomDensity);
                int nodeCountForCurrentRoom = m_rng.Next(maxNodesForCurrentRoom);

                Cell lastCell = entryNode;
                Direction lastDirection = entryDirection;
                // Pick random node for testing targets
                for (int j = 0; j < nodeCountForCurrentRoom; ++j)
                {
                    Cell chosenCell = m_rng.Pick(currentRoom.Cells.Where(entry => !visitedCells.Contains(entry)));
                    visitedCells.Add(chosenCell);
                    SpriteRenderer node = Object.Instantiate(m_dungeonAmbientSettings.tilePulsePrefab, m_pathOutline.transform);
                    node.name = string.Format("Node {0}", i);
                    node.transform.localPosition = new Vector3(chosenCell.x, 0.5f, chosenCell.y);
                    (lastCell, lastDirection) = BuildPathBetweenNodes(lastCell, chosenCell, lastDirection, currentRoom, fullPath);
                }

                if (i < allRooms.Count - 1)
                {
                    var (exitNode, exitDirection) = FindDoorwayLeadingToNextRoom(currentRoom);
                    (lastCell, lastDirection) = BuildPathBetweenNodes(lastCell, exitNode, lastDirection, currentRoom, fullPath);

                    // Extend path to cross boundary into next room
                    Cell positionAfterMove = lastCell + Defs.facings[exitDirection];
                    fullPath.Add(positionAfterMove);
                    entryNode = positionAfterMove;
                    entryDirection = exitDirection;
                }
            }

            Debug.Log(string.Format("Generated {0} steps in path", fullPath.Count));
            LineRenderer renderer = m_pathOutline.AddComponent<LineRenderer>();
            renderer.material = m_levelBuilderSettings.regionDebugOutlineMaterial;
            Vector3[] pathLines = fullPath.Select(entry => new Vector3(entry.x, 0.25f, entry.y)).ToArray();
            renderer.positionCount = fullPath.Count;
            renderer.startWidth = renderer.endWidth = 0.25f;
            renderer.SetPositions(pathLines);
            renderer.startColor = renderer.endColor = new Color(1.0f, 1.0f, 0.0f);
            m_pathOutline.transform.SetParent(m_dungeonRoot, false);
            dungeon.Path.AddRange(fullPath);
        }

        private (Cell, Direction) PickRandomEntryNode(Room firstRoom)
        {
            Direction startDirection = m_rng.Pick(Defs.ForEachDirection());
            bool isVertical = startDirection == Direction.Forward || startDirection == Direction.Backward;
            int coordX = 0;
            int coordZ = 0;
            if (isVertical)
            {
                coordX = m_rng.Next(firstRoom.Left + 1, firstRoom.Right);
            }
            else
            {
                coordZ = m_rng.Next(firstRoom.Back + 1, firstRoom.Front);
            }

            switch (startDirection)
            {
                case Direction.Forward:
                    coordZ = firstRoom.Front - 1;
                    break;
                case Direction.Right:
                    coordX = firstRoom.Right - 1;
                    break;
                case Direction.Backward:
                    coordZ = firstRoom.Back + 1;
                    break;
                case Direction.Left:
                    coordX = firstRoom.Left + 1;
                    break;
            }
            return (Cell.Create(coordX, coordZ), Defs.InverseDirection(startDirection));
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
            CoordinateOffset offset = Defs.facings[Defs.InverseDirection(exitDir)];
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
                Cell positionAfterMove = currentPosition + Defs.facings[currentDirection];
                CoordinateOffset newOffset = CoordinateOffset.Distance(positionAfterMove, to);
                if (newOffset > currentOffset || !IsValidCellForRoom(positionAfterMove, room))
                {
                    // Moving past target, find direction to rotate towards target
                    Direction cw = Defs.RotateDirection(currentDirection, RotationDirection.Clockwise);
                    Cell positionAfterCWMove = currentPosition + Defs.facings[cw];
                    Direction ccw = Defs.RotateDirection(currentDirection, RotationDirection.CounterClockwise);
                    Cell positionAfterCCWMove = currentPosition + Defs.facings[ccw];
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
                    positionAfterMove = currentPosition + Defs.facings[currentDirection];
                }
                fullPath.Add(positionAfterMove);
                currentPosition = positionAfterMove;
                currentOffset = CoordinateOffset.Distance(currentPosition, to);
            }
            Debug.AssertFormat(currentOffset.Magnitude == 0, "Failed to reach target {0}", to);
            return (currentPosition, currentDirection);
        }

        private bool IsValidCellForRoom(in Cell cell, Room room)
        {
            return cell.x >= room.Left && cell.x < room.Right
                && cell.y >= room.Back && cell.y < room.Front;
        }
    }
}
