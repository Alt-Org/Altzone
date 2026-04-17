using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;

public class TutorialController : AltMonoBehaviour
{
    [SerializeField] private List<TutorialPanelHandler> _tutorialPanelList;
    [SerializeField] private string _tutorialPanelName;
    [SerializeField] private SwipeUI _swipe;

    private int _currentPage=0;
    private string _playerName = string.Empty;

    private bool _inProgress = false;
    public bool IsTutorialInProgress => _inProgress;

    // Start is called before the first frame update
    void Start()
    {
        //Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => _playerName = p.Name);
        //if (PlayerPrefs.GetInt(_tutorialPanelName+"_"+ _playerName, 0) == 1) return;
        //Initialize();
    }

    public void Initialize()
    {
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => _playerName = p.Name);
        if (PlayerPrefs.GetInt(_tutorialPanelName + "_" + _playerName, 0) == 1) return;
        if (_tutorialPanelList.Count <= 0) return;
        foreach (var tutorialPanel in _tutorialPanelList)
        {
            tutorialPanel.SetData(AdvanceTutorial);
        }
        _currentPage = 0;
        _tutorialPanelList[_currentPage].gameObject.SetActive(true);
        _inProgress = true;
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
            _inProgress = false;
        }
    }

    public void RefreshPositions()
    {
        foreach(var tutorialPanel in _tutorialPanelList)
        {
            tutorialPanel.UpdatePosition();
        }
    }

}
