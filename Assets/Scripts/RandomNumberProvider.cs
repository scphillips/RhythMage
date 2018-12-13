namespace Outplay
{
    public class RandomNumberProvider
    {
        readonly System.Random m_random;

        public RandomNumberProvider(
            [Zenject.InjectOptional]
            int? seed)
        {
            if (seed != null)
            {
                m_random = new System.Random(seed.Value);
            }
            else
            {
                m_random = new System.Random();
            }
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

        public virtual double NextDouble()
        {
            return Get().NextDouble();
        }
    }
}
