/// @file BattleGameViewController.cs
/// <summary>
/// Has a class BattleGameViewController which controls the %Battle %UI elements.
/// </summary>
///
/// This script:<br/>
/// Initializes %Battle %UI elements, and controls their visibility and functionality.<br/>
/// Inherits <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_callbacks.html">QuantumCallbacks@u-exlink</a>.

using UnityEngine;

using Quantum;

using Altzone.Scripts.Lobby;

using Battle.View.UI;
using Battle.View.Effect;
using Battle.View.Audio;
using PlayerType = Battle.View.UI.BattleUiPlayerInfoHandler.PlayerType;

namespace Battle.View.Game
{
    /// <summary>
    /// Initializes %Battle %UI elements, and controls their visibility and functionality.<br/>
    /// Inherits <a href="https://doc-api.photonengine.com/en/quantum/current/class_quantum_1_1_quantum_callbacks.html">QuantumCallbacks@u-exlink</a>.
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

        /// <value>[SerializeField] Reference to BattleSoundFXViewController which plays sound effects.</value>
        [SerializeField] private BattleSoundFXViewController _soundFXViewController;

        /// @}

        #endregion

        #region UiInput methods

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
        /// No functionality yet.
        /// </summary>
        /// <param name="characterNumber">The character number which the local player selected.</param>
        public void UiInputOnCharacterSelected(int characterNumber)
        {
            Debug.Log($"Character number {characterNumber} selected!");
        }

        /// <summary>
        /// Public method that gets called when the local player pressed the exit game button.<br/>
        /// Calls ExitQuantum method in LobbyManager, which makes the local player leave the game.
        /// </summary>
        public void UiInputOnExitGamePressed()
        {
            LobbyManager.ExitQuantum();
        }

        /// @}

        #endregion

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/6000.1/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method. Handles subscribing to QuantumEvents.
        /// </summary>
        private void Awake()
        {
            QuantumEvent.Subscribe<EventViewInit>(this, QEventOnViewInit);
            QuantumEvent.Subscribe<EventBattleChangeEmotionState>(this, QEventOnChangeEmotionState);
            QuantumEvent.Subscribe<EventBattlePlaySoundFX>(this, QEventPlaySoundFX);
            QuantumEvent.Subscribe<EventBattleDebugUpdateStatsOverlay>(this, QEventDebugOnUpdateStatsOverlay);
        }

        #region QuantumEvent handlers

        /// @name QuantumEvent handlers
        /// QuantumEvent handler methods are called by QuantumEvents. These methods shouldn't be called any other way.
        /// @{

        /// <summary>
        /// Private handler method for EventViewInit QuantumEvent.<br/>
        /// Handles initializing the @ref UIHandlerReferences scripts, #_gridViewController, and BattleCamera scale and rotation.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventOnViewInit(EventViewInit e)
        {
            if (_gridViewController != null)
            {
                _gridViewController.SetGrid();
            }

            if (_uiController.DiamondsHandler != null)
            {
                _uiController.DiamondsHandler.SetShow(true);
                _uiController.DiamondsHandler.SetDiamondsText(0);
            }

            // Commented out code to hide the ui elements which shouldn't be shown at this point, but the code will be used later

            /*
            if (_uiController.GiveUpButtonHandler != null)
            {
                _uiController.GiveUpButtonHandler.SetShow(true);
                _uiController.GiveUpButtonHandler.SetShow(true);
            }

            if (_uiController.PlayerInfoHandler != null)
            {
                _uiController.PlayerInfoHandler.SetShow(true);
                _uiController.PlayerInfoHandler.SetInfo(PlayerType.LocalPlayer, "Min�", new int[3] { 101, 201, 301 });
                _uiController.PlayerInfoHandler.SetInfo(PlayerType.LocalTeammate, "Tiimil�inen", new int[3] { 401, 501, 601 });
            }
            */
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
        /// Private handler method for EventBattlePlaySoundFX QuantumEvent.<br/>
        /// Handles calling BattleSoundFXViewController::PlaySound in #_soundFXViewController.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventPlaySoundFX(EventBattlePlaySoundFX e)
        {
            _soundFXViewController.PlaySound(e.Effect);
        }

        /// <summary>
        /// Private handler method for EventBattleDebugUpdateStatsOverlay QuantumEvent.<br/>
        /// Handles setting stats to BattleUiController::DebugStatsOverlayHandler in #_uiController using BattleUiDebugStatsOverlayHandler::SetStats method.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void QEventDebugOnUpdateStatsOverlay(EventBattleDebugUpdateStatsOverlay e)
        {
            _uiController.DebugStatsOverlayHandler.SetShow(true);
            _uiController.DebugStatsOverlayHandler.SetStats(e.Character);
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
                        _uiController.AnnouncementHandler.SetShow(true);
                        _uiController.AnnouncementHandler.SetCountDownNumber(countDown);
                        break;

                    case BattleGameState.Playing:
                        // Clear the countdown text when the countdown is negative
                        _uiController.AnnouncementHandler.ClearAnnouncerTextField();
                        _uiController.AnnouncementHandler.SetShow(true);

                        // Starting game timer
                        _uiController.TimerHandler.SetShow(true);
                        _uiController.TimerHandler.StartTimer(frame);

                        // updating diamonds (at the moment shows only alpha team's diamonds)
                        BattleDiamondCounterQSingleton diamondCounter = frame.GetSingleton<BattleDiamondCounterQSingleton>();
                        _uiController.DiamondsHandler.SetDiamondsText(diamondCounter.AlphaDiamonds);
                        break;

                    case BattleGameState.GetReadyToPlay:
                        // Display "GO!" when the countdown reaches zero
                        _uiController.AnnouncementHandler.ShowEndOfCountDownText();
                        break;

                    case BattleGameState.GameOver:
                        _uiController.TimerHandler.StopTimer();
                        _uiController.TimerHandler.SetShow(false);

                        _uiController.DiamondsHandler.SetShow(false);
                        _uiController.GiveUpButtonHandler.SetShow(false);
                        _uiController.PlayerInfoHandler.SetShow(false);

                        // If the game is over, display "Game Over!" and show the Game Over UI
                        _uiController.GameOverHandler.SetShow(true);
                        break;
                }

            }
        }

    }
}
