using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Button soundButton;     // assign your button in the Inspector
    public Image soundIcon;        // the Image component that shows the icon
    public Sprite soundOnSprite;   // sprite for "sound on"
    public Sprite soundOffSprite;  // sprite for "sound off"

    private bool isSoundOn = true; // default state

    void Start()
    {
        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);

        UpdateIcon();
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;

        // Turn Audio on/off globally
        AudioListener.volume = isSoundOn ? 1 : 0;

        UpdateIcon();
    }

    void UpdateIcon()
    {
        if (soundIcon != null)
            soundIcon.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }
}
