using System.Collections.Generic;
using MenuUi.Scripts.AvatarEditor;
using UnityEngine;
using Prg.Scripts.Common;

public class GameModeChoiceScript : MonoBehaviour
{
    private int currentModeInt = 0;
    private int amountOfModes = 3;


    [SerializeField] private List<GameObject> GameModeButtons = new List<GameObject>();

    [SerializeField] private List<GameObject> GameModeHeaders = new List<GameObject>();



    //For the game mode swipe functionality
    private Vector2 _startTouchPosition;
    private Vector2 _endTouchPosition;

    private float _minimunScrollDistance = 100; //so that movements too small wont swipe

    public void GetSwipeStart()
    {
        _startTouchPosition = ClickStateHandler.GetClickPosition();
    }


    public void GetSwipeEnd()
    {
        _endTouchPosition = ClickStateHandler.GetClickPosition();
        SwipeToMode();
    }

    private void SwipeToMode()
    {

        if (_startTouchPosition.x < _endTouchPosition.x && Mathf.Abs(_endTouchPosition.x) - Mathf.Abs(_startTouchPosition.x) > _minimunScrollDistance)
        {
            PressArrowLeft();
        }
        else if (_startTouchPosition.x > _endTouchPosition.x && Mathf.Abs(_startTouchPosition.x) - Mathf.Abs(_endTouchPosition.x) > _minimunScrollDistance)
        {
            PressArrowRight();
        }
    }



    private void Start()
    {
        currentModeInt = 0;

        foreach (GameObject gameModeButton in GameModeButtons)
        {
            gameModeButton.SetActive(false);
        }
        GameModeButtons[0].SetActive(true);

        foreach (GameObject gameModeHeader in GameModeHeaders)
        {
            gameModeHeader.SetActive(false);
        }
        GameModeHeaders[0].SetActive(true);

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

        foreach (GameObject gameModeButton in GameModeButtons)  //make correct game mode button active
        {
            gameModeButton.SetActive(false);
        }
        GameModeButtons[currentModeInt].SetActive(true);


        foreach (GameObject gameModeHeader in GameModeHeaders) //make correct header active
        {
            gameModeHeader.SetActive(false);
        }
        GameModeHeaders[currentModeInt].SetActive(true);
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

        foreach (GameObject gameModeButton in GameModeButtons) //make correct game mode button active
        {
            gameModeButton.SetActive(false);
        }
        GameModeButtons[currentModeInt].SetActive(true);

        foreach (GameObject gameModeHeader in GameModeHeaders)//make correct header active
        {
            gameModeHeader.SetActive(false);
        }
        GameModeHeaders[currentModeInt].SetActive(true);
    }
}
