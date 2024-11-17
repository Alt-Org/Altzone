using UnityEngine;
using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;

public class SettingsCarrier : MonoBehaviour
{
    // Script for carrying settings data between scenes
    public static SettingsCarrier Instance { get; private set; }
    public int mainMenuWindowIndex;

    private TextSize _textSize;

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
    }

    private void Start()
    {
        Application.targetFrameRate = PlayerPrefs.GetInt("TargetFrameRate", Screen.currentResolution.refreshRate);
        mainMenuWindowIndex = 0;

        _textSize = (TextSize)PlayerPrefs.GetInt("TextSize", 2);
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
    public enum TextSize
    {
        None,
        Small,
        Medium,
        Large
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

    public void SetTextSize(TextSize size)
    {
        _textSize = size;
        PlayerPrefs.SetInt("Textsize", (int)size);
    }

    // Determines which character stat window to load/show from character gallery
    private CharacterID _characterGalleryCharacterStatWindowToShow = CharacterID.None;
    public CharacterID CharacterGalleryCharacterStatWindowToShow
    {
        get => _characterGalleryCharacterStatWindowToShow;
        set
        {
            if (_characterGalleryCharacterStatWindowToShow != value)
            {
                _characterGalleryCharacterStatWindowToShow = value;
                OnCharacterGalleryCharacterStatWindowToShowChange?.Invoke(_characterGalleryCharacterStatWindowToShow);
                Debug.Log("CharacterGallery value changed" + _characterGalleryCharacterStatWindowToShow);
            }
        }
    }

    public TextSize Textsize { get => _textSize; }

    public event Action<CharacterID> OnCharacterGalleryCharacterStatWindowToShowChange;

}
