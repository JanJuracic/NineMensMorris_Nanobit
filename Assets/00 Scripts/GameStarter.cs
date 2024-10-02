
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
