using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Button soundButton;
    public Image soundIcon;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    private const string SoundEnabledKey = "SoundEnabled";
    private bool isSoundOn;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RestoreSavedSound()
    {
        AudioListener.volume = PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1 ? 1f : 0f;
    }

    private void OnEnable()
    {
        isSoundOn = PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;

        if (soundButton != null && !HasPersistentToggle())
        {
            soundButton.onClick.RemoveListener(ToggleSound);
            soundButton.onClick.AddListener(ToggleSound);
        }

        ApplySoundState();
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt(SoundEnabledKey, isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplySoundState();
    }

    private void OnDisable()
    {
        if (soundButton != null)
        {
            soundButton.onClick.RemoveListener(ToggleSound);
        }
    }

    private bool HasPersistentToggle()
    {
        for (int i = 0; i < soundButton.onClick.GetPersistentEventCount(); i++)
        {
            if (soundButton.onClick.GetPersistentTarget(i) == this &&
                soundButton.onClick.GetPersistentMethodName(i) == nameof(ToggleSound) &&
                soundButton.onClick.GetPersistentListenerState(i) != UnityEventCallState.Off)
            {
                return true;
            }
        }

        return false;
    }

    private void ApplySoundState()
    {
        AudioListener.volume = isSoundOn ? 1f : 0f;
        Sprite icon = isSoundOn ? soundOnSprite : soundOffSprite;
        if (soundIcon != null && icon != null)
        {
            soundIcon.sprite = icon;
        }
    }
}
