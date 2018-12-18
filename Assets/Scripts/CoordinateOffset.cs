using System;

namespace Outplay.RhythMage
{
    public struct CoordinateOffset : IEquatable<CoordinateOffset>
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
