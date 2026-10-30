using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour        // makes sure no residue sound plays when loading menu
{
    public AudioClip cancelSound;

    public void LoadMainMenu()
    {
        SfxManager.Instance.PlaySfx(cancelSound);
        SceneManager.LoadScene("MainMenu");
    }
}