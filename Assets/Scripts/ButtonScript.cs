using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLoader : MonoBehaviour
{
    // Load Menu2 scene
    public void LoadMenu2()
    {
        SceneManager.LoadScene("Menu2");
    }

    // Load Guide scene
    public void LoadGuide()
    {
        SceneManager.LoadScene("Guide");
    }

    public void LoadSettings()

    {
        SceneManager.LoadScene("Settings");
    }

    public void LoadLevels()
    {
        SceneManager.LoadScene("Levels");
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("MainGame");
    }
}
