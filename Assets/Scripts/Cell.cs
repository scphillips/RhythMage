// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

namespace RhythMage
{
    public struct Cell : System.IEquatable<Cell>
    {
        public int x;
        public int y;

        public static Cell Zero
        {
            get
            {
                Cell entry;
                entry.x = 0;
                entry.y = 0;
                return entry;
            }
        }

        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public string ID
        {
            get
            {
                return x.ToString("X2") + y.ToString("X2");
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Cell cell && Equals(cell);
        }

        public bool Equals(Cell other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return (x << 16) + y;
        }

        public static bool operator == (in Cell lhs, in Cell rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator != (in Cell lhs, in Cell rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }
    }
}
