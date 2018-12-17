using UnityEngine;
using UnityEngine.SceneManagement;

namespace Outplay.RhythMage
{
    public class GameOverController
    {
        public string nextScene = "MenuScene";

        int m_finalScore;

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
            SceneManager.LoadScene(nextScene);
        }
    }
}
