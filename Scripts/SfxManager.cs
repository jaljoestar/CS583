using UnityEngine;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance;

    public float sfxVolume = 0.5f;
    AudioSource source;

    void Awake()
{
    if (Instance != null && Instance != this)       // makes sure one instance, destroys others
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);

    source = GetComponent<AudioSource>();
    sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 0.5f);
}

    public void SetVolume(float volume)     // set sfx to player pref
    {
        sfxVolume = volume;
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || source == null) return;
        source.PlayOneShot(clip, sfxVolume);
    }
}