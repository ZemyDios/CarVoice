using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene("TestLevelScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
