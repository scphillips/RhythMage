// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;

namespace RhythMage
{
    public enum Direction
    {
        Forward,
        Right,
        Backward,
        Left,
        None
    }

    public enum RotationDirection
    {
        Clockwise,
        CounterClockwise
    }

    public enum EnemyType
    {
        Flying,
        Magic,
        Melee
    }

    public struct SegmentModelDef
    {
        public Cell origin;
        public int width;
        public int depth;

        public int insetCorners;

        public IReadOnlyList<Cell> Path => path;

        List<Cell> path;

        public IEnumerable<Cell> Cells
        {
            get
            {
                Cell current;
                for (int i = 0; i < width; ++i)
                {
                    for (int j = 0; j < depth; ++j)
                    {
                        current.x = i;
                        current.y = j;
                        if (HasCell(current))
                        {
                            yield return current;
                        }
                    }
                }
            }
        }

        public SegmentModelDef(Cell origin, int width, int depth, int insetCorners = 0)
        {
            this.origin = origin;
            this.width = width;
            this.depth = depth;
            this.insetCorners = insetCorners;

            path = new List<Cell>();
        }

        public bool HasCell(in Cell cell)
        {
            if (path.Contains(cell))
            {
                return true;
            }
            
            int mag = cell.x + cell.y;
            int invMag = width - 1 - cell.x + cell.y;

            return mag >= insetCorners
                && mag < width + depth - 1 - insetCorners
                && invMag >= insetCorners
                && invMag < width + depth - 1 - insetCorners;
        }

        public void AddToPath(in Cell cell)
        {
            path.Add(cell);
        }
    }

    public class Defs
    {
        public static readonly int enemyTypeCount = System.Enum.GetValues(typeof(EnemyType)).Length;

        public static IReadOnlyDictionary<Direction, CoordinateOffset> facings = new Dictionary<Direction, CoordinateOffset>()
        {
            { Direction.Forward, CoordinateOffset.Create(0, 1) },
            { Direction.Right, CoordinateOffset.Create(1, 0) },
            { Direction.Backward, CoordinateOffset.Create(0, -1) },
            { Direction.Left, CoordinateOffset.Create(-1, 0) }
        };

        public static Direction GetOffsetDirection(in CoordinateOffset offset)
        {
            foreach (var entry in facings)
            {
                if (entry.Value == offset)
                {
                    return entry.Key;
                }
            }
            return Direction.None;
        }

        public static Direction InverseDirection(Direction direction)
        {
            var directionCount = facings.Count;
            var directionInt = (int)direction;
            var inverseDirectionInt = (directionInt + directionCount / 2) % directionCount;
            return (Direction)inverseDirectionInt;
        }

        public static Direction RotateDirection(Direction direction, RotationDirection rotation)
        {
            int directionCount = facings.Count;
            var rotatedDirectionInt = (int)direction + (rotation == RotationDirection.Clockwise ? -1 : 1);
            rotatedDirectionInt = (rotatedDirectionInt + directionCount) % directionCount;
            return (Direction)rotatedDirectionInt;
        }
    }
}
