// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythMage
{
    public class GameSceneController : MonoBehaviour
    {
        GameStateManager m_gameStateManager;
        AvatarModel m_avatar;
        public DungeonModel dungeon = new DungeonModel();

        LevelBuilder m_levelBuilder;
        PathBuilder m_pathBuilder;
        GameSettings m_settings;

        public float TimeSinceAvatarDied { get; private set; }

        private void Start()
        {
            m_gameStateManager = Utils.FindGameStateManager();
            m_settings = Utils.FindGameSettings();
            m_avatar = Utils.FindAvatarModel();
            m_pathBuilder = new PathBuilder();
            m_levelBuilder = new LevelBuilder(m_settings, m_pathBuilder);
            m_levelBuilder.BuildLevel(dungeon, Utils.FindLevelRoot());
            m_gameStateManager.IsGameRunning = true;
        }
        
        private void Update()
        {
            if (m_gameStateManager.IsGameRunning == true && m_avatar.IsAlive == false)
            {
                TimeSinceAvatarDied += Time.deltaTime;

                if (TimeSinceAvatarDied >= m_settings.GameStateManagerSettings.delayTransitionToGameOverDuration)
                {
                    m_gameStateManager.IsGameRunning = false;

                    SceneManager.LoadScene(m_settings.GameStateManagerSettings.gameOverScene);
                }
            }
        }

        public void BuildLevel()
        {
            m_levelBuilder.BuildLevel(dungeon, Utils.FindLevelRoot());
        }
    }
}
