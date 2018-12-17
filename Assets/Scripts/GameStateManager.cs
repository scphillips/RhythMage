using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Outplay.RhythMage
{
    public class GameStateManager
    {
        [Serializable]
        public class Settings
        {
            public string gameOverSceneId;
        }

        [Inject]
        readonly Settings m_settings;

        AvatarModel m_avatar;

        readonly ZenjectSceneLoader m_sceneLoader;

        [Inject]
        GameStateManager(ZenjectSceneLoader sceneLoader,
            AvatarModel avatarModel)
        {
            m_sceneLoader = sceneLoader;
            m_avatar = avatarModel;
            m_avatar.OnHealthChange += OnHealthChanged;
        }

        void OnHealthChanged(object sender, EventArgs e)
        {
            if (m_avatar.IsAlive() == false)
            {
                m_sceneLoader.LoadScene(m_settings.gameOverSceneId, LoadSceneMode.Single, (container) =>
                {
                    container.BindInstance(m_avatar.KillCount).WhenInjectedInto<GameOverSceneInstaller>();
                });
            }
        }
    }
}
