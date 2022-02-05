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
        readonly RandomNumberProvider m_rng;

        [Zenject.Inject(Id = "dungeon_root")] readonly Transform m_dungeonRoot;

        private GameObject m_pathOutline;

        public void BuildPath(DungeonModel dungeon, List<Room> allRooms)
        {
            Debug.AssertFormat(allRooms.Any(), "Attempting to build path with no rooms");
            List<Cell> fullPath = new List<Cell>();
            // Pick random entry node for first room
            var (entryNode, entryDirection) = PickRandomEntryNode(allRooms.First());
            for (int i = 0; i < allRooms.Count - 1; ++i)
            {
                Room currentRoom = allRooms[i];
                Room nextRoom = allRooms[i + 1];

                int maxNodesForCurrentRoom = (int)(currentRoom.Cells.Count() * m_settings.maxRoomDensity);
                int nodeCountForCurrentRoom = m_rng.Next(maxNodesForCurrentRoom);

                var (exitNode, exitDirection) = FindDoorwayLeadingToNextRoom(currentRoom);
                BuildPathBetweenNodes(entryNode, exitNode, entryDirection, fullPath);

                entryNode = exitNode;
                entryDirection = exitDirection;
            }

            Debug.Log(string.Format("Generated {0} steps in path", fullPath.Count));
            Object.Destroy(m_pathOutline);
            m_pathOutline = new GameObject(string.Format("Path"));
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

        private void BuildPathBetweenNodes(in Cell from, in Cell to, Direction sourceDirection, List<Cell> fullPath)
        {
            Cell currentPosition = from;
            CoordinateOffset currentOffset = CoordinateOffset.Distance(currentPosition, to);
            int maxIterations = currentOffset.Magnitude * 2;
            int currentIteration = 0;
            Direction currentDirection = sourceDirection;
            while (currentOffset.Magnitude > 0 && ++currentIteration < maxIterations)
            {
                //Debug.Log(string.Format("Generating {0}-facing path from {1} to {2} ({3})", sourceDirection, from, to, currentOffset));
                // Favour maintaining source direction
                Cell positionAfterMove = currentPosition + Defs.facings[currentDirection];
                CoordinateOffset newOffset = CoordinateOffset.Distance(positionAfterMove, to);
                if (newOffset > currentOffset)
                {
                    // Moving past target, find direction to rotate towards target
                    Direction cw = Defs.RotateDirection(currentDirection, RotationDirection.Clockwise);
                    Cell positionAfterCWMove = currentPosition + Defs.facings[cw];
                    Direction ccw = Defs.RotateDirection(currentDirection, RotationDirection.CounterClockwise);
                    Cell positionAfterCCWMove = currentPosition + Defs.facings[ccw];
                    if (CoordinateOffset.Distance(positionAfterCWMove, to) < currentOffset)
                    {
                        currentDirection = cw;
                    }
                    else if (CoordinateOffset.Distance(positionAfterCCWMove, to) < currentOffset)
                    {
                        currentDirection = ccw;
                    }
                    else
                    {
                        // We are facing directly away from the target, pick a direction which won't intersect a wall
                        currentDirection = m_rng.NextBool() ? cw : ccw;
                    }
                    positionAfterMove = currentPosition + Defs.facings[currentDirection];
                }
                fullPath.Add(positionAfterMove);
                currentPosition = positionAfterMove;
                currentOffset = CoordinateOffset.Distance(currentPosition, to);
            }
            Debug.AssertFormat(currentOffset.Magnitude == 0, "Failed to reach target {0}", to);
        }
    }
}
