// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

namespace RhythMage
{
    public struct CoordinateOffset : System.IEquatable<CoordinateOffset>
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

        public void Apply(ref Cell cell)
        {
            cell.x += x;
            cell.y += y;
        }

        public override bool Equals(object obj)
        {
            return obj is CoordinateOffset && Equals((CoordinateOffset)obj);
        }

        public bool Equals(CoordinateOffset other)
        {
            return (x == other.x && y == other.y);
        }

        public override int GetHashCode()
        {
            return (x << 16) + y;
        }

        public static bool operator == (CoordinateOffset lhs, CoordinateOffset rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator != (CoordinateOffset lhs, CoordinateOffset rhs)
        {
            return !(lhs == rhs);
        }

        public static Cell operator +(Cell lhs, CoordinateOffset rhs)
        {
            Cell result = lhs;
            rhs.Apply(ref result);
            return result;
        }
    }
}
