using UnityEngine.SceneManagement;

namespace Outplay.RhythMage
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
