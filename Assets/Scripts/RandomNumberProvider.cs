// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

namespace RhythMage
{
    public class RandomNumberProvider
    {
        readonly System.Random m_random;

        public RandomNumberProvider()
        {
            m_random = new System.Random();
        }

        public RandomNumberProvider(int seed)
        {
            m_random = new System.Random(seed);
        }

        public System.Random Get()
        {
            return m_random;
        }

        public int Next()
        {
            return Get().Next();
        }

        public int Next(int minValue, int maxValue)
        {
            return Get().Next(minValue, maxValue);
        }

        public virtual int Next(int maxValue)
        {
            return Get().Next(maxValue);
        }

        public virtual void NextBytes(byte[] buffer)
        {
            Get().NextBytes(buffer);
        }

        public virtual float NextSingle()
        {
            return System.Convert.ToSingle(Get().NextDouble());
        }

        public virtual double NextDouble()
        {
            return Get().NextDouble();
        }
    }
}
