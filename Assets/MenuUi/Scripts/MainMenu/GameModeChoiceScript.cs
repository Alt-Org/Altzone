using System.Collections.Generic;
using UnityEngine;
using Prg.Scripts.Common;
using UnityEngine.UI;
using MenuUi.Scripts.Lobby.BattleButton;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.ReferenceSheets;
using System.Linq;


public class GameModeChoiceScript : MonoBehaviour
{

    [SerializeField, Header("Game Objects")] private GameObject _gameModeButton;

    [SerializeField, Header("Buttons")] private Button _arrowLeft;
    [SerializeField] private Button _arrowRight;

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
            _gameModeButton.GetComponent<Button>().onClick.Invoke();
        }
        _isSwiping = false;
    }



    //for switching game modes


    private void Start()
    {
        currentModeInt = 0;
        amountOfModes = GameTypeReference.Instance.GetEnabledCount();

        SetData();

        _arrowLeft.onClick.AddListener(PressArrowLeft);
        _arrowRight.onClick.AddListener(PressArrowRight);
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

        SetData();
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

        SetData();
    }

    private void SetData()
    {
        List<GameTypeInfo> list = GameTypeReference.Instance.GetGameTypeInfos().OrderBy(x => x.gameType).ToList();

        GameTypeInfo gameInfo = list[currentModeInt];

        _gameModeButton.GetComponent<BattleButton>().UpdateGameType(gameInfo);
    }
}
