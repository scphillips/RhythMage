using TMPro;
using UnityEngine;

namespace Outplay.RhythMage
{
    public class GameOverUIController : MonoBehaviour
    {
        [Zenject.Inject]
        readonly GameOverController m_gameOverController;

        public TextMeshProUGUI FinalScoreLabel;

        void Start()
        {
            FinalScoreLabel.text = "Total Kills: " + m_gameOverController.GetFinalScore();
        }

        public void OnReplayButtonPressed()
        {
            m_gameOverController.LoadNextScene();
        }
    }
}
