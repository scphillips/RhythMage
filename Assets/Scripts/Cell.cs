using System;

namespace Outplay
{
    public struct Cell : IEquatable<Cell>
    {
        public int x;
        public int y;

        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public string ID
        {
            get
            {
                // [ 222, 173 ] -> "DEAD"
                return x.ToString("X2") + y.ToString("X2");
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Cell && Equals((Cell)obj);
        }

        public bool Equals(Cell other)
        {
            return (x == other.x && y == other.y);
        }

        public override int GetHashCode()
        {
            return (x << 16) + y;
        }

        public static bool operator == (Cell lhs, Cell rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator != (Cell lhs, Cell rhs)
        {
            return !(lhs == rhs);
        }
    }
}
