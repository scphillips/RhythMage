﻿// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
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
        }

        public bool HasCell(in Cell cell)
        {
            int mag = cell.x + cell.y;
            int invMag = width - 1 - mag;

            return mag >= insetCorners
                && mag < width + depth - 1 - insetCorners
                && invMag >= insetCorners
                && invMag < width + depth - 1 - insetCorners;
        }
    }

    public class Region
    {
        public Cell origin;
        public CoordinateOffset size;
        public List<System.ValueTuple<Direction, Region>> connections;

        public bool Enabled { get; set; } = true;

        public Region(int x, int y, int width, int depth)
        {
            origin.x = x;
            origin.y = y;
            size.x = width;
            size.y = depth;
            connections = new List<(Direction, Region)>();
        }

        public IEnumerable<Cell> Cells
        {
            get
            {
                Cell current;
                for (int i = 0; i < size.x - 1; ++i)
                {
                    for (int j = 0; j < size.y - 1; ++j)
                    {
                        current.x = origin.x + i;
                        current.y = origin.y + j;
                        yield return current;
                    }
                }
            }
        }

        public int Front => origin.y + size.y - 1;
        public int Right => origin.x + size.x - 1;
        public int Back => origin.y;
        public int Left => origin.x;
        public int Width => size.x;
        public int Depth => size.y;

        public override string ToString()
        {
            return string.Format("{0} ({1}x{2})", origin, size.x, size.y);
        }
    }

    public class Room : Region, System.IEquatable<Room>
    {
        public int index;

        public List<System.ValueTuple<Cell, Room>> Doorways { get; }

        public Room(int x, int y, int w, int d, int i) :
            base(x, y, w, d)
        {
            index = i;
            Doorways = new List<System.ValueTuple<Cell, Room>>();
        }

        public override bool Equals(object obj)
        {
            return obj is Room room && Equals(room);
        }

        public bool Equals(Room other)
        {
            return other?.Equals(null) == false && index == other.index;
        }

        public override int GetHashCode()
        {
            return origin.GetHashCode();
        }

        public static bool operator ==(in Room lhs, in Room rhs)
        {
            return ReferenceEquals(lhs, rhs) || (!(lhs is null) && lhs.Equals(rhs));
        }

        public static bool operator !=(in Room lhs, in Room rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return string.Format("{0} {1} ({2}x{3})", index, origin, size.x, size.y);
        }
    }

    public class AggregateRegion : Region
    {
        public List<Region> regions;

        public AggregateRegion() :
            base(0, 0, 0, 0)
        {
            regions = new List<Region>();
        }

        public AggregateRegion(Region sourceRegion) :
            base(0, 0, 0, 0)
        {
            regions = new List<Region>();
            Add(sourceRegion);
        }

        public void Reset()
        {
            regions.Clear();
            origin = default;
            size = default;
        }

        public void Add(Region region)
        {
            if (regions.Count > 0)
            {
                int right = Right;
                int front = Front;
                origin.x = System.Math.Min(origin.x, region.origin.x);
                origin.y = System.Math.Min(origin.y, region.origin.y);
                size.x = System.Math.Max(right, region.Right) - origin.x + 1;
                size.y = System.Math.Max(front, region.Front) - origin.y + 1;
            }
            else
            {
                origin = region.origin;
                size = region.size;
            }
            regions.Add(region);
        }

        public void AddRange(IEnumerable<Region> regions)
        {
            foreach (var region in regions)
            {
                Add(region);
            }
        }

        public int Count => regions.Count;

        public override string ToString()
        {
            return string.Format("{0} ({1}x{2}) containing {3} region{4}", origin, size.x, size.y, regions.Count, regions.Count == 1 ? "" : "s");
        }
    }

    public class Defs
    {
        public static readonly int enemyTypeCount = System.Enum.GetValues(typeof(EnemyType)).Length;

        public static IReadOnlyList<CoordinateOffset> facings = new List<CoordinateOffset>()
        {
            CoordinateOffset.Create(0, 1),
            CoordinateOffset.Create(1, 0),
            CoordinateOffset.Create(0, -1),
            CoordinateOffset.Create(-1, 0)
        };

        public static Direction GetOffsetDirection(in CoordinateOffset offset)
        {
            int directionInt = 0;
            foreach (var entry in facings)
            {
                if (entry == offset)
                {
                    return (Direction)directionInt;
                }
                ++directionInt;
            }
            return Direction.None;
        }

        public static CoordinateOffset GetFacing(Direction direction)
        {
            return facings[(int)direction];
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
            var rotatedDirectionInt = (int)direction + (rotation == RotationDirection.Clockwise ? 1 : -1);
            rotatedDirectionInt = (rotatedDirectionInt + directionCount) % directionCount;
            return (Direction)rotatedDirectionInt;
        }

        public static bool IsOrthogonal(Direction from, Direction to)
        {
            return RotateDirection(from, RotationDirection.Clockwise) == to
                || RotateDirection(from, RotationDirection.CounterClockwise) == to;
        }

        public static T Clamp<T>(T value, T min, T max) where T : System.IComparable<T>
        {
            int comparison = value.CompareTo(max);
            if (comparison > 0) return max;
            comparison = value.CompareTo(min);
            if (comparison < 0) return min;
            return value;
        }

        public static IEnumerable<Direction> ForEachDirection(Direction startDirection = Direction.Forward)
        {
            int startValue = (int)startDirection;
            for (int i = 0; i < facings.Count; ++i)
            {
                int value = (startValue + i) % facings.Count;
                yield return (Direction)value;
            }
        }
    }
}
