using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGame()
    {
        GameSettings.IsAgainstAI = false;
        SceneManager.LoadScene("GameScene");
    }

    public void LoadLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void LoadGameVsAI()
    {
        GameSettings.IsAgainstAI = true;
        SceneManager.LoadScene("GameScene");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}

public static class GameSettings
{
    public static bool IsAgainstAI = false;
}