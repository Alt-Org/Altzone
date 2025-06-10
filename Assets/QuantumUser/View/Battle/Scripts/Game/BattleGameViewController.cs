using UnityEngine;

using Quantum;
using Photon.Deterministic;

using Altzone.Scripts.BattleUiShared;
using Altzone.Scripts.Lobby;

using Battle.QSimulation.Player;
using Battle.View.UI;
using Battle.View.Effect;
using Battle.View.Audio;
using PlayerType = Battle.View.UI.BattleUiPlayerInfoHandler.PlayerType;

using BattleUiElementType = SettingsCarrier.BattleUiElementType;

namespace Battle.View.Game
{
    public class BattleGameViewController : QuantumCallbacks
    {
        [SerializeField] private BattleGridViewController _gridViewController;
        [SerializeField] private BattleUiController _uiController;
        [SerializeField] private BattleScreenEffectViewController _screenEffectViewController;
        [SerializeField] private BattleStoneCharacterViewController _stoneCharacterViewController;
        [SerializeField] private BattleLightrayEffectViewController _lightrayEffectViewController;
        [SerializeField] private BattleSoundFXViewController _soundFXViewController;

        public static BattlePlayerSlot LocalPlayerSlot { get; private set; }
        public static BattleTeamNumber LocalPlayerTeam { get; private set; }

        public static GameObject ProjectileReference { get; private set; }

        public static void AssignProjectileReference(GameObject projectileReference)
        {
            ProjectileReference = projectileReference;
        }

        public void UiInputOnLocalPlayerGiveUp()
        {
            Debug.Log("Give up button pressed!");
        }

        public void UiInputOnCharacterSelected(int characterNumber)
        {
            Debug.Log($"Character number {characterNumber} selected!");
        }

        public void UiInputOnExitGamePressed()
        {
            if (_endOfGameDataHasEnded) LobbyManager.ExitQuantum(_endOfGameDataWinningTeam, (float)_endOfGameDataGameLengthSec);
        }

        private bool _endOfGameDataHasEnded = false;
        private BattleTeamNumber _endOfGameDataWinningTeam;
        private FP _endOfGameDataGameLengthSec;

        private void Awake()
        {
            // Showing announcement handler and setting view pre-activate loading text
            _uiController.AnnouncementHandler.SetShow(true);
            _uiController.AnnouncementHandler.SetText(BattleUiAnnouncementHandler.TextType.Loading);

            // Subscribing to Game Flow events
            QuantumEvent.Subscribe<EventBattleViewWaitForPlayers>(this, QEventOnViewWaitForPlayers);
            QuantumEvent.Subscribe<EventBattleViewInit>(this, QEventOnViewInit);
            QuantumEvent.Subscribe<EventBattleViewActivate>(this, QEventOnViewActivate);
            QuantumEvent.Subscribe<EventBattleViewGetReadyToPlay>(this, QEventOnViewGetReadyToPlay);
            QuantumEvent.Subscribe<EventBattleViewGameStart>(this, QEventOnViewGameStart);
            QuantumEvent.Subscribe<EventBattleViewGameOver>(this, QEventOnViewGameOver);

            // Subscribing to other View Init events
            QuantumEvent.Subscribe<EventBattleStoneCharacterPieceViewInit>(this, QEventOnStoneCharacterPieceViewInit);

            // Subscribing to Gameplay events
            QuantumEvent.Subscribe<EventBattleChangeEmotionState>(this, QEventOnChangeEmotionState);
            QuantumEvent.Subscribe<EventBattleLastRowWallDestroyed>(this, QEventOnLastRowWallDestroyed);
            QuantumEvent.Subscribe<EventBattlePlaySoundFX>(this, QEventPlaySoundFX);

            // Subscribing to Debug events
            QuantumEvent.Subscribe<EventBattleDebugUpdateStatsOverlay>(this, QEventDebugOnUpdateStatsOverlay);
        }

        private void QEventOnViewWaitForPlayers(EventBattleViewWaitForPlayers e)
        {
            // Setting view pre-activate waiting for players text
            _uiController.AnnouncementHandler.SetText(BattleUiAnnouncementHandler.TextType.WaitingForPlayers);
        }

        private void QEventOnViewInit(EventBattleViewInit e)
        {
            // Getting LocalPlayerSlot and LocalPlayerTeam
            if (Utils.TryGetQuantumFrame(out Frame f))
            {
                PlayerRef playerRef = QuantumRunner.Default.Game.GetLocalPlayers()[0];
                LocalPlayerSlot = BattlePlayerManager.PlayerHandle.GetSlot(f, playerRef);
                LocalPlayerTeam = BattlePlayerManager.PlayerHandle.GetTeamNumber(LocalPlayerSlot);
            }

            // Initializing BattleGridViewController
            if (_gridViewController != null)
            {
                _gridViewController.SetGrid();
            }

            //{ Initializing UI Handlers

            if (_uiController.DiamondsHandler != null)
            {
                _uiController.DiamondsHandler.SetDiamondsText(0);

                BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.Diamonds);
                if (data != null) _uiController.DiamondsHandler.MovableUiElement.SetData(data);
            }

            if (_uiController.TimerHandler != null)
            {
                BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.Timer);
                if (data != null) _uiController.TimerHandler.MovableUiElement.SetData(data);
            }

            // Commented out code to hide the ui elements which shouldn't be shown at this point, but the code will be used later
            /*
            if (_uiController.GiveUpButtonHandler != null)
            {
                BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.GiveUpButton);
                if (data != null) _uiController.GiveUpButtonHandler.MovableUiElement.SetData(data);
            }

            if (_uiController.PlayerInfoHandler != null)
            {
                // Setting local player info
                _uiController.PlayerInfoHandler.SetInfo(
                    PlayerType.LocalPlayer,
                    "Minä",
                    new int[3] { 101, 201, 301 },
                    SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.PlayerInfo)
                );

                // Setting local teammate info
                _uiController.PlayerInfoHandler.SetInfo(
                    PlayerType.LocalTeammate,
                    "Tiimiläinen",
                    new int[3] { 401, 501, 601 },
                    SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.TeammateInfo)
                );
            }
            */

            //} Initializing UI Handlers
        }

        private void QEventOnStoneCharacterPieceViewInit(EventBattleStoneCharacterPieceViewInit e)
        {
            if (_stoneCharacterViewController != null)
            {
                _stoneCharacterViewController.SetEmotionIndicator(e.WallNumber, e.Team, e.EmotionIndicatorColorIndex);
            }
        }

        private void QEventOnViewActivate(EventBattleViewActivate e)
        {
            // Activating view, meaning displaying all visual elements of the game view except for pre-activation elements

            // Show UI elements
            if (_uiController.DiamondsHandler != null) _uiController.DiamondsHandler.SetShow(true);
            /* These UI elements aren't ready and shouldn't be shown yet
            if (_uiController.GiveUpButtonHandler != null) _uiController.GiveUpButtonHandler.SetShow(true);
            if (_uiController.PlayerInfoHandler != null) _uiController.PlayerInfoHandler.SetShow(true);
            */

            // Load settings and set BattleCamera to show game scene with previously loaded settings
            BattleCamera.SetView(
                SettingsCarrier.Instance.BattleArenaScale * 0.01f,
                new(SettingsCarrier.Instance.BattleArenaPosX * 0.01f, SettingsCarrier.Instance.BattleArenaPosY * 0.01f),
                LocalPlayerTeam == BattleTeamNumber.TeamBeta
            );
        }

        private void QEventOnViewGetReadyToPlay(EventBattleViewGetReadyToPlay e)
        {
            // Show end of countdown text in announcement handler
            _uiController.AnnouncementHandler.SetText(BattleUiAnnouncementHandler.TextType.EndOfCountdown);
        }

        private void QEventOnViewGameStart(EventBattleViewGameStart e)
        {
            // Clear the countdown text
            _uiController.AnnouncementHandler.ClearAnnouncerTextField();

            // Show the timer
            _uiController.TimerHandler.SetShow(true);
        }

        private void QEventOnViewGameOver(EventBattleViewGameOver e)
        {
            // Hiding UI elements
            _uiController.TimerHandler.SetShow(false);
            _uiController.DiamondsHandler.SetShow(false);
            _uiController.GiveUpButtonHandler.SetShow(false);
            _uiController.PlayerInfoHandler.SetShow(false);

            // If the game is over, display "Game Over!" and show the Game Over UI
            _uiController.GameOverHandler.SetShow(true);

            // Setting end of game data variables
            _endOfGameDataHasEnded = true;
            _endOfGameDataWinningTeam = e.WinningTeam;
            _endOfGameDataGameLengthSec = e.GameLengthSec;
        }

        private void QEventOnChangeEmotionState(EventBattleChangeEmotionState e)
        {
            if (!_screenEffectViewController.IsActive) _screenEffectViewController.SetActive(true);
            _screenEffectViewController.ChangeColor((int)e.Emotion);
        }

        private void QEventOnLastRowWallDestroyed(EventBattleLastRowWallDestroyed e)
        {
            if (_stoneCharacterViewController != null)
            {
                _stoneCharacterViewController.DestroyCharacterPart(e.WallNumber, e.Team);
            }

            if (_lightrayEffectViewController != null)
            {
                _lightrayEffectViewController.SpawnLightray(e.WallNumber, e.LightrayColor);
            }
        }

        private void QEventPlaySoundFX(EventBattlePlaySoundFX e)
        {
            _soundFXViewController.PlaySound(e.Effect);
        }

        private void QEventDebugOnUpdateStatsOverlay(EventBattleDebugUpdateStatsOverlay e)
        {
            if (!SettingsCarrier.Instance.BattleShowDebugStatsOverlay) return;
            _uiController.DebugStatsOverlayHandler.SetShow(true);
            _uiController.DebugStatsOverlayHandler.SetStats(e.Stats);
        }

        // Handles UI updates based on the game's state and countdown
        private void Update()
        {
            // Try to get the current Quantum frame
            if (Utils.TryGetQuantumFrame(out Frame frame))
            {
                // Try to retrieve the singleton entity reference for the GameSession
                if (frame.TryGetSingletonEntityRef<BattleGameSessionQSingleton>(out var entity) == false)
                {
                    // If the GameSession singleton is not found, display an error message
                    Debug.LogError("GameSession singleton not found -- BattleUIHandler");
                    return;
                }

                // Retrieve the GameSession singleton from the Quantum frame
                BattleGameSessionQSingleton gameSession = frame.GetSingleton<BattleGameSessionQSingleton>();

                // Convert the countdown time to an integer for display
                int countDown = (int)gameSession.TimeUntilStart;

                // Handle different game states to update the UI
                switch (gameSession.State)
                {
                    case BattleGameState.Countdown:
                        // If the game is in the countdown state, display the countdown timer
                        _uiController.AnnouncementHandler.SetCountDownNumber(countDown);
                        break;

                    case BattleGameState.Playing:
                        // Updating diamonds (at the moment shows only alpha team's diamonds)
                        BattleDiamondCounterQSingleton diamondCounter = frame.GetSingleton<BattleDiamondCounterQSingleton>();
                        _uiController.DiamondsHandler.SetDiamondsText(diamondCounter.AlphaDiamonds);

                        // Updating timer text
                        _uiController.TimerHandler.FormatAndSetTimerText(gameSession.GameTimeSec);
                        break;
                }

            }
        }

    }
}
