using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Factory;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.Room
{
    /// <summary>
    /// Instantiate local Photon player in correct position.
    /// </summary>
    /// <remarks>
    /// Note that this is not actual player prefab but Photon component for it.
    /// </remarks>
    public class PlayerInstantiate : MonoBehaviour
    {
        [Header("Player Settings"), SerializeField] private GameObject _playerPrefab;

        private void OnEnable()
        {
            var player = PhotonNetwork.LocalPlayer;
            if (!PhotonBattle.IsRealPlayer(player))
            {
                Debug.Log($"OnEnable SKIP player {player.GetDebugLabel()}");
                return;
            }
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var gameBackground = Context.GetGameBackground;
            gameBackground.SetBackgroundImageByIndex(0);
            var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
            var isNormalPlayerPlacement = playerPos <= PhotonBattle.PlayerPosition2;
            if (teamNumber == PhotonBattle.TeamRedValue)
            {
                var gameCamera = Context.GetGameCamera;
                var isRotated = RotateLocalPlayerGameplayExperience(gameCamera.Camera, gameBackground.Background);
                if (isRotated)
                {
                    isNormalPlayerPlacement = false;
                }
            }
            var pos = GetPlayerPosition(playerPos, isNormalPlayerPlacement);
            var instantiationPosition = new Vector3(pos.x, pos.y);
            Debug.Log($"OnEnable create player {player.GetDebugLabel()} @ {instantiationPosition} from {_playerPrefab.name}");
            PhotonNetwork.Instantiate(_playerPrefab.name, instantiationPosition, Quaternion.identity);
        }

        private static Vector2 GetPlayerPosition(int playerPos, bool isNormalPlayerPlacement)
        {
            if (!isNormalPlayerPlacement)
            {
                playerPos = PhotonBattle.GetTeamMemberPlayerPos(playerPos);
            }
            var startPosition = Context.GetPlayerPlayArea.GetPlayerStartPosition(playerPos);
            return startPosition;
        }

        private static bool RotateLocalPlayerGameplayExperience(Camera gameCamera, GameObject gameBackground)
        {
            var features = RuntimeGameConfig.Get().Features;
            var isRotateGameCamera = features._isRotateGameCamera && gameCamera != null;
            if (isRotateGameCamera)
            {
                // Rotate game camera.
                Debug.Log($"RotateGameCamera upsideDown");
                gameCamera.GetComponent<Transform>().Rotate(true);
            }
            var isRotateGameBackground  = features._isRotateGameBackground && gameBackground != null;
            if (isRotateGameBackground)
            {
                // Rotate background.
                Debug.Log($"RotateGameBackground upsideDown");
                gameBackground.GetComponent<Transform>().Rotate(true);
                // Separate sprites for each team gameplay area - these might not be visible in final game
                // - see Battle.Scripts.Room.RoomSetup.SetupLocalPlayer() how this is done in Altzone project.
            }
            return isRotateGameCamera;
        }
    }
}