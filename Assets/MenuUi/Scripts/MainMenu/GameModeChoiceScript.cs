using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Language;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.Lobby.BattleButton;
using MenuUi.Scripts.ReferenceSheets;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;

public class GameModeChoiceScript : MonoBehaviour
{
    private int currentModeInt = 0;
    private int amountOfModes = 3;


    [SerializeField] private List<GameObject> GameModeButtons = new List<GameObject>();

    [SerializeField] private List<GameObject> GameModeHeaders = new List<GameObject>();



    

    [SerializeField] private Image _gameTypeIcon;
    [SerializeField] private TextLanguageSelectorCaller _gameTypeName;
    [SerializeField] private TextLanguageSelectorCaller _gameTypeDescription;
    [SerializeField] private GameObject _gameTypeSelectionMenu;
    [SerializeField] private GameObject _gameTypeOptionPrefab;
    [SerializeField] private Button _openBattleUiEditorButton;
    [SerializeField] private GameTypeReference _gameTypeReference;
    [SerializeField] private GameObject _touchBlocker;

    private const string SelectedGameTypeKey = "BattleButtonGameType";

    private GameType _selectedGameType = GameType.Custom;

    private List<GameTypeOption> _gameTypeOptionList = new();
    private Button _button;
    private SwipeUI _swipe;

    private void Awake()
    {
        GameTypeOption gameTypeOption = Instantiate(_gameTypeOptionPrefab).GetComponent<GameTypeOption>();


        // Instantiate game type option buttons to game type selection menu
        foreach (GameTypeInfo gameTypeInfo in _gameTypeReference.GetGameTypeInfos())
        {

            int i = 0;

            GameModeButtons[i] = _gameTypeOptionPrefab;


            i++;
        
            
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
