// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

namespace RhythMage
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
