using System.Collections.Generic;
using UnityEngine;
using Prg.Scripts.Common;
using UnityEngine.UI;
using MenuUi.Scripts.Lobby.BattleButton;
using Altzone.Scripts.Lobby;


public class GameModeChoiceScript : MonoBehaviour
{

    [SerializeField] private List<GameObject> _gameModeButtons = new();
    [SerializeField] private List<GameObject> _gameModeHeaders = new();
    [SerializeField] private List<Button> _gameModeButtonsAsButtons = new();

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
            _gameModeButtonsAsButtons[currentModeInt].onClick.Invoke();
        }
        _isSwiping = false;
    }



    //for switching game modes


    private void Start()
    {
        currentModeInt = 0;
        amountOfModes = _gameModeButtons.Count;

        foreach (GameObject gameModeButton in _gameModeButtons)
        {
            gameModeButton.SetActive(false);
        }
        _gameModeButtons[0].SetActive(true);

        foreach (GameObject gameModeHeader in _gameModeHeaders)
        {
            gameModeHeader.SetActive(false);
        }
        _gameModeHeaders[0].SetActive(true);

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

        foreach (GameObject gameModeButton in _gameModeButtons)  //make correct game mode button active
        {
            gameModeButton.SetActive(false);
        }
        _gameModeButtons[currentModeInt].SetActive(true);


        foreach (GameObject gameModeHeader in _gameModeHeaders) //make correct header active
        {
            gameModeHeader.SetActive(false);
        }
        _gameModeHeaders[currentModeInt].SetActive(true);

        switch (currentModeInt)
        {
            case 0:
                _gameModeButtons[currentModeInt].GetComponent<BattleButton>().SelectedGameType = GameType.Custom;
                break;
            case 1:
                _gameModeButtons[currentModeInt].GetComponent<BattleButton>().SelectedGameType = GameType.Random2v2;
                break;
        }

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

        foreach (GameObject gameModeButton in _gameModeButtons) //make correct game mode button active
        {
            gameModeButton.SetActive(false);
        }
        _gameModeButtons[currentModeInt].SetActive(true);

        foreach (GameObject gameModeHeader in _gameModeHeaders)//make correct header active
        {
            gameModeHeader.SetActive(false);
        }
        _gameModeHeaders[currentModeInt].SetActive(true);

        switch (currentModeInt)
        {
            case 0:
                _gameModeButtons[currentModeInt].GetComponent<BattleButton>().SelectedGameType = GameType.Custom;
                break;
            case 1:
                _gameModeButtons[currentModeInt].GetComponent<BattleButton>().SelectedGameType = GameType.Random2v2;
                break;
        }
    }
}
