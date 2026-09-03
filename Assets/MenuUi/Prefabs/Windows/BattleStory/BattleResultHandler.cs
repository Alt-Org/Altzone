using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Window;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultHandler : MonoBehaviour
{
    [SerializeField]
    private Image _ownResultImage;

    [SerializeField]
    private GameObject _teamAlphaWingsImage;
    [SerializeField]
    private GameObject _teamBetaWingsImage;

    [SerializeField]
    private Sprite _winImageEN;
    [SerializeField]
    private Sprite _winImageFI;
    [SerializeField]
    private Sprite _loseImageEN;
    [SerializeField]
    private Sprite _loseImageFI;

    public void SetBattleResult(bool result)
    {
        if (result)
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
            {
                _ownResultImage.sprite = _winImageFI;
            }
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
            {
                _ownResultImage.sprite = _winImageEN;
            }
        }
        else
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
            {
                _ownResultImage.sprite = _loseImageFI;
            }
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
            {
                _ownResultImage.sprite = _loseImageEN;
            }
        }

        _teamAlphaWingsImage.SetActive(false);
        _teamBetaWingsImage.SetActive(false);

        int winningTeam = DataCarrier.GetData<int>(DataCarrier.BattleWinner, clear: false, suppressWarning: true);



        if (winningTeam == 1)
        {
            _teamAlphaWingsImage.SetActive(true);
        }
        else if (winningTeam == 2)
        {
            _teamBetaWingsImage.SetActive(true);
        }

    }
}
