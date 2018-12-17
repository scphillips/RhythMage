namespace Outplay.RhythMage
{
    public class GameDifficulty
    {
        [System.Serializable]
        public class Settings
        {
            public float minEnemyPopulation;
            public float maxEnemyPopulation;

            public float maxInputTimeOffBeat;
        }

        [Zenject.Inject]
        readonly Settings m_settings;
    }
}
