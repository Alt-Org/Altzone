using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;
using static SettingsCarrier;

public class LanguageSelectorHandler : MonoBehaviour
{
    [SerializeField] private Button _finnishSelectButton;
    [SerializeField] private Button _englishSelectButton;

    [SerializeField] private WindowNavigation _navigation;

    // Start is called before the first frame update
    void Start()
    {
        _finnishSelectButton.onClick.AddListener(() => SetLanguage(LanguageType.Finnish));
        _englishSelectButton.onClick.AddListener(() => SetLanguage(LanguageType.English));
    }

    private void OnDestroy()
    {
        _finnishSelectButton.onClick.RemoveAllListeners();
        _englishSelectButton.onClick.RemoveAllListeners();
    }

    private void SetLanguage(LanguageType type)
    {
        SettingsCarrier.Instance.Language = type;
        if(_navigation != null) StartCoroutine(_navigation.Navigate());

        if (TryGetComponent(out DailyTaskProgressListener dtListener))
        {
            dtListener.UpdateProgress("1");
        }
    }
}
