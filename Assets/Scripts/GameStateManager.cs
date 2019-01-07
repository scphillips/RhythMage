using System;
using UnityEngine.SceneManagement;
using Zenject;

namespace Outplay.RhythMage
{
    public class GameStateManager : IInitializable
    {
        [Serializable]
        public class Settings
        {
            public SceneReference gameOverScene;
            public SceneReference gameScene;
            public SceneReference menuScene;
        }

        [Inject]
        readonly Settings m_settings;

        [Inject]
        readonly ZenjectSceneLoader m_sceneLoader;

        [Inject]
        AvatarModel m_avatar;
        
        public void Initialize()
        {
            m_avatar.OnHealthChange += OnHealthChanged;
        }

        void OnHealthChanged(object sender, EventArgs e)
        {
            if (m_avatar.IsAlive() == false)
            {
                m_sceneLoader.LoadScene(m_settings.gameOverScene, LoadSceneMode.Single, (container) =>
                {
                    container.BindInstance(m_avatar.killCount).WhenInjectedInto<GameOverSceneInstaller>();
                });
            }
        }
    }
}
