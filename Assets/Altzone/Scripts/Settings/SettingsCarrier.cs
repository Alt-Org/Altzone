using UnityEngine;
using System;
using Altzone.Scripts.BattleUiShared;
using System.Collections.Generic;

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

    public enum LanguageType
    {
        None,
        Finnish,
        English,
        Swedish
    }

    public enum TextSize
    {
        Small,
        Medium,
        Large
    }

    public enum BattleUiElementType
    {
        None = -1,
        Timer = 0,
        PlayerInfo = 1,
        TeammateInfo = 2,
        Diamonds = 3,
        GiveUpButton = 4,
        MoveJoystick = 5,
        RotateJoystick = 6,
    }

    public enum BattleMovementInputType
    {
        PointAndClick,
        FollowPointer,
        Swipe,
        Joystick
    }

    public enum BattleRotationInputType
    {
        Swipe,
        TwoFinger,
        Joystick,
        Gyroscope,
        ScreenArea
    }

    public enum TopBarStyle
    {
        Old,
        NewHelena,
        NewNiko
    }

    public enum JukeboxPlayArea
    {
        MainMenu,
        Soulhome,
        Battle
    }

    public enum SettingsType
    {
        None,
        JukeboxSoulhomeToggle,
        JukeboxUIToggle,
        JukeboxBattleToggle,
    }

    // Events
    public event Action OnTextSizeChange;
    public event Action OnButtonLabelVisibilityChange;

    public delegate void TopBarChanged(int index);
    public static event TopBarChanged OnTopBarChanged;

    public delegate void LanguageChanged(LanguageType language);
    public static event LanguageChanged OnLanguageChanged;

    // Constants
    public const string BattleShowDebugStatsOverlayKey = "BattleStatsOverlay";
    public const string BattleArenaScaleKey = "BattleUiArenaScale";
    public const string BattleArenaPosXKey = "BattleUiPosX";
    public const string BattleArenaPosYKey = "BattleUiPosY";
    public const string BattleMovementInputKey = "BattleMovement";
    public const string BattleRotationInputKey = "BattleRotation";
    public const string BattleSwipeMinDistanceKey = "BattleSwipeMinDistance";
    public const string BattleSwipeMaxDistanceKey = "BattleSwipeMaxDistance";
    public const string BattleSwipeSensitivityKey = "BattleSwipeSensitivity";
    public const string BattleGyroMinAngleKey = "BattleGyroMinAngle";

    public const int BattleArenaScaleDefault = 100;
    public const int BattleArenaPosXDefault = 50;
    public const int BattleArenaPosYDefault = 50;

    public const BattleMovementInputType BattleMovementInputDefault = BattleMovementInputType.PointAndClick;
    public const BattleRotationInputType BattleRotationInputDefault = BattleRotationInputType.TwoFinger;
    public const float BattleSwipeMinDistanceDefault = 0.1f;
    public const float BattleSwipeMaxDistanceDefault = 1f;
    public const float BattleSwipeSensitivityDefault = 1f;
    public const float BattleGyroMinAngleDefault = 10f;

    public const string UnlimitedStatUpgradeMaterialsKey = "UnlimitedStatUpgrade";

    public const string StatDebuggingModeKey = "StatDebugging";

    public const string TopBarStyleSettingKey = "TopBarStyleSetting";

    // Settings variables
    public int mainMenuWindowIndex;

    public float masterVolume;
    public float menuVolume;
    public float musicVolume;
    public float soundVolume;

    public bool jukeboxSoulhome;
    public bool jukeboxUI;
    public bool jukeboxBattle;

    public int TextSizeSmall = 22;
    public int TextSizeMedium = 26;
    public int TextSizeLarge = 30;

    private LanguageType _language = LanguageType.None;

    public LanguageType Language
    {
        get { return _language; }
        set
        {
            if (value == _language) return;
            _language = value;
            PlayerPrefs.SetString("LanguageType", ParseLanguage(value));
            OnLanguageChanged?.Invoke(_language);
        }
    }

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

    private bool _unlimitedStatUpgradeMaterials;
    public bool UnlimitedStatUpgradeMaterials
    {
        get => _unlimitedStatUpgradeMaterials;
        set
        {
            if (_unlimitedStatUpgradeMaterials == value) return;
            _unlimitedStatUpgradeMaterials = value;
            PlayerPrefs.SetInt(UnlimitedStatUpgradeMaterialsKey, value ? 1 : 0);
        }
    }

    private bool _statDebuggingMode;
    public bool StatDebuggingMode
    {
        get => _statDebuggingMode;
        set
        {
            if (_statDebuggingMode == value) return;
            _statDebuggingMode = value;
            PlayerPrefs.SetInt(StatDebuggingModeKey, value ? 1 : 0);
        }
    }

    private bool _battleShowDebugStatsOverlay;
    public bool BattleShowDebugStatsOverlay
    {
        get => _battleShowDebugStatsOverlay;
        set
        {
            if (_battleShowDebugStatsOverlay == value) return;
            _battleShowDebugStatsOverlay = value;
            PlayerPrefs.SetInt(BattleShowDebugStatsOverlayKey, value ? 1 : 0);
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

    private BattleMovementInputType _battleMovementInput;
    public BattleMovementInputType BattleMovementInput
    {
        get => _battleMovementInput;
        set
        {
            if (_battleMovementInput == value) return;
            _battleMovementInput = value;
            PlayerPrefs.SetInt(BattleMovementInputKey, (int)value);
        }
    }

    private BattleRotationInputType _battleRotationInput;
    public BattleRotationInputType BattleRotationInput
    {
        get => _battleRotationInput;
        set
        {
            if (_battleRotationInput == value) return;
            _battleRotationInput = value;
            PlayerPrefs.SetInt(BattleRotationInputKey, (int)value);
        }
    }

    private float _battleSwipeMinDistance;
    public float BattleSwipeMinDistance
    {
        get => _battleSwipeMinDistance;
        set
        {
            if (_battleSwipeMinDistance == value) return;
            _battleSwipeMinDistance = value;
            PlayerPrefs.SetFloat(BattleSwipeMinDistanceKey, (float)value);
        }
    }

    private float _battleSwipeMaxDistance;
    public float BattleSwipeMaxDistance
    {
        get => _battleSwipeMaxDistance;
        set
        {
            if (_battleSwipeMaxDistance == value) return;
            _battleSwipeMaxDistance = value;
            PlayerPrefs.SetFloat(BattleSwipeMaxDistanceKey, (float)value);
        }
    }

    private float _battleSwipeSensitivity;
    public float BattleSwipeSensitivity
    {
        get => _battleSwipeSensitivity;
        set
        {
            if (_battleSwipeSensitivity == value) return;
            _battleSwipeSensitivity = value;
            PlayerPrefs.SetFloat(BattleSwipeSensitivityKey, (float)value);
        }
    }

    private float _battleGyroMinAngle;
    public float BattleGyroMinAngle
    {
        get => _battleGyroMinAngle;
        set
        {
            if (_battleGyroMinAngle == value) return;
            _battleGyroMinAngle = value;
            PlayerPrefs.SetFloat(BattleGyroMinAngleKey, (float)value);
        }
    }

    private TopBarStyle _topBarStyleSetting;
    public TopBarStyle TopBarStyleSetting
    {
        get => _topBarStyleSetting;
        set
        {
            if (_topBarStyleSetting == value) return;
            _topBarStyleSetting = value;
            PlayerPrefs.SetInt(TopBarStyleSettingKey, (int)value);
            OnTopBarChanged?.Invoke((int)value);
        }
    }



    public static bool IsTopBarItemVisibleByKeyStatic(string key, bool defaultOn = true)
        => PlayerPrefs.GetInt(key, defaultOn ? 1 : 0) != 0;


    private const string _topBarOrderKeyPrefix = "TopBarOrder_";
    private static string GetTopBarOrderKey(TopBarStyle style) => _topBarOrderKeyPrefix + style;


    private string _mainMenuMusicName;
    public string MainMenuMusicName { get { return _mainMenuMusicName; } }

    public enum SelectionBoxType
    {
        None,
        MainMenuMusic
    }

    // Functions
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        Application.targetFrameRate = PlayerPrefs.GetInt("TargetFrameRate", (int)Screen.currentResolution.refreshRateRatio.value);
        mainMenuWindowIndex = 0;

        _language = ParseLanguage(PlayerPrefs.GetString("LanguageType", ""));

        _textSize = (TextSize)PlayerPrefs.GetInt("TextSize", 1);
        _showButtonLabels = (PlayerPrefs.GetInt("showButtonLabels", 1) == 1);

        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1);
        menuVolume = PlayerPrefs.GetFloat("MenuVolume", 1);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
        soundVolume = PlayerPrefs.GetFloat("SoundVolume", 1);

        jukeboxSoulhome = PlayerPrefs.GetInt("JukeboxSoulHome", 1) != 0;
        jukeboxUI = PlayerPrefs.GetInt("JukeboxUI",1) != 0;
        jukeboxBattle = PlayerPrefs.GetInt("JukeboxBattle", 0) != 0;

        _battleShowDebugStatsOverlay = PlayerPrefs.GetInt(BattleShowDebugStatsOverlayKey, 0) == 1;

        _battleArenaScale = PlayerPrefs.GetInt(BattleArenaScaleKey, BattleArenaScaleDefault);
        _battleArenaPosX = PlayerPrefs.GetInt(BattleArenaPosXKey, BattleArenaPosXDefault);
        _battleArenaPosY = PlayerPrefs.GetInt(BattleArenaPosYKey, BattleArenaPosYDefault);

        _battleMovementInput = (BattleMovementInputType)PlayerPrefs.GetInt(BattleMovementInputKey, (int)BattleMovementInputDefault);
        _battleRotationInput = (BattleRotationInputType)PlayerPrefs.GetInt(BattleRotationInputKey, (int)BattleRotationInputDefault);

        _battleSwipeMinDistance = PlayerPrefs.GetFloat(BattleSwipeMinDistanceKey, BattleSwipeMinDistanceDefault);
        _battleSwipeMaxDistance = PlayerPrefs.GetFloat(BattleSwipeMaxDistanceKey, BattleSwipeMaxDistanceDefault);
        _battleSwipeSensitivity = PlayerPrefs.GetFloat(BattleSwipeSensitivityKey, BattleSwipeSensitivityDefault);
        _battleGyroMinAngle = PlayerPrefs.GetFloat(BattleGyroMinAngleKey, BattleGyroMinAngleDefault);

        _unlimitedStatUpgradeMaterials = PlayerPrefs.GetInt(UnlimitedStatUpgradeMaterialsKey, 1) == 1;

        _statDebuggingMode = /*PlayerPrefs.GetInt(StatDebuggingModeKey, 1) == 1*/true;

        _topBarStyleSetting = (TopBarStyle)PlayerPrefs.GetInt(TopBarStyleSettingKey, 1);
        OnTopBarChanged?.Invoke((int)_topBarStyleSetting);

        _mainMenuMusicName = PlayerPrefs.GetString("MainMenuMusic");
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

    public bool SetBoolValue(SettingsType type, bool? value = null)
    {
        switch (type)
        {
            case SettingsType.None:
                return false;
            case SettingsType.JukeboxSoulhomeToggle:
                jukeboxSoulhome = value ?? !jukeboxSoulhome;
                PlayerPrefs.SetInt("JukeboxSoulHome", jukeboxSoulhome ? 1 : 0);
                return true;
            case SettingsType.JukeboxUIToggle:
                jukeboxUI = value ?? !jukeboxUI;
                PlayerPrefs.SetInt("JukeboxUI", jukeboxUI ? 1 : 0);
                return true;
            case SettingsType.JukeboxBattleToggle:
                jukeboxBattle = value ?? !jukeboxBattle;
                PlayerPrefs.SetInt("JukeboxBattle", jukeboxBattle ? 1 : 0);
                return true;
            default:
                Debug.LogError($"Cannot find type: {type}. Somebody probably forgot to add it.");
                return false;
        }
    }

    public bool? GetBoolValue(SettingsType type)
    {
        switch (type)
        {
            case SettingsType.None:
                return null;
            case SettingsType.JukeboxSoulhomeToggle:
                return jukeboxSoulhome;
            case SettingsType.JukeboxUIToggle:
                return jukeboxUI;
            case SettingsType.JukeboxBattleToggle:
                return jukeboxBattle;
            default:
                Debug.LogError($"Cannot find type: {type}. Somebody probably forgot to add it.");
                return null;
        }
    }

    public BattleUiMovableElementData GetBattleUiMovableElementData(BattleUiElementType type)
    {
        if (type == BattleUiElementType.None) return null;

        string json = PlayerPrefs.GetString($"BattleUi{type}", string.Empty);
        if (string.IsNullOrEmpty(json)) return null;

        BattleUiMovableElementData data = JsonUtility.FromJson<BattleUiMovableElementData>(json);
        if (data != null) data.UiElementType = data.UiElementType == BattleUiElementType.None ? type : data.UiElementType; // Backwards compatibility with old BattleUiMovableElementData

        return data;
    }

    public void SetBattleUiMovableElementData(BattleUiElementType type, BattleUiMovableElementData data)
    {
        if (type == BattleUiElementType.None) return;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString($"BattleUi{type}", json);
    }

    public bool CanPlayJukeboxInArea(JukeboxPlayArea playArea)
    {
        switch (playArea)
        {
            case JukeboxPlayArea.MainMenu:
                {
                    return jukeboxUI;
                }
            case JukeboxPlayArea.Soulhome:
                {
                    return jukeboxSoulhome;
                }
            case JukeboxPlayArea.Battle:
                {
                    return jukeboxBattle;
                }
            default:
                return false;
        }
    }

    public string GetSelectionBoxData(SelectionBoxType type)
    {
        switch (type)
        {
            case SelectionBoxType.MainMenuMusic: return _mainMenuMusicName;
            default: return null;
        }
    }

    public void SetDataFromSelectionBox(SelectionBoxType type, string value)
    {
        switch (type)
        {
            case SelectionBoxType.MainMenuMusic: _mainMenuMusicName = value; PlayerPrefs.SetString("MainMenuMusic", value); break;
        }
    }

    public void SetTopBarItemVisibleByKey(string key, bool visible)
    {
        int newV = visible ? 1 : 0;
        int oldV = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : -1;
        if (oldV == newV) return;

        PlayerPrefs.SetInt(key, newV);
        OnTopBarChanged?.Invoke((int)TopBarStyleSetting);
    }

    [System.Serializable]
    private class TopBarOrderData
    {
        public List<int> order = new List<int>();
    }

    public void SaveTopBarOrder(TopBarStyle style, IList<int> order)
    {
        string key = GetTopBarOrderKey(style);

        TopBarOrderData data = new TopBarOrderData { order = new List<int>(order) };
        string jsonNew = JsonUtility.ToJson(data);

        string jsonOld = PlayerPrefs.GetString(key, "");
        if (jsonOld == jsonNew) return;

        PlayerPrefs.SetString(key, jsonNew);
        OnTopBarChanged?.Invoke((int)style);
    }

    public static List<int> LoadTopBarOrderStatic(TopBarStyle style, int count)
    {
        List<int> result = new List<int>(count);
        bool[] used = new bool[count];

        string raw = PlayerPrefs.GetString(GetTopBarOrderKey(style), "");
        if (string.IsNullOrEmpty(raw))
        {
            for (int i = 0; i < count; i++) result.Add(i);
            return result;
        }

        TopBarOrderData data = JsonUtility.FromJson<TopBarOrderData>(raw);

        // JSON-lista l�pi foreachilla
        if (data != null && data.order != null)
        {
            foreach (int idx in data.order)
            {
                if ((uint)idx < (uint)count && !used[idx])
                {
                    used[idx] = true;
                    result.Add(idx);
                    if (result.Count == count) return result;
                }
            }
        }

        for (int i = 0; i < count; i++)
            if (!used[i]) result.Add(i);

        return result;
    }

    private LanguageType ParseLanguage(string languageName)
    {
        switch (languageName)
        {
            case "fi":
                return LanguageType.Finnish;
            case "en":
                return LanguageType.English;
            default:
                return LanguageType.None;
        }
    }

    private string ParseLanguage(LanguageType languageType)
    {
        switch (languageType)
        {
            case LanguageType.Finnish:
                return "fi";
            case LanguageType.English:
                return "en";
            default:
                return "";
        }
    }
}
