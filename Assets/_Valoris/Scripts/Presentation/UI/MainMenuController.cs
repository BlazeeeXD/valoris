using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject menuRoot;
    public string gameSceneName = "MainGame";

    public void StartGame()
    {
        if (menuRoot != null) menuRoot.SetActive(false);
        if (SceneManager.GetActiveScene().name != gameSceneName)
            SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
