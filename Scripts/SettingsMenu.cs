using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Inscribed")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public AudioClip cancelSound;

    void Start()
    {
        float savedMusic = PlayerPrefs.GetFloat("musicVolume", 0.4f);       // music/sfx load at 0.4
        float savedSfx = PlayerPrefs.GetFloat("sfxVolume", 0.4f);

        musicSlider.value = savedMusic;
        sfxSlider.value = savedSfx;
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("musicVolume", value);     // takes player volume pref and applies
        if (MusicManager.Instance != null)
            MusicManager.Instance.ApplyVolume(value);
    }

    public void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat("sfxVolume", value);       // takes player sfx volume pref and applies
        if (SfxManager.Instance != null)
            SfxManager.Instance.SetVolume(value);
    }

    public void BackToMenu()
    {
        SfxManager.Instance.PlaySfx(cancelSound);       // button noise
        gameObject.SetActive(false);
    }
}