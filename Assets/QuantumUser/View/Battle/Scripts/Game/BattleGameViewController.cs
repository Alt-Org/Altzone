/// @file BattleGameViewController.cs
/// <summary>
/// Has a class BattleGameViewController which controls the %Battle %UI elements.
/// </summary>
///
/// This script:<br/>
/// Initializes %Battle %UI elements, and controls their visibility and functionality.

using UnityEngine;
using Quantum;
using Photon.Deterministic;

using Altzone.Scripts.BattleUiShared;
using Altzone.Scripts.Lobby;

using Battle.View.Audio;
using Battle.View.Effect;
using Battle.View.UI;
using Battle.View.Player;
using Battle.QSimulation.Player;

using BattleMovementInputType = SettingsCarrier.BattleMovementInputType;
using BattleRotationInputType = SettingsCarrier.BattleRotationInputType;
using BattleUiElementType = SettingsCarrier.BattleUiElementType;
using PlayerType = Battle.View.UI.BattleUiPlayerInfoHandler.PlayerType;

namespace Battle.View.Game
{
    /// <summary>
    /// <span class="brief-h">Game view <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_callbacks.html">QuantumCallbacks class@u-exlink</a>.</span><br/>
    /// Initializes %Battle %UI elements, and controls their visibility and functionality.
    /// </summary>
    ///
    /// Handles any functionality needed by the @uihandlerslink which need to access other parts of %Battle, for example triggering the selection of another character.
    /// Accesses all of the @ref UIHandlerReferences through the BattleUiController reference variable #_uiController.<br/>
    public class BattleGameViewController : QuantumCallbacks
    {
        #region SerializeFields

        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <value>[SerializeField] Reference to BattleGridViewController which handles visual functionality for the %Battle arena's grid.</value>
        [SerializeField] private BattleGridViewController _gridViewController;

        /// <value>[SerializeField] Reference to BattleUiController which holds references to all of the @ref UIHandlerReferences scripts.</value>
        [SerializeField] private BattleUiController _uiController;

        /// <value>[SerializeField] Reference to BattleScreenEffectViewController which handles the screen effects.</value>
        [SerializeField] private BattleScreenEffectViewController _screenEffectViewController;

        /// <value>[SerializeField] Reference to BattleStoneCharacterViewController which handles stone character parts visibility.</value>
        [SerializeField] private BattleStoneCharacterViewController _stoneCharacterViewController;

        /// <value>[SerializeField] Reference to BattleLightrayEffectViewController which handles lightray effects visibility.</value>
        [SerializeField] private BattleLightrayEffectViewController _lightrayEffectViewController;

        /// <value>[SerializeField] Reference to BattleSoundFXViewController which plays sound effects.</value>
        [SerializeField] private BattleSoundFXViewController _soundFXViewController;

        /// <value>[SerializeField] Reference to BattlePlayerInput which polls player input for %Quantum.</value>
        [SerializeField] private BattlePlayerInput _playerInput;

        /// @}

        #endregion SerializeFields

        #region Public

        #region Public - Static Properties

        /// <value>The local player's BattlePlayerSlot.</value>
        public static BattlePlayerSlot LocalPlayerSlot { get; private set; }

        /// <value>The local player's BattleTeamNumber.</value>
        public static BattleTeamNumber LocalPlayerTeam { get; private set; }

        /// <value>Reference to the projectile's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</value>
        public static GameObject ProjectileReference { get; private set; }

        #endregion Public - Static Properties

        #region Public - Static Methods

        /// <summary>
        /// Public static method for assigning #ProjectileReference.
        /// </summary>
        /// <param name="projectileReference">The projectile <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</param>
        public static void AssignProjectileReference(GameObject projectileReference)
        {
            ProjectileReference = projectileReference;
        }

        #endregion Public - Static Methods

        #region Public - Methods

        #region Public - Methods - UiInput

        /// @name UiInput methods
        /// UiInput methods are called when the player gives an %UI input, such as presses a button. These methods shouldn't be called any other way.
        /// @{

        /// <summary>
        /// Public method that gets called when the local player pressed the give up button.<br/>
        /// No functionality yet.
        /// </summary>
        public void UiInputOnLocalPlayerGiveUp()
        {
            Debug.Log("Give up button pressed!");
        }

        /// <summary>
        /// Public method that gets called when the local player selected another character.<br/>
        /// Calls BattlePlayerInput::OnCharacterSelected method in #_playerInput.
        /// </summary>
        /// <param name="characterNumber">The character number which the local player selected.</param>
        public void UiInputOnCharacterSelected(int characterNumber)
        {
            _playerInput.OnCharacterSelected(characterNumber);

            Debug.Log($"Character number {characterNumber} selected!");
        }

        /// <summary>
        /// Public method that gets called when local player gives movement joystick input.
        /// Calls BattlePlayerInput::OnJoystickMovement method in #_playerInput.
        /// </summary>
        /// <param name="input">The movement direction Vector2.</param>
        public void UiInputOnJoystickMovement(Vector2 input)
        {
            _playerInput.OnJoystickMovement(input);
            //Debug.Log($"Move joystick input {input}");
        }

        /// <summary>
        /// Public method that gets called when local player gives rotation joystick input.
        /// Calls BattlePlayerInput::OnJoystickRotation method in #_playerInput.
        /// </summary>
        /// <param name="input">The rotation input as float.</param>
        public void UiInputOnJoystickRotation(float input)
        {
            _playerInput.OnJoystickRotation(input);
            //Debug.Log($"Rotate joystick input {input}");
        }

        /// <summary>
        /// Public method that gets called when the local player pressed the exit game button.<br/>
        /// Calls ExitQuantum method in LobbyManager, which makes the local player leave the game.
        /// </summary>
        public void UiInputOnExitGamePressed()
        {
            if (_endOfGameDataHasEnded) LobbyManager.ExitQuantum(_endOfGameDataWinningTeam == LocalPlayerTeam, (float)_endOfGameDataGameLengthSec);
        }

        /// @}

        #endregion Public - Methods - UiInput

        #endregion Public - Methods

        #endregion Public

        /// @name EndOfGameData variables
        /// Private variables which contain end of game data to pass on to LobbyManager.
        /// @{

        /// <value>Bool to indicate whether the game has ended yet.</value>
        private bool _endOfGameDataHasEnded = false;

        /// <value>The winning team's BattleTeamNumber.</value>
        private BattleTeamNumber _endOfGameDataWinningTeam;

        /// <value>The game's length as seconds.</value>
        private FP _endOfGameDataGameLengthSec;

        /// @}

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method. Handles subscribing to QuantumEvents.
        /// </summary>
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
            QuantumEvent.Subscribe<EventBattleCharacterTakeDamage>(this, QEventOnCharacterTakeDamage);

            // Subscribing to Debug events
            QuantumEvent.Subscribe<EventBattleDebugUpdateStatsOverlay>(this, QEventDebugOnUpdateStatsOverlay);
            QuantumEvent.Subscribe<EventBattleDebugOnScreenMessage>(this, QEventDebugOnScreenMessage);
        }

        #region QuantumEvent handlers

        /// @name QuantumEvent handlers
        /// QuantumEvent handler methods are called by QuantumEvents. These methods shouldn't be called any other way.
        /// @{

        /// <summary>
        /// Private handler method for EventBattleViewWaitForPlayers QuantumEvent.<br/>
        /// Handles initializing BattleUiController::AnnouncementHandler in #_uiController with waiting for players text.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnViewWaitForPlayers(EventBattleViewWaitForPlayers e)
        {
            // Setting view pre-activate waiting for players text
            _uiController.AnnouncementHandler.SetText(BattleUiAnnouncementHandler.TextType.WaitingForPlayers);
        }

        /// <summary>
        /// Private handler method for EventViewInit QuantumEvent.<br/>
        /// Handles initializing the @ref UIHandlerReferences scripts and #_gridViewController.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnViewInit(EventBattleViewInit e)
        {
            PlayerRef playerRef = default;

            // Getting LocalPlayerSlot and LocalPlayerTeam
            if (Utils.TryGetQuantumFrame(out Frame f))
            {
                playerRef = QuantumRunner.Default.Game.GetLocalPlayers()[0];
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

            if (_uiController.JoystickHandler != null)
            {
                if (SettingsCarrier.Instance.BattleMovementInput == BattleMovementInputType.Joystick)
                {
                    BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.MoveJoystick);
                    _uiController.JoystickHandler.SetInfo(BattleUiElementType.MoveJoystick, data);
                    _uiController.JoystickHandler.SetShow(true, BattleUiElementType.MoveJoystick);
                }

                if (SettingsCarrier.Instance.BattleRotationInput == BattleRotationInputType.Joystick)
                {
                    BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.RotateJoystick);
                    _uiController.JoystickHandler.SetInfo(BattleUiElementType.RotateJoystick, data);
                    _uiController.JoystickHandler.SetShow(true, BattleUiElementType.RotateJoystick);
                }

                _uiController.JoystickHandler.SetLocked(true);
            }

            // Commented out code to hide the ui elements which shouldn't be shown at this point, but the code will be used later
            /*
            if (_uiController.GiveUpButtonHandler != null)
            {
                BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.GiveUpButton);
                if (data != null) _uiController.GiveUpButtonHandler.MovableUiElement.SetData(data);
            }
            */

            if (_uiController.PlayerInfoHandler != null)
            {
                RuntimePlayer localPlayerData = f.GetPlayerData(playerRef);
                RuntimePlayer localTeammateData = f.GetPlayerData(BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, LocalPlayerSlot).PlayerRef);

                // Setting local player info
                _uiController.PlayerInfoHandler.SetInfo(
                    PlayerType.LocalPlayer,
                    "Minä",
                    new int[3] { localPlayerData.Characters[0].Id, localPlayerData.Characters[1].Id, localPlayerData.Characters[2].Id },
                    SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.PlayerInfo)
                );

                // Setting local teammate info
                if (localTeammateData != null)
                {
                    _uiController.PlayerInfoHandler.SetInfo(
                        PlayerType.LocalTeammate,
                        "Tiimiläinen",
                        new int[3] { localTeammateData.Characters[0].Id, localTeammateData.Characters[1].Id, localTeammateData.Characters[2].Id },
                        SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.TeammateInfo)
                    );
                }
                else
                {
                    _uiController.PlayerInfoHandler.SetInfo(
                        PlayerType.LocalTeammate,
                        "Tiimiläinen",
                        new int[3] { 101, 201, 301 },
                        SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.TeammateInfo)
                    );
                }
            }

            //} Initializing UI Handlers
        }

        /// <summary>
        /// Private handler method for EventBattleStoneCharacterPieceViewInit QuantumEvent.<br/>
        /// Handles initializing #_stoneCharacterViewController.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnStoneCharacterPieceViewInit(EventBattleStoneCharacterPieceViewInit e)
        {
            if (_stoneCharacterViewController != null)
            {
                _stoneCharacterViewController.SetEmotionIndicator(e.WallNumber, e.Team, e.EmotionIndicatorColorIndex);
            }
        }

        /// <summary>
        /// Private handler method for EventBattleViewActivate QuantumEvent.<br/>
        /// Handles showing %UI elements and initializing BattleCamera scale and rotation.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnViewActivate(EventBattleViewActivate e)
        {
            // Activating view, meaning displaying all visual elements of the game view except for pre-activation elements

            // Show UI elements
            if (_uiController.DiamondsHandler != null) _uiController.DiamondsHandler.SetShow(true);
            /* These UI elements aren't ready and shouldn't be shown yet
            if (_uiController.GiveUpButtonHandler != null) _uiController.GiveUpButtonHandler.SetShow(true);
            */
            if (_uiController.PlayerInfoHandler != null) _uiController.PlayerInfoHandler.SetShow(true);

            // Load settings and set BattleCamera to show game scene with previously loaded settings
            BattleCamera.SetView(
                SettingsCarrier.Instance.BattleArenaScale * 0.01f,
                new(SettingsCarrier.Instance.BattleArenaPosX * 0.01f, SettingsCarrier.Instance.BattleArenaPosY * 0.01f),
                LocalPlayerTeam == BattleTeamNumber.TeamBeta
            );
        }

        /// <summary>
        /// Private handler method for EventBattleViewGetReadyToPlay QuantumEvent.<br/>
        /// Handles showing end of countdown text in BattleUiController::AnnouncementHandler<br/>
        /// and calling BattleUiJoystickHandler::SetLocked false to unlock joysticks to allow player movement and rotation.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnViewGetReadyToPlay(EventBattleViewGetReadyToPlay e)
        {
            // Show end of countdown text in announcement handler
            _uiController.AnnouncementHandler.SetText(BattleUiAnnouncementHandler.TextType.EndOfCountdown);

            // Unlock the joysticks to allow movement
            _uiController.JoystickHandler.SetLocked(false);
        }

        /// <summary>
        /// Private handler method for EventBattleViewGameStart QuantumEvent.<br/>
        /// Handles clearing BattleUiController::AnnouncementHandler text and showing the game timer.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnViewGameStart(EventBattleViewGameStart e)
        {
            // Clear the countdown text
            _uiController.AnnouncementHandler.ClearAnnouncerTextField();

            // Show the timer
            _uiController.TimerHandler.SetShow(true);
        }

        /// <summary>
        /// Private handler method for EventBattleViewGameOver QuantumEvent.<br/>
        /// Handles hiding %UI elements, showing BattleUiController::GameOverHandler and setting EndOfGameData variables.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnViewGameOver(EventBattleViewGameOver e)
        {
            // Hiding UI elements
            _uiController.TimerHandler.SetShow(false);
            _uiController.DiamondsHandler.SetShow(false);
            _uiController.GiveUpButtonHandler.SetShow(false);
            _uiController.PlayerInfoHandler.SetShow(false);
            _uiController.JoystickHandler.SetShow(false);

            // If the game is over, display "Game Over!" and show the Game Over UI
            _uiController.GameOverHandler.SetShow(true);

            // Setting end of game data variables
            _endOfGameDataHasEnded = true;
            _endOfGameDataWinningTeam = e.WinningTeam;
            _endOfGameDataGameLengthSec = e.GameLengthSec;
        }

        /// <summary>
        /// Private handler method for EventBattleChangeEmotionState QuantumEvent.<br/>
        /// Handles calling BattleScreenEffectViewController::ChangeColor in #_screenEffectViewController.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnChangeEmotionState(EventBattleChangeEmotionState e)
        {
            if (!_screenEffectViewController.IsActive) _screenEffectViewController.SetActive(true);
            _screenEffectViewController.ChangeColor((int)e.Emotion);
        }

        /// <summary>
        /// Private handler method for EventBattleLastRowWallDestroyed QuantumEvent.<br/>
        /// Handles calling BattleStoneCharacterViewController::DestroyCharacterPart in #_stoneCharacterViewController<br/>
        /// and BattleLightrayEffectViewController::SpawnLightray in #_lightrayEffectViewController.
        /// </summary>
        /// <param name="e">The event data.</param>
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

        /// <summary>
        /// Private handler method for EventBattlePlaySoundFX QuantumEvent.<br/>
        /// Handles calling BattleSoundFXViewController::PlaySound in #_soundFXViewController.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventPlaySoundFX(EventBattlePlaySoundFX e)
        {
            _soundFXViewController.PlaySound(e.Effect);
        }

        /// <summary>
        /// Private handler method for EventBattleCharacterTakeDamage QuantumEvent.<br/>
        /// Handles calling BattleUiPlayerInfoHandler::UpdateHealthVisual in #_uiController's BattleUiController::PlayerInfoHandler.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnCharacterTakeDamage(EventBattleCharacterTakeDamage e)
        {
            if (e.Team == LocalPlayerTeam)
            {
                _uiController.PlayerInfoHandler.UpdateHealthVisual(e.Slot, e.CharacterNumber, (float)e.HealthPercentage);
            }
        }

        /// <summary>
        /// Private handler method for EventBattleDebugUpdateStatsOverlay QuantumEvent.<br/>
        /// Handles setting stats to BattleUiController::DebugStatsOverlayHandler in #_uiController using BattleUiDebugStatsOverlayHandler::SetStats method.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventDebugOnUpdateStatsOverlay(EventBattleDebugUpdateStatsOverlay e)
        {
            if (!SettingsCarrier.Instance.BattleShowDebugStatsOverlay) return;
            if (e.Slot != LocalPlayerSlot) return;

            _uiController.DebugStatsOverlayHandler.SetShow(true);
            _uiController.DebugStatsOverlayHandler.SetStats(e.Stats);
        }

        /// <summary>
        /// Private handler method for EventBattleDebugOnScreenMessage QuantumEvent.<br/>
        /// Handles calling BattleUiAnnouncementHandler::SetDebugtext in #_uiController's BattleUiController::AnnouncementHandler.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventDebugOnScreenMessage(EventBattleDebugOnScreenMessage e)
        {
            _uiController.AnnouncementHandler.SetDebugtext(e.Message);
        }

        /// @}
        
        #endregion

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method. Handles %UI updates based on the game's state and countdown.
        /// </summary>
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
