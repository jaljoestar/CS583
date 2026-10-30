using UnityEngine;
// sound on click helper
public class ButtonSfx : MonoBehaviour
{
    public AudioClip sound;

    public void PlaySound()     // hooked up to buttons onclick
    {
        SfxManager.Instance.PlaySfx(sound);
    }
}