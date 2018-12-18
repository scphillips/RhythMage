namespace Outplay.RhythMage
{
    public class ObjectPool<T>
    {
        class Entry
        {
            public T entity { get; }

            public Entry next;

            public Entry(T entity)
            {
                this.entity = entity;
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
            var newEntry = new Entry(entity);
            newEntry.next = m_nextAvailable;
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
                return entry.entity;
            }
            return default(T);
        }
    }
}
