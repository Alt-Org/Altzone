using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class TutorialController : AltMonoBehaviour
{
    [SerializeField] private List<TutorialPanelHandler> _tutorialPanelList;
    [SerializeField] private string _tutorialPanelName;

    private int _currentPage=0;
    private string _playerName = string.Empty;

    // Start is called before the first frame update
    void Start()
    {
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => _playerName = p.Name);
        if (PlayerPrefs.GetInt(_tutorialPanelName+"_"+ _playerName, 0) == 1) return;
        Initialize();
    }

    private void Initialize()
    {
        if(_tutorialPanelList.Count <= 0) return;
        foreach (var tutorialPanel in _tutorialPanelList)
        {
            tutorialPanel.SetData(AdvanceTutorial);
        }
        _currentPage = 0;
        _tutorialPanelList[_currentPage].gameObject.SetActive(true);
    }

    private void AdvanceTutorial()
    {
        _tutorialPanelList[_currentPage].gameObject.SetActive(false);
        _currentPage++;
        if (_tutorialPanelList.Count > _currentPage)
        {
            _tutorialPanelList[_currentPage].gameObject.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt(_tutorialPanelName + "_" + _playerName, 1);
        }
    }

}
