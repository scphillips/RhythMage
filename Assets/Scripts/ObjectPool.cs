// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

namespace RhythMage
{
    public class ObjectPool<T>
    {
        class Entry
        {
            public T Entity { get; }

            public Entry next;

            public Entry(T entity)
            {
                Entity = entity;
            }
        }

        private Entry m_nextAvailable;

        public int Count
        {
            get;
            private set;
        }

        public void Add(T entity)
        {
            var newEntry = new Entry(entity)
            {
                next = m_nextAvailable
            };
            m_nextAvailable = newEntry;
            ++Count;
        }

        public T TryGet()
        {
            if (m_nextAvailable != null)
            {
                var entry = m_nextAvailable;
                m_nextAvailable = entry.next;
                --Count;
                return entry.Entity;
            }
            return default;
        }
    }
}
