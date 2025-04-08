using UnityEngine;
using Button = UnityEngine.UI.Button;

using Quantum;

using Altzone.Scripts.Lobby;

using Battle.View.UI;
using Battle.View.Effect;
using Battle.View.Audio;
using PlayerType = Battle.View.UI.BattleUiPlayerInfoHandler.PlayerType;

namespace Battle.View.Game
{
    public class BattleGameViewController : QuantumCallbacks
    {
        [SerializeField] private BattleGridViewController _gridViewController;
        [SerializeField] private BattleUiController _uiController;
        [SerializeField] private BattleScreenEffectViewController _screenEffectViewController;
        [SerializeField] private BattleSoundFXViewController _soundFXViewController;

        public void OnLocalPlayerGiveUp()
        {
            Debug.Log("Give up button pressed!");
        }

        public void OnCharacterSelected(int characterNumber)
        {
            Debug.Log($"Character number {characterNumber} selected!");
        }

        public void OnExitGamePressed()
        {
            LobbyManager.ExitQuantum();
        }

        private void Awake()
        {
            QuantumEvent.Subscribe<EventViewInit>(this, OnViewInit);
            QuantumEvent.Subscribe<EventBattleChangeEmotionState>(this, OnChangeEmotionState);
            QuantumEvent.Subscribe<EventBattlePlaySoundFX>(this, PlaySoundFX);
            QuantumEvent.Subscribe<EventBattleDebugUpdateStatsOverlay>(this, DebugOnUpdateStatsOverlay);
        }

        private void OnViewInit(EventViewInit e)
        {
            if (_gridViewController != null)
            {
                _gridViewController.SetGrid();
            }


            if (_uiController.DiamondsHandler != null)
            {
                _uiController.DiamondsHandler.SetDiamondsText(0);
            }

            // Commented out code to hide the ui elements which shouldn't be shown at this point, but the code will be used later

            
            if (_uiController.GiveUpButtonHandler != null)
            {
                _uiController.GiveUpButtonHandler.SetShow(true);
            }

            if (_uiController.PlayerInfoHandler != null)
            {
                _uiController.PlayerInfoHandler.SetInfo(PlayerType.LocalPlayer, "Minä", new int[3] { 101, 201, 301 });
                _uiController.PlayerInfoHandler.SetInfo(PlayerType.LocalPlayerTeammate, "Tiimiläinen", new int[3] { 401, 501, 601 });
            }
            
        }

        private void OnChangeEmotionState(EventBattleChangeEmotionState e)
        {
            if (!_screenEffectViewController.IsActive) _screenEffectViewController.SetActive(true);
            _screenEffectViewController.ChangeColor((int)e.Emotion);
        }

        private void PlaySoundFX(EventBattlePlaySoundFX e)
        {
            _soundFXViewController.PlaySound(e.Effect);
        }

        private void DebugOnUpdateStatsOverlay(EventBattleDebugUpdateStatsOverlay e)
        {
            _uiController.DebugStatsOverlayHandler.SetShow(true);
            _uiController.DebugStatsOverlayHandler.SetStats(e.Character);
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
                        // Clear the countdown text when the countdown is negative
                        _uiController.AnnouncementHandler.ClearAnnouncerTextField();
                        _uiController.TimerHandler.StartTimer(frame);
                        break;

                    case BattleGameState.GetReadyToPlay:
                        // Display "GO!" when the countdown reaches zero
                        _uiController.AnnouncementHandler.ShowEndOfCountDownText();
                        break;

                    case BattleGameState.GameOver:
                        // If the game is over, display "Game Over!" and show the Game Over UI
                        _uiController.GameOverHandler.SetShow(true);
                        _uiController.GiveUpButtonHandler.SetShow(false);
                        _uiController.TimerHandler.StopTimer();
                        break;
                }
            }
        }

    }
}
