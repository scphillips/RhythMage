// Copyright (C) 2020-2021 Stephen Phillips - All Rights Reserved
// Unauthorized copying of this file via any medium is strictly prohibited.
// Written by Stephen Phillips <stephen.phillips.me@gmail.com>, May 2020

using UnityEngine;
using UnityEngine.SceneManagement;

namespace RhythMage
{
    public class GameSceneController : MonoBehaviour
    {
        [Zenject.Inject]
        readonly GameStateManager m_gameStateManager;

        [Zenject.Inject]
        readonly GameStateManager.Settings m_settings;

        [Zenject.Inject]
        readonly AvatarModel m_avatar;

        [Zenject.Inject]
        readonly LevelBuilder m_levelBuilder;

        [Zenject.Inject]
        readonly Zenject.ZenjectSceneLoader m_sceneLoader;

        [Zenject.Inject] readonly DungeonModel m_dungeon;
        [Zenject.Inject(Id = "dungeon_root")] readonly Transform m_dungeonRoot;

        float m_timeSinceAvatarDied;

        void Start()
        {
            m_levelBuilder.BuildLevel(m_dungeon, m_dungeonRoot);
            m_gameStateManager.IsGameRunning = true;
        }
        
        void Update()
        {
            if (m_gameStateManager.IsGameRunning == true && m_avatar.IsAlive() == false)
            {
                m_timeSinceAvatarDied += Time.deltaTime;

                if (m_timeSinceAvatarDied >= m_settings.delayTransitionToGameOverDuration)
                {
                    m_gameStateManager.IsGameRunning = false;

                    m_sceneLoader.LoadScene(m_settings.gameOverScene, LoadSceneMode.Single, (container) =>
                    {
                        container.BindInstance(m_avatar.killCount).WhenInjectedInto<GameOverSceneInstaller>();
                    });
                }
            }
        }
    }
}
