// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using System.Collections.Generic;
using System.Linq;

namespace RhythMage
{
    public class RandomNumberProvider
    {
        private System.Random m_random;

        public RandomNumberProvider()
        {
            m_random = new System.Random();
        }

        public RandomNumberProvider(int seed)
        {
            m_random = new System.Random(seed);
        }

        public void SetSeed(int seed)
        {
            m_random = new System.Random(seed);
        }

        public System.Random Get()
        {
            return m_random;
        }

        public int Next()
        {
            return m_random.Next();
        }

        public int Next(int minValue, int maxValue)
        {
            return m_random.Next(minValue, maxValue);
        }

        public int Next(int maxValue)
        {
            return m_random.Next(maxValue);
        }

        public bool NextBool()
        {
            return (m_random.Next() & 1) == 0;
        }

        public void NextBytes(byte[] buffer)
        {
            m_random.NextBytes(buffer);
        }

        public float NextSingle()
        {
            return System.Convert.ToSingle(m_random.NextDouble());
        }

        public double NextDouble()
        {
            return m_random.NextDouble();
        }

        public T Pick<T>(IEnumerable<T> collection)
        {
            if (collection != null && collection.Any())
            {
                int index = Next(collection.Count());
                return collection.ElementAt(index);
            }
            return default;
        }
    }
}
