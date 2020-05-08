using UnityEngine;
using UnityEngine.SceneManagement;

namespace Outplay.RhythMage
{
    public class GameSceneController : MonoBehaviour
    {
        [Zenject.Inject]
        readonly GameStateManager m_gameStateManager;

        [Zenject.Inject]
        readonly GameStateManager.Settings m_settings;

        [Zenject.Inject]
        AvatarModel m_avatar;

        [Zenject.Inject]
        readonly Zenject.ZenjectSceneLoader m_sceneLoader;

        float m_timeSinceAvatarDied;

        void Start()
        {
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
