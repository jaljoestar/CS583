using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioClip menuMusic;
    public AudioClip restaurantMusic;

    AudioSource source;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);              // when restaurant loads music doesnt get destroyed

        source = GetComponent<AudioSource>();
        source.loop = true;
        PlayerPrefs.DeleteKey("musicVolume");           // resets the volume at every launch
        source.volume = PlayerPrefs.GetFloat("musicVolume", 0.5f);      // starts at 0.5 volume

        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    void PlayForScene(string sceneName)         // swaps to other music
    {
        AudioClip clip = null;

        if (sceneName == "MainMenu")
            clip = menuMusic;
        else if (sceneName == "Restaurant")
            clip = restaurantMusic;
        else
            return;          

        if (clip == null || source.clip == clip)
            return;

        source.clip = clip;
        source.Play();
    }

    public void ApplyVolume(float volume)
    {
        source.volume = volume;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}