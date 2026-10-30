using UnityEngine;

public class PersistentUI : MonoBehaviour       // makes sure settings panel works in both main menu and restaurant
{
    public static PersistentUI Instance;
    public GameObject settingsPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)           // only one instance should exist
        {
            Destroy(gameObject);                            // if one alr exists, destroy
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}