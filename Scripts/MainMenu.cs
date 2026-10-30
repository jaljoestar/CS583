using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioClip bubbleSound;

    // plays a bubble sound when start game
    public void StartGame()
    {
        SfxManager.Instance.PlaySfx(bubbleSound);
        SceneManager.LoadScene("Restaurant");
    }

    public void QuitGame()      // quit game
    {
        SfxManager.Instance.PlaySfx(bubbleSound);
        Application.Quit();
    }

}