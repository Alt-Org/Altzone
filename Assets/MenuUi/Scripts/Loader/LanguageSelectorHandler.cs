using System;
using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

using static SettingsCarrier;

public class LanguageSelectorHandler : MonoBehaviour
{
    [SerializeField] private Button _decrease;
    [SerializeField] private Button _increase;

    [SerializeField] private WindowNavigation _navigation;
    [SerializeField] private Image _flagHolder;
    [SerializeField] private Sprite[] _flagsImages;

    // Start is called before the first frame update
    void Start()
    {
        _decrease.onClick.AddListener(() => SetLanguage(-1));
        _increase.onClick.AddListener(() => SetLanguage(1));


        _flagHolder.sprite = _flagsImages[(int)Localization.Language];

    }

    private void SetLanguage(int value)
    {

        ///RN the code is like this because other languages do not work as is now but when they do just modify the <see cref="int max"/>.Length -2 into -1 and the index = 1 into 0 instead
        int index = (int)Localization.Language;
        int max = (int)(LanguageType)Enum.GetValues(typeof(LanguageType)).Length - 2;
        index += value;

        if (index > max)
        {
            index = 1;
        }
        else if (index < 1)
        {
            index = max;
        }

        Localization.Language = (LanguageType)index;
        _flagHolder.sprite = _flagsImages[(int)Localization.Language];

        if (_navigation != null) StartCoroutine(_navigation.Navigate());

        if (TryGetComponent(out DailyTaskProgressListener dtListener))
        {
            dtListener.UpdateProgress("1");
        }
    }
}
