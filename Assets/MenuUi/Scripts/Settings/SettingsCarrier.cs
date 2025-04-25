using UnityEngine;
using System;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.BattleUiShared;

public class SettingsCarrier : MonoBehaviour // Script for carrying settings data between scenes
{
    // Instance singleton
    public static SettingsCarrier Instance { get; private set; }

    // Enums
    public enum SoundType
    {
        none,
        menu,
        music,
        sound
    }

    public enum TextSize
    {
        Small,
        Medium,
        Large
    }

    public enum BattleUiElementType
    {
        Timer,
        PlayerInfo,
        TeammateInfo,
        Diamonds,
        GiveUpButton,
    }

    // Events
    public event Action OnTextSizeChange;
    public event Action OnButtonLabelVisibilityChange;
    public event Action<CharacterID> OnCharacterGalleryCharacterStatWindowToShowChange;

    // Constants
    public const string BattleArenaScaleKey = "BattleUiArenaScale";
    public const string BattleArenaPosXKey = "BattleUiPosX";
    public const string BattleArenaPosYKey = "BattleUiPosY";

    public const int BattleArenaScaleDefault = 100;
    public const int BattleArenaPosXDefault = 50;
    public const int BattleArenaPosYDefault = 50;

    // Settings variables
    public int mainMenuWindowIndex;

    public float masterVolume;
    public float menuVolume;
    public float musicVolume;
    public float soundVolume;

    public int TextSizeSmall = 22;
    public int TextSizeMedium = 26;
    public int TextSizeLarge = 30;

    private TextSize _textSize;
    public TextSize Textsize { get => _textSize; }
    
    private bool _showButtonLabels;
    public bool ShowButtonLabels
    {
        get
        {
            return _showButtonLabels;
        }

        set
        {
            _showButtonLabels = value;
            OnButtonLabelVisibilityChange?.Invoke();
        }
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

    private int _battleArenaScale;
    public int BattleArenaScale
    {
        get => _battleArenaScale;
        set
        {
            if (_battleArenaScale == value) return;
            _battleArenaScale = value;
            PlayerPrefs.SetInt(BattleArenaScaleKey, value);
        }
    }

    private int _battleArenaPosX;
    public int BattleArenaPosX
    {
        get => _battleArenaPosX;
        set
        {
            if (_battleArenaPosX == value) return;
            _battleArenaPosX = value;
            PlayerPrefs.SetInt(BattleArenaPosXKey, value);
        }
    }

    private int _battleArenaPosY;
    public int BattleArenaPosY
    {
        get => _battleArenaPosY;
        set
        {
            if (_battleArenaPosY == value) return;
            _battleArenaPosY = value;
            PlayerPrefs.SetInt(BattleArenaPosYKey, value);
        }
    }

    // Functions
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

        _textSize = (TextSize)PlayerPrefs.GetInt("TextSize", 1);
        _showButtonLabels = (PlayerPrefs.GetInt("showButtonLabels", 1) == 1);

        _battleArenaScale = PlayerPrefs.GetInt(BattleArenaScaleKey, BattleArenaScaleDefault);
        _battleArenaPosX = PlayerPrefs.GetInt(BattleArenaPosXKey, BattleArenaPosXDefault);
        _battleArenaPosY = PlayerPrefs.GetInt(BattleArenaPosYKey, BattleArenaPosYDefault);
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
        PlayerPrefs.SetInt("TextSize", (int)size);
        OnTextSizeChange?.Invoke();
    }

    public BattleUiMovableElementData GetBattleUiMovableElementData(BattleUiElementType type)
    {
        string json = PlayerPrefs.GetString($"BattleUi{type}", string.Empty);
        if (string.IsNullOrEmpty(json)) return null;
        return JsonUtility.FromJson<BattleUiMovableElementData>(json);
    }

    public void SetBattleUiMovableElementData(BattleUiElementType type, BattleUiMovableElementData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString($"BattleUi{type}", json);
    }
}
