using UnityEngine;
public class SettingsCarrier : MonoBehaviour
{
    // Script for carrying settings data between scenes
    public static SettingsCarrier Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public float masterVolume;
    public float menuVolume;
    public float musicVolume;
    public float soundVolume;

    public enum SoundType
    {
        none,
        menu,
        music,
        sound
    }

    // SentVolume combines masterVolume and another volume chosen by the sent type
    public float SentVolume(SoundType type)
    {
        float otherVolume = 1;
        switch (type)
        {
            case SoundType.menu: otherVolume = menuVolume; break;
            case SoundType.music: otherVolume = musicVolume; break;
            case SoundType.sound: otherVolume = soundVolume; break;
            default: break;
        }
        return 1 * (otherVolume * masterVolume);
    }

    public void PlaySound(AudioClip audioClip, float volume)
    {
        audioSource.PlayOneShot(audioClip, volume);
    }
}
