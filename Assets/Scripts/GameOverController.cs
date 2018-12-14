using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public string nextScene = "MenuScene";

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }
}
