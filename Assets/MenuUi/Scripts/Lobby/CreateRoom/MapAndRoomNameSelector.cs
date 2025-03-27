using Altzone.Scripts.ModelV2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Lobby.CreateRoom
{
    /// <summary>
    /// Used for selecting the game map and room name when creating a custom game from the Battle Popup.
    /// </summary>
    public class MapAndRoomNameSelector : MonoBehaviour
    {
        [Header("Map selector")]
        [SerializeField] private TextMeshProUGUI _currentMapText;
        [SerializeField] private Button _nextMapButton;
        [SerializeField] private Button _previousMapButton;

        [Header("Room name selector")]
        [SerializeField] private TextMeshProUGUI _currentNameText;
        [SerializeField] private Button _nextNameButton;
        [SerializeField] private Button _previousNameButton;

        [Header("Reference sheet")]
        [SerializeField] private BattleMapReference _battleMapReference;

        public BattleMap SelectedBattleMap { get; private set; }
        public MapEmotionalSituation SelectedEmotionalSituation { get; private set; }

        private void Awake()
        {
            //_nextButton.onClick.AddListener(OnNextOptionClicked);
            //_previousButton.onClick.AddListener(OnPreviousOptionClicked);
            //_currentGameModeText.text = SelectedGameMode.GetString();
        }

        private void OnNextOptionClicked()
        {
            //bool isLast = SelectedGameMode == Enum.GetValues(typeof(CustomGameMode)).Cast<CustomGameMode>().Last();
            //SelectedGameMode = isLast ? 0 : SelectedGameMode + 1;
            //_currentGameModeText.text = SelectedGameMode.GetString();
        }

        private void OnPreviousOptionClicked()
        {
            //bool isFirst = (int)SelectedGameMode <= 0;
            //SelectedGameMode = isFirst ? Enum.GetValues(typeof(CustomGameMode)).Cast<CustomGameMode>().Last() : SelectedGameMode - 1;
            //_currentGameModeText.text = SelectedGameMode.GetString();
        }
    }
}
