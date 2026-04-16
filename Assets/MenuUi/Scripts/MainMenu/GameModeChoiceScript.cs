using System.Collections.Generic;
using UnityEngine;
using Prg.Scripts.Common;
using UnityEngine.UI;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Signals;


public class GameModeChoiceScript : MonoBehaviour
{

    [SerializeField] private List<GameObject> _gameModeButtons = new();
    [SerializeField] private List<GameObject> _gameModeHeaders = new();
    [SerializeField] private List<Button> _gameModeButtonsAsButtons = new();
    [SerializeField] private List<GameType> _gameModeTypes = new() { GameType.Custom, GameType.Random2v2, GameType.FriendLobby };

    private int currentModeInt = 0;
    private int amountOfModes;


    //For the game mode swipe functionality
    private Vector2 _startTouchPosition;
    private Vector2 _endTouchPosition;

    private float _minimunScrollDistance = 100; //so that movements too small wont swipe

    private bool _isSwiping = false;


    public void GetSwipeStart()
    {
        _startTouchPosition = ClickStateHandler.GetClickPosition();
        _isSwiping = true;
    }


    public void GetSwipeEnd()
    {
        _endTouchPosition = ClickStateHandler.GetClickPosition();

        SwipeToMode();
    }

    private void SwipeToMode()
    {
        //Debug.Log("start: " + _startTouchPosition + " end: " + _endTouchPosition);
        if (_startTouchPosition.x < _endTouchPosition.x && Mathf.Abs(_endTouchPosition.x) - Mathf.Abs(_startTouchPosition.x) > _minimunScrollDistance)
        {
            PressArrowLeft();
        }
        else if (_startTouchPosition.x > _endTouchPosition.x && Mathf.Abs(_startTouchPosition.x) - Mathf.Abs(_endTouchPosition.x) > _minimunScrollDistance)
        {
            PressArrowRight();
        }
    }


    public void PointerUp() //triggers button if swipe didn't happen
    {
        if (!_isSwiping)
        {
            OpenCurrentMode();
        }
        _isSwiping = false;
    }



    //for switching game modes


    private void Start()
    {
        currentModeInt = 0;
        amountOfModes = Mathf.Min(_gameModeButtons.Count, _gameModeTypes.Count);
        SetCurrentModeActive();

    }

    public void PressArrowLeft() 
    {
        if (currentModeInt > 0)
        {
            currentModeInt--;
        }
        else
        {
            currentModeInt = amountOfModes - 1;
        }

        SetCurrentModeActive();
    }

    public void PressArrowRight() 
    {
        if (currentModeInt < amountOfModes - 1)
        {
            currentModeInt++;
        }
        else
        {
            currentModeInt = 0;
        }

        SetCurrentModeActive();
    }

    private void SetCurrentModeActive()
    {
        foreach (GameObject gameModeButton in _gameModeButtons)
        {
            gameModeButton.SetActive(false);
        }

        foreach (GameObject gameModeHeader in _gameModeHeaders)
        {
            gameModeHeader.SetActive(false);
        }

        if (amountOfModes <= 0)
        {
            return;
        }

        int buttonIndex = Mathf.Clamp(currentModeInt, 0, amountOfModes - 1);
        _gameModeButtons[buttonIndex].SetActive(true);

        if (_gameModeHeaders.Count > 0)
        {
            int headerIndex = Mathf.Clamp(buttonIndex, 0, _gameModeHeaders.Count - 1);
            _gameModeHeaders[headerIndex].SetActive(true);
        }
    }

    private void OpenCurrentMode()
    {
        if (currentModeInt >= 0 && currentModeInt < _gameModeTypes.Count)
        {
            SignalBus.OnBattlePopupRequestedSignal(_gameModeTypes[currentModeInt]);
            return;
        }

        if (currentModeInt >= 0 && currentModeInt < _gameModeButtonsAsButtons.Count)
        {
            _gameModeButtonsAsButtons[currentModeInt].onClick.Invoke();
        }
    }
}
