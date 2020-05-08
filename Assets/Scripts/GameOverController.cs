// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine.SceneManagement;

namespace RhythMage
{
    public class GameOverController
    {
        [Zenject.Inject]
        readonly GameStateManager.Settings m_settings;

        [Zenject.Inject]
        readonly int m_finalScore;

        public int GetFinalScore()
        {
            return m_finalScore;
        }

        public void LoadNextScene()
        {
            SceneManager.LoadScene(m_settings.menuScene);
        }
    }
}
