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

    [SerializeField, Header("Battle Button")] private BattleButton _gameModeButton;

    [SerializeField, Header("Selection Buttons")] private Button _arrowLeft;
    [SerializeField] private Button _arrowRight;

    private int currentModeInt = 0;
    private int amountOfModes;

    private List<GameTypeInfo> _list;

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
            _gameModeButton.Button.onClick.Invoke();
        }
        _isSwiping = false;
    }



    //for switching game modes


    private void Start()
    {
        currentModeInt = 0;

        // Build list from only enabled game types so UI matches enabled count
        _list = GameTypeReference.Instance.GetGameTypeInfos()
            .Where(x => x.Enabled)
            .OrderBy(x => x.gameType)
            .ToList();

        amountOfModes = _list.Count;

        if (amountOfModes > 0)
        {
            currentModeInt = _list.FindIndex(x => x.gameType == _gameModeButton.SelectedGameType);
            if (currentModeInt < 0) currentModeInt = 0;
            SetData();
        }

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
        GameTypeInfo gameInfo = _list[currentModeInt];

        _gameModeButton.UpdateGameType(gameInfo);
    }
}
