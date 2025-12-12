/// @file BattleGameViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Game,BattleGameViewController} class which controls the %Battle %UI elements.
/// </summary>
///
/// This script:<br/>
/// Initializes %Battle %UI elements, and controls their visibility and functionality.

//#define DEBUG_OVERLAY_ENABLED_OVERRIDE

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Altzone usings
using Altzone.Scripts.BattleUiShared;
using Altzone.Scripts.Lobby;

// Battle QSimulation usings
using Battle.QSimulation;
using Battle.QSimulation.Game;
using Battle.QSimulation.Player;

// Battle View usings
using Battle.View.Audio;
using Battle.View.Effect;
using Battle.View.UI;
using Battle.View.Player;

using BattleMovementInputType = SettingsCarrier.BattleMovementInputType;
using BattleRotationInputType = SettingsCarrier.BattleRotationInputType;
using BattleUiElementType = SettingsCarrier.BattleUiElementType;
using PlayerType = Battle.View.UI.BattleUiPlayerInfoHandler.PlayerType;

namespace Battle.View.Game
{
    /// <summary>
    /// <span class="brief-h">%Game view <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_callbacks.html">QuantumCallbacks class@u-exlink</a>.</span><br/>
    /// Initializes %Battle %UI elements, and controls their visibility and functionality.
    /// </summary>
    ///
    /// Handles any functionality needed by the @uihandlerslink which need to access other parts of %Battle, for example triggering the selection of another character.
    /// Accesses all of the @ref UIHandlerReferences through the BattleUiController reference variable <see cref="BattleGameViewController._uiController">_uiController</see>.<br/>
    public class BattleGameViewController : QuantumCallbacks
    {
        #region SerializeFields

        /// @anchor BattleGameViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to BattleGridViewController which handles visual functionality for the %Battle arena's grid.</summary>
        /// @ref BattleGameViewController-SerializeFields
        [SerializeField] private BattleGridViewController _gridViewController;

        /// <summary>[SerializeField] Reference to BattleUiController which holds references to all of the @ref UIHandlerReferences scripts.</summary>
        /// @ref BattleGameViewController-SerializeFields
        [SerializeField] private BattleUiController _uiController;

        /// <summary>[SerializeField] Reference to BattleScreenEffectViewController which handles the screen effects.</summary>
        /// @ref BattleGameViewController-SerializeFields
        [SerializeField] private BattleScreenEffectViewController _screenEffectViewController;

        /// <summary>[SerializeField] Reference to BattleStoneCharacterViewController which handles stone character parts visibility.</summary>
        /// @ref BattleGameViewController-SerializeFields
        [SerializeField] private BattleStoneCharacterViewController _stoneCharacterViewController;

        /// <summary>[SerializeField] Reference to BattleLightrayEffectViewController which handles lightray effects visibility.</summary>
        /// @ref BattleGameViewController-SerializeFields
        [SerializeField] private BattleLightrayEffectViewController _lightrayEffectViewController;

        /// <summary>[SerializeField] Reference to BattlePlayerInput which polls player input for %Quantum.</summary>
        /// @ref BattleGameViewController-SerializeFields
        [SerializeField] private BattlePlayerInput _playerInput;

        /// @}

        #endregion SerializeFields

        #region Public

        #region Public - Static Properties

        /// <summary>The local player's BattlePlayerSlot.</summary>
        public static BattlePlayerSlot LocalPlayerSlot { get; private set; }

        /// <summary>The local player's BattleTeamNumber.</summary>
        public static BattleTeamNumber LocalPlayerTeam { get; private set; }

        /// <summary>Reference to the projectile's <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/GameObject.html">GameObject@u-exlink</a>.</summary>
        public static GameObject ProjectileReference { get; private set; }

        /// <summary>Reference to the UiController.</summary>
        public static BattleUiController UiController => s_instance._uiController;

        #endregion Public - Static Properties

        #region Public - Static Methods

        /// <summary>
        /// Public static method for assigning <see cref="BattleGameViewController.ProjectileReference">ProjectileReference</see>.
        /// </summary>
        ///
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
        /// Public method that gets called when the local player pressed the give up button.
        /// </summary>
        public void UiInputOnLocalPlayerGiveUp()
        {
            _debugLogger.Log("Give up button pressed");
            _playerInput.OnGiveUp();
        }

        /// <summary>
        /// Public method that gets called when the local player selected another character.<br/>
        /// Calls <see cref="BattlePlayerInput.OnCharacterSelected"/> method
        /// in <see cref="BattleGameViewController._playerInput">_playerInput</see>.
        /// </summary>
        ///
        /// <param name="characterNumber">The character number which the local player selected.</param>
        public void UiInputOnCharacterSelected(int characterNumber)
        {
            _playerInput.OnCharacterSelected(characterNumber);

            _debugLogger.LogFormat("Character number {0} button pressed!", characterNumber);
        }

        /// <summary>
        /// Public method that gets called when local player gives movement joystick input.
        /// Calls <see cref="Battle.View.Player.BattlePlayerInput.OnJoystickMovement">OnJoystickMovement</see> method
        /// in <see cref="BattleGameViewController._playerInput">_playerInput</see>.
        /// </summary>
        ///
        /// <param name="input">The movement direction Vector2.</param>
        public void UiInputOnJoystickMovement(Vector2 input)
        {
            _playerInput.OnJoystickMovement(input);
            //Debug.Log($"Move joystick input {input}");
        }

        /// <summary>
        /// Public method that gets called when local player gives rotation joystick input.
        /// Calls <see cref="Battle.View.Player.BattlePlayerInput.OnJoystickRotation">OnJoystickRotation</see> method
        /// in <see cref="BattleGameViewController._playerInput">_playerInput</see>.
        /// </summary>
        ///
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

        /// <summary>Private static reference to an instance of BattleGameViewController.</summary>
        private static BattleGameViewController s_instance;

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private BattleDebugLogger _debugLogger;

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
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method. Handles subscribing to QuantumEvents.
        /// </summary>
        private void Awake()
        {
            s_instance = this;

            BattleDebugOverlay.Init();

            _debugLogger = BattleDebugLogger.Create<BattleGameViewController>();

            // Showing announcement handler and setting view pre-activate loading text
            _uiController.AnnouncementHandler.SetShow(true);
            _uiController.AnnouncementHandler.SetText(BattleUiAnnouncementHandler.TextType.Loading);

            // Subscribing to Game Flow events
            QuantumEvent.Subscribe<EventBattleViewWaitForPlayers>(this, QEventOnViewWaitForPlayers);
            QuantumEvent.Subscribe<EventBattleViewPlayerConnected>(this, QEventOnViewPlayerConnected);
            QuantumEvent.Subscribe<EventBattleViewAllPlayersConnected>(this, QEventOnViewAllPlayersConnected);
            QuantumEvent.Subscribe<EventBattleViewInit>(this, QEventOnViewInit);
            QuantumEvent.Subscribe<EventBattleViewActivate>(this, QEventOnViewActivate);
            QuantumEvent.Subscribe<EventBattleViewGetReadyToPlay>(this, QEventOnViewGetReadyToPlay);
            QuantumEvent.Subscribe<EventBattleViewGameStart>(this, QEventOnViewGameStart);
            QuantumEvent.Subscribe<EventBattleViewGameOver>(this, QEventOnViewGameOver);

            // Subscribing to other View Init events
            QuantumEvent.Subscribe<EventBattleStoneCharacterPieceViewInit>(this, QEventOnStoneCharacterPieceViewInit);

            // subscribing to UI control events
            QuantumEvent.Subscribe<EventBattleViewSetRotationJoystickVisibility>(this, QEventOnSetRotationJoystickVisibility);

            // Subscribing to Gameplay events
            QuantumEvent.Subscribe<EventBattleChangeEmotionState>(this, QEventOnChangeEmotionState);
            QuantumEvent.Subscribe<EventBattleLastRowWallDestroyed>(this, QEventOnLastRowWallDestroyed);
            QuantumEvent.Subscribe<EventBattlePlaySoundFX>(this, QEventPlaySoundFX);
            QuantumEvent.Subscribe<EventBattleCharacterSelected>(this, QEventCharacterSelected);
            QuantumEvent.Subscribe<EventBattleCharacterTakeDamage>(this, QEventOnCharacterTakeDamage);
            QuantumEvent.Subscribe<EventBattleShieldTakeDamage>(this, QEventOnShieldTakeDamage);
            QuantumEvent.Subscribe<EventBattleGiveUpStateChange>(this, QEventOnGiveUpStateChange);

            // Subscribing to Debug events
            QuantumEvent.Subscribe<EventBattleDebugOnScreenMessage>(this, QEventDebugOnScreenMessage);
        }

        #region QuantumEvent handlers

        /// @name QuantumEvent handlers
        /// QuantumEvent handler methods are called by QuantumEvents. These methods shouldn't be called any other way.
        /// @{

        /// <summary>
        /// Private handler method for EventBattleViewWaitForPlayers QuantumEvent.<br/>
        /// Handles initializing <see cref="Battle.View.UI.BattleUiController.AnnouncementHandler">AnnouncementHandler</see>
        /// in <see cref="BattleGameViewController._uiController">_uiController</see> with waiting for players text.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnViewWaitForPlayers(EventBattleViewWaitForPlayers e)
        {
            // Setting view pre-activate waiting for players text
            Utils.TryGetQuantumFrame(out Frame f);
            BattleParameters.PlayerType[] playerSlotTypes = BattleParameters.GetPlayerSlotTypes(f);
            _uiController.LoadScreenHandler.Show(playerSlotTypes, e.Data.PlayerNames);

            _uiController.AnnouncementHandler.SetText(BattleUiAnnouncementHandler.TextType.WaitingForPlayers);
        }

        /// <summary>
        /// Private handler method for EventBattleViewPlayerConnected QuantumEvent.<br/>
        /// Handles calling <see cref="Battle.View.UI.BattleUiLoadScreenHandler.PlayerConnected">PlayerConnected</see>
        /// through <see cref="BattleGameViewController._uiController">_uiController</see> with the slot and character IDs of the connected player.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnViewPlayerConnected(EventBattleViewPlayerConnected e)
        {
            BattlePlayerSlot playerSlot = e.Data.PlayerSlot;
            int[] characterIds = new int[3];
            int[] characterClasses = new int[3];
            for (int i = 0; i < 3; i++)
            {
                characterIds[i] = e.Data.Characters[i].Id;
                characterClasses[i] = e.Data.Characters[i].Class;
            }

            _uiController.LoadScreenHandler.PlayerConnected(playerSlot, characterIds, characterClasses);
        }

        /// <summary>
        /// Private handler method for EventBattleViewAllPlayersConnected QuantumEvent.<br/>
        /// Handles calling <see cref="Battle.View.UI.BattleUiAnnouncementHandler.ClearAnnouncerTextField">ClearAnnouncerTextField</see> once all players have successfully joined the game.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnViewAllPlayersConnected(EventBattleViewAllPlayersConnected e)
        {
            _uiController.AnnouncementHandler.ClearAnnouncerTextField();
        }

        /// <summary>
        /// Private handler method for EventViewInit QuantumEvent.<br/>
        /// Handles initializing the <b>UIHandlerReferences</b> scripts and <see cref="BattleGameViewController._gridViewController">_gridViewController</see>.
        /// </summary>
        ///
        /// See @ref UIHandlerReferences for more info.
        ///
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

            BattleDebugOverlayLink.SetLocalPlayerSlot(LocalPlayerSlot);

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
                }

                if (SettingsCarrier.Instance.BattleRotationInput == BattleRotationInputType.Joystick)
                {
                    BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.RotateJoystick);
                    _uiController.JoystickHandler.SetInfo(BattleUiElementType.RotateJoystick, data);
                }

                _uiController.JoystickHandler.SetLocked(true);
            }

            if (_uiController.GiveUpButtonHandler != null)
            {
                BattleUiMovableElementData data = SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.GiveUpButton);
                if (data != null) _uiController.GiveUpButtonHandler.MovableUiElement.SetData(data);
            }

            if (_uiController.PlayerInfoHandler != null)
            {
                RuntimePlayer localPlayerData = f.GetPlayerData(playerRef);
                RuntimePlayer localTeammateData = f.GetPlayerData(BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, LocalPlayerSlot).PlayerRef);

                // Setting local player info
                _uiController.PlayerInfoHandler.SetInfo(
                    PlayerType.LocalPlayer,
                    "Minä",
                    new int[3] { localPlayerData.Characters[0].Id, localPlayerData.Characters[1].Id, localPlayerData.Characters[2].Id },
                    new int[3] { localPlayerData.Characters[0].Class, localPlayerData.Characters[1].Class, localPlayerData.Characters[2].Class },
                    new float[3] { (float)localPlayerData.Characters[0].Stats.Defence, (float)localPlayerData.Characters[1].Stats.Defence, (float)localPlayerData.Characters[2].Stats.Defence },
                    SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.PlayerInfo)
                );
                _uiController.PlayerInfoHandler.SetShowPlayer(true);

                // Setting local teammate info
                if (localTeammateData != null)
                {
                    _uiController.PlayerInfoHandler.SetInfo(
                        PlayerType.LocalTeammate,
                        "Tiimiläinen",
                        new int[3] { localTeammateData.Characters[0].Id, localTeammateData.Characters[1].Id, localTeammateData.Characters[2].Id },
                        new int[3] { localTeammateData.Characters[0].Class, localTeammateData.Characters[1].Class, localTeammateData.Characters[2].Class },
                        new float[3] { (float)localTeammateData.Characters[0].Stats.Defence, (float)localTeammateData.Characters[1].Stats.Defence, (float)localTeammateData.Characters[2].Stats.Defence },
                        SettingsCarrier.Instance.GetBattleUiMovableElementData(BattleUiElementType.TeammateInfo)
                    );
                    _uiController.PlayerInfoHandler.SetShowTeammate(true);
                }
            }
        }

        /// <summary>
        /// Private handler method for EventBattleStoneCharacterPieceViewInit QuantumEvent.<br/>
        /// Handles initializing <see cref="BattleGameViewController._stoneCharacterViewController"/>.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnStoneCharacterPieceViewInit(EventBattleStoneCharacterPieceViewInit e)
        {
            if (_stoneCharacterViewController != null)
            {
                _stoneCharacterViewController.SetEmotionIndicator(e.WallNumber, e.Team, e.EmotionIndicatorColorIndex);
            }
        }

        /// <summary>
        /// Private handler method for EventBattleViewSetRotationJoystickVisibility QuantumEvent.<br/>
        /// Sets the rotation control joystick to be shown or hidden, if that control method is selected.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnSetRotationJoystickVisibility(EventBattleViewSetRotationJoystickVisibility e)
        {
            if (e.Slot != LocalPlayerSlot) return;

            if (SettingsCarrier.Instance.BattleRotationInput == BattleRotationInputType.Joystick)
            {
                _uiController.JoystickHandler.SetShow(e.IsVisible, BattleUiElementType.RotateJoystick);
            }
        }

        /// <summary>
        /// Private handler method for EventBattleViewActivate QuantumEvent.<br/>
        /// Handles showing %UI elements and initializing BattleCamera scale and rotation.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnViewActivate(EventBattleViewActivate e)
        {
            // Activating view, meaning displaying all visual elements of the game view except for pre-activation elements

            _uiController.LoadScreenHandler.Hide();

            // Show UI elements
            if (_uiController.DiamondsHandler != null) _uiController.DiamondsHandler.SetShow(true);
            if (_uiController.GiveUpButtonHandler != null) _uiController.GiveUpButtonHandler.SetShow(true);
            if (SettingsCarrier.Instance.BattleMovementInput == BattleMovementInputType.Joystick) _uiController.JoystickHandler.SetShow(true, BattleUiElementType.MoveJoystick);
            if (SettingsCarrier.Instance.BattleRotationInput == BattleRotationInputType.Joystick) _uiController.JoystickHandler.SetShow(true, BattleUiElementType.RotateJoystick);
            if (SettingsCarrier.Instance.BattleShowDebugStatsOverlay) _uiController.DebugOverlayHandler.SetShow(true);
            /* These UI elements aren't ready and shouldn't be shown yet
            if (_uiController.GiveUpButtonHandler != null) _uiController.GiveUpButtonHandler.SetShow(true);
            */
            if (_uiController.PlayerInfoHandler != null) _uiController.PlayerInfoHandler.SetShow(true);

#if DEBUG_OVERLAY_ENABLED_OVERRIDE
            _uiController.DebugOverlayHandler.SetShow(true);
#endif

            // Load settings and set BattleCamera to show game scene with previously loaded settings
            BattleCamera.SetView(
                SettingsCarrier.Instance.BattleArenaScale * 0.01f,
                new(SettingsCarrier.Instance.BattleArenaPosX * 0.01f, SettingsCarrier.Instance.BattleArenaPosY * 0.01f),
                LocalPlayerTeam == BattleTeamNumber.TeamBeta
            );

            BattleAudioController.PlayMusic();
        }

        /// <summary>
        /// Private handler method for EventBattleViewGetReadyToPlay QuantumEvent.<br/>
        /// Handles showing end of countdown text in BattleUiController::AnnouncementHandler<br/>
        /// and calling <see cref="Battle.View.UI.BattleUiJoystickHandler.SetLocked">SetLocked</see> false to unlock joysticks to allow player movement and rotation.
        /// </summary>
        ///
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
        /// Handles clearing <see cref="Battle.View.UI.BattleUiController.AnnouncementHandler">AnnouncementHandler</see> text and showing the game timer.
        /// </summary>
        ///
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
        /// Handles hiding %UI elements, showing <see cref="Battle.View.UI.BattleUiController.GameOverHandler">GameOverHandler</see> and setting EndOfGameData variables.
        /// </summary>
        ///
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
        /// Handles calling <see cref="Battle.View.Effect.BattleScreenEffectViewController.ChangeColor">ChangeColor</see>
        /// in <see cref="BattleGameViewController._screenEffectViewController">_screenEffectViewController</see>.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnChangeEmotionState(EventBattleChangeEmotionState e)
        {
            if (!_screenEffectViewController.IsActive) _screenEffectViewController.SetActive(true);
            _screenEffectViewController.ChangeColor((int)e.Emotion);
        }

        /// <summary>
        /// Private handler method for EventBattleLastRowWallDestroyed QuantumEvent.<br/>
        /// Handles calling <see cref="BattleStoneCharacterViewController.DestroyCharacterPart">DestroyCharacterPart</see>
        /// in <see cref="BattleGameViewController._stoneCharacterViewController">_stoneCharacterViewController</see><br/>
        /// and <see cref="Battle.View.Effect.BattleLightrayEffectViewController.SpawnLightray">SpawnLightray</see>
        /// in <see cref="BattleGameViewController._lightrayEffectViewController">_lightrayEffectViewController</see>.
        /// </summary>
        ///
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
        /// Handles calling <see cref="Battle.View.Audio.BattleAudioController.PlaySoundFX">PlaySound</see>
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventPlaySoundFX(EventBattlePlaySoundFX e)
        {
            BattleAudioController.PlaySoundFX(e.Effect);
        }

        private void QEventCharacterSelected(EventBattleCharacterSelected e)
        {
            _uiController.PlayerInfoHandler.SetSelected(e.Slot, e.CharacterNumber);
        }

        /// <summary>
        /// Private handler method for EventBattleCharacterTakeDamage QuantumEvent.<br/>
        /// Handles calling <see cref="Battle.View.UI.BattleUiPlayerInfoHandler.UpdateHealthVisual">UpdateHealthVisual</see>
        /// in <see cref="BattleGameViewController._uiController">_uiController's</see>
        /// <see cref="Battle.View.UI.BattleUiController.PlayerInfoHandler">PlayerInfoHandler</see>.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnCharacterTakeDamage(EventBattleCharacterTakeDamage e)
        {
            if (e.Team == LocalPlayerTeam)
            {
                _uiController.PlayerInfoHandler.UpdateHealthVisual(e.Slot, e.CharacterNumber, (float)e.HealthPercentage);
            }
        }

        /// <summary>
        /// Private handler method for EventBattleShieldTakeDamage QuantumEvent.<br/>
        /// Handles calling <see cref="Battle.View.UI.BattleUiPlayerInfoHandler.UpdateDefenceVisual">UpdateDefenceVisual</see> in
        /// <see cref="BattleGameViewController._uiController">_uiController's</see>
        /// <see cref="Battle.View.UI.BattleUiController.PlayerInfoHandler">PlayerInfoHandler</see>.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnShieldTakeDamage(EventBattleShieldTakeDamage e)
        {
            if (e.Team == LocalPlayerTeam)
            {
                _uiController.PlayerInfoHandler.UpdateDefenceVisual(e.Slot, e.CharacterNumber, (float)e.DefenceValue);
            }
        }

        /// <summary>
        /// Private handler method for EventBattleGiveUpStateChange QuantumEvent.<br/>
        /// Handles calling <see cref="Battle.View.UI.BattleUiGiveUpButtonHandler.UpdateState">UpdateState</see>
        /// in <see cref="BattleGameViewController._uiController">_uiController's</see>
        /// <see cref="Battle.View.UI.BattleUiController.GiveUpButtonHandler">GiveUpButtonHandler</see>.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventOnGiveUpStateChange(EventBattleGiveUpStateChange e)
        {
            if (e.Team == LocalPlayerTeam)
            {
                _uiController.GiveUpButtonHandler.UpdateState(e.Slot, e.StateUpdate);
            }
        }

        /// <summary>
        /// Private handler method for EventBattleDebugOnScreenMessage QuantumEvent.<br/>
        /// Handles calling <see cref="Battle.View.UI.BattleUiAnnouncementHandler.SetDebugtext">SetDebugtext</see>
        /// in <see cref="BattleGameViewController._uiController">_uiController's</see> BattleUiController::AnnouncementHandler.
        /// </summary>
        ///
        /// <param name="e">The event data.</param>
        private void QEventDebugOnScreenMessage(EventBattleDebugOnScreenMessage e)
        {
            _uiController.AnnouncementHandler.SetDebugtext(e.Message);
        }

        /// @}

#endregion

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method. Handles %UI updates based on the game's state and countdown.
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
                    _debugLogger.Error(frame, "GameSession singleton not found");
                    return;
                }

                // Retrieve the GameSession singleton from the Quantum frame
                BattleGameSessionQSingleton gameSession = frame.GetSingleton<BattleGameSessionQSingleton>();

                // Convert the countdown time to an integer for display
                int countDown = (int)gameSession.TimeUntilStartSec;

                // Handle different game states to update the UI
                switch (gameSession.State)
                {
                    case BattleGameState.Countdown:
                        // If the game is in the countdown state, display the countdown timer
                        _uiController.AnnouncementHandler.SetCountDownNumber(countDown);
                        break;

                    case BattleGameState.Playing:
                        // Updating diamonds
                        BattleDiamondCounterQSingleton diamondCounter = frame.GetSingleton<BattleDiamondCounterQSingleton>();
                        _uiController.DiamondsHandler.SetDiamondsText(LocalPlayerTeam == BattleTeamNumber.TeamAlpha ? diamondCounter.AlphaDiamonds : diamondCounter.BetaDiamonds);

                        // Updating timer text
                        _uiController.TimerHandler.FormatAndSetTimerText(gameSession.GameTimeSec);
                        break;
                }
            }
        }
    }
}
