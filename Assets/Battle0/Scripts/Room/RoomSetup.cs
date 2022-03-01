using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle0.Scripts.interfaces;
using Battle0.Scripts.Player;
using Battle0.Scripts.Scene;
using Photon.Pun;
using UnityEngine;

namespace Battle0.Scripts.Room
{
    /// <summary>
    /// Setup arena for Battle gameplay.
    /// </summary>
    /// <remarks>
    /// Wait that all players has been instantiated properly.
    /// </remarks>
    public class RoomSetup : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private GameObject[] objectsToManage;

        [Header("Live Data"), SerializeField] private List<PlayerActor> _playerActors;

        private void Awake()
        {
            Debug.Log($"Awake: {PhotonNetwork.NetworkClientState}");
            PlayerActivator.AllPlayerActors.Clear();
            PlayerActivator.HomeTeamNumber = PhotonBattle.NoTeamValue;
            PlayerActivator.LocalTeamNumber = PhotonBattle.NoTeamValue;
            PrepareCurrentStage();
        }

        private void OnEnable()
        {
            SetupLocalPlayer();
            StartCoroutine(SetupAllPlayers());
        }

        private void PrepareCurrentStage()
        {
            // Disable game objects until this room stage is ready
            Array.ForEach(objectsToManage, x => x.SetActive(false));
        }

        private void ContinueToNextStage()
        {
            enabled = false;
            // Enable game objects when this room stage is ready to play
            Array.ForEach(objectsToManage, x => x.SetActive(true));
        }

        private void SetupLocalPlayer()
        {
            var player = PhotonNetwork.LocalPlayer;
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
            Debug.Log($"SetupLocalPlayer pos={playerPos} team={teamNumber} {player.GetDebugLabel()}");
            if (teamNumber == PhotonBattle.TeamBlueValue)
            {
                return;
            }
            var sceneConfig = SceneConfig.Get();
            var features = RuntimeGameConfig.Get().Features;
            if (features._isRotateGameCamera)
            {
                // Rotate game camera
                sceneConfig.rotateGameCamera(upsideDown: true);
            }
            if (features._isRotateGamePlayArea)
            {
                // Rotate background
                sceneConfig.rotateBackground(upsideDown: true);
                // Separate sprites for each team gameplay area - these might not be visible in final game
                if (sceneConfig.upperTeamSprite.enabled && sceneConfig.lowerTeamSprite.enabled)
                {
                    // c# swap via deconstruction
                    (sceneConfig.upperTeamSprite.color, sceneConfig.lowerTeamSprite.color) =
                        (sceneConfig.lowerTeamSprite.color, sceneConfig.upperTeamSprite.color);
                }
            }
        }

        private IEnumerator SetupAllPlayers()
        {
            var playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            _playerActors = FindObjectsOfType<PlayerActor>().ToList();
            var wait = new WaitForSeconds(0.2f);
            // Wait for players so that everybody can know (find) each other if required!
            var logLine = string.Empty;
            while (_playerActors.Count < playerCount && PhotonNetwork.InRoom)
            {
                var line = $"setupAllPlayers playerCount={playerCount} playerActors={_playerActors.Count} wait {Time.unscaledTime:F0}";
                if (line != logLine)
                {
                    logLine = line;
                    Debug.Log(logLine);
                }
                yield return wait;
                _playerActors = FindObjectsOfType<PlayerActor>().ToList();
                playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            }
            // All player have been instantiated in the scene, wait until they are in known state
            for (; PhotonNetwork.InRoom;)
            {
                if (CheckPlayerActors(_playerActors) == playerCount)
                {
                    break;
                }
                yield return null;
            }
            if (!PhotonNetwork.InRoom)
            {
                yield break;
            }
            // TODO: this need more work to make it better and easier to understand
            // Save current player actor list for easy access later!
            PlayerActivator.AllPlayerActors.AddRange(_playerActors);
            Debug.Log($"setupAllPlayers playerCount={playerCount} allPlayerActors={PlayerActivator.AllPlayerActors.Count} ready");
            // Now we can activate all players safely with two passes over them!
            foreach (var playerActor in _playerActors)
            {
                if (!PhotonNetwork.InRoom)
                {
                    yield break;
                }
                playerActor.LateAwakePass1();
                ((IPlayerActor)playerActor).setGhostedMode();
            }
            foreach (var playerActor in _playerActors)
            {
                if (!PhotonNetwork.InRoom)
                {
                    yield break;
                }
                playerActor.LateAwakePass2();
            }
            ContinueToNextStage();
        }

        private static int CheckPlayerActors(List<PlayerActor> playerActors)
        {
            var activeCount = 0;
            foreach (var playerActor in playerActors)
            {
                var activator = playerActor.GetComponent<PlayerActivator>();
                if (activator._isAwake)
                {
                    activeCount += 1;
                }
            }
            Debug.Log($"checkPlayerActors activeCount={activeCount}");
            return activeCount;
        }
    }
}