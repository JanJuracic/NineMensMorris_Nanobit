using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void GoToPlayScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitApplication()
    {
        if (Application.isEditor)
        {
            Debug.Log("Application quitting called.");
        }

        Application.Quit();
    }
}
