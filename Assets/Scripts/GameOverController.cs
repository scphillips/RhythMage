using UnityEngine.SceneManagement;

namespace Outplay.RhythMage
{
    public class GameOverController
    {
        [Zenject.Inject]
        readonly GameStateManager.Settings m_settings;
        
        readonly int m_finalScore;

        GameOverController(int finalScore)
        {
            m_finalScore = finalScore;
        }

        public int GetFinalScore()
        {
            return m_finalScore;
        }

        public void LoadNextScene()
        {
            SceneManager.LoadScene(m_settings.menuScene.name);
        }
    }
}
