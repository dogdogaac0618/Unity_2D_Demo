using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneManager.LoadScene(SceneNames.GameScene);
    }

    public void LoadResultUI()
    {
        SceneManager.LoadScene(SceneNames.ResultUI);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
