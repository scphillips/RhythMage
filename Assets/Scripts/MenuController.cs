using UnityEngine;
using UnityEngine.SceneManagement;

namespace Outplay.RhythMage
{
    public class MenuController : MonoBehaviour
    {
        public string nextScene = "GameScene";
        
        public void LoadNextScene()
        {
            SceneManager.LoadScene(nextScene);
        }
    }
}
