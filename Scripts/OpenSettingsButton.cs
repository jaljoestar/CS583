using UnityEngine;

public class OpenSettingsButton : MonoBehaviour         // plays bubble sounds when click on open settings button
{
    public AudioClip bubbleSound;

    public void OpenSettings()
    {
        SfxManager.Instance.PlaySfx(bubbleSound);
        PersistentUI.Instance.settingsPanel.SetActive(true);
    }
}