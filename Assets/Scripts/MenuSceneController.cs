using UnityEngine;
using UnityEngine.SceneManagement;

namespace Outplay.RhythMage
{
    public class MenuSceneController : MonoBehaviour
    {
        [Zenject.Inject]
        readonly GameStateManager.Settings m_settings;
        
        public void LoadNextScene()
        {
            SceneManager.LoadScene(m_settings.gameScene);
        }
    }
}
