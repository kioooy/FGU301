using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Option 1: Hardcoded scene name
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // Option 2: Configurable scene name
    [SerializeField] private string sceneName = "MainMenu";

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
