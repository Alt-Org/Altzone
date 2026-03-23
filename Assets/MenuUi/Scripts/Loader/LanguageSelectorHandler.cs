using System;
using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

using static SettingsCarrier;

public class LanguageSelectorHandler : MonoBehaviour
{
    SettingsCarrier carrier = SettingsCarrier.Instance;
    [SerializeField] private Button _decrease;
    [SerializeField] private Button _increase;

    [SerializeField] private WindowNavigation _navigation;
    [SerializeField] private int _buttonValue;
    [SerializeField] private Image _flagHolder;
    [SerializeField] private Sprite[] _flagsImages;

    // Start is called before the first frame update
    void Start()
    {
        _decrease.onClick.AddListener(() => SetLanguage());
        _increase.onClick.AddListener(() => SetLanguage());


        _flagHolder.sprite = _flagsImages[(int)SettingsCarrier.Instance.Language];

    }

    private void OnDestroy()
    {
    }

    public void ChangeValue(int amount)
    {
        _buttonValue = amount;
    }


    private void SetLanguage()
    {

        ///RN the code is like this because other languages do not work as is now but when they do just modify the <see cref="int max"/>.Length -2 into -1 and the index = 1 into 0 instead
        int index = (int)carrier.Language;
        int max = (int)(SettingsCarrier.LanguageType)Enum.GetValues(typeof(SettingsCarrier.LanguageType)).Length - 2;
        index += _buttonValue;

        if (index > max)
        {
            index = 1;
        }
        else if (index < 1)
        {
            index = max;
        }

        SettingsCarrier.Instance.Language = (SettingsCarrier.LanguageType)index;
        _flagHolder.sprite = _flagsImages[(int)SettingsCarrier.Instance.Language];

        if (_navigation != null) StartCoroutine(_navigation.Navigate());

        if (TryGetComponent(out DailyTaskProgressListener dtListener))
        {
            dtListener.UpdateProgress("1");
        }
    }
}
