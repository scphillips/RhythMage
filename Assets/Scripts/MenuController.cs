using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public string nextScene = "GameScene";

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }
}
