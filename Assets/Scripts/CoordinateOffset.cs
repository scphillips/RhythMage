// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

namespace RhythMage
{
    public struct CoordinateOffset : System.IComparable<CoordinateOffset>, System.IEquatable<CoordinateOffset>
    {
        public int x;
        public int y;

        public static CoordinateOffset Create(int x, int y)
        {
            CoordinateOffset offset;
            offset.x = x;
            offset.y = y;
            return offset;
        }

        public static CoordinateOffset Distance(in Cell from, in Cell to)
        {
            CoordinateOffset offset;
            offset.x = to.x - from.x;
            offset.y = to.y - from.y;
            return offset;
        }

        public void ApplyTo(ref Cell index)
        {
            index.x += x;
            index.y += y;
        }

        public int CompareTo(CoordinateOffset other)
        {
            return (x << 16) + y - ((other.x << 16) + other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is CoordinateOffset offset && Equals(offset);
        }

        public bool Equals(CoordinateOffset other)
        {
            return (x == other.x) && (y == other.y);
        }

        public override int GetHashCode()
        {
            return (x << 16) + y;
        }

        public static bool operator ==(CoordinateOffset lhs, CoordinateOffset rhs)
        {
            return (lhs.x == rhs.x) && (lhs.y == rhs.y);
        }

        public static bool operator !=(CoordinateOffset lhs, CoordinateOffset rhs)
        {
            return !(lhs == rhs);
        }

        public static CoordinateOffset operator -(CoordinateOffset offset)
        {
            return Create(-offset.x, -offset.y);
        }

        public static CoordinateOffset operator +(CoordinateOffset lhs, CoordinateOffset rhs)
        {
            return Create(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        public static CoordinateOffset operator -(CoordinateOffset lhs, CoordinateOffset rhs)
        {
            return Create(lhs.x - rhs.x, lhs.y - rhs.y);
        }

        public static CoordinateOffset operator *(CoordinateOffset offset, int magnitude)
        {
            return Create(offset.x * magnitude, offset.y * magnitude);
        }

        public static Cell operator +(in Cell lhs, in CoordinateOffset rhs)
        {
            Cell result = lhs;
            rhs.ApplyTo(ref result);
            return result;
        }

        public static Cell operator -(in Cell lhs, in CoordinateOffset rhs)
        {
            Cell result = lhs;
            (-rhs).ApplyTo(ref result);
            return result;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }
}
