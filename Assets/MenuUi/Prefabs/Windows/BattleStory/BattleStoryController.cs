using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using UnityEngine;
using UnityEngine.UI;

public class BattleStoryController : MonoBehaviour
{
    [SerializeField]
    private Button _exitButton;

    // Start is called before the first frame update
    void Start()
    {
        _exitButton.onClick.AddListener(ExitStory);
    }

    private void ExitStory()
    {
        LobbyManager.ExitBattleStory();
    }

}
