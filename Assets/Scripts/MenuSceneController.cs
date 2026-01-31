// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythMage
{
    public class MenuSceneController : MonoBehaviour
    {
        GameSettings m_settings;

        void Start()
        {
            m_settings = Utils.GetOrCreateGameSettings();
        }

        public void LoadNextScene()
        {
            SceneManager.LoadScene(m_settings.GameStateManagerSettings.gameScene);
        }
    }
}
