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
            var isNormalTeamLayout = playerPos <= PhotonBattle.PlayerPosition2;
            var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
            if (teamNumber == PhotonBattle.TeamRedValue)
            {
                var gameCamera = Context.GetGameCamera;
                var gameBackground = Context.GetGameBackground;
                var isRotated = RotateLocalPlayer(gameCamera.Camera, gameBackground.Background);
                if (isRotated)
                {
                    isNormalTeamLayout = false;
                }
            }
            var pos = GetPlayerPosition(playerPos, isNormalTeamLayout);
            var instantiationPosition = new Vector3(pos.x, pos.y);
            Debug.Log($"OnEnable create player {player.GetDebugLabel()} @ {instantiationPosition} from {_playerPrefab.name}");
            PhotonNetwork.Instantiate(_playerPrefab.name, instantiationPosition, Quaternion.identity);
        }

        private static Vector2 GetPlayerPosition(int playerPos, bool isNormalTeamLayout)
        {
            if (!isNormalTeamLayout)
            {
                playerPos = PhotonBattle.GetTeamMemberPlayerPos(playerPos);
            }
            var startPosition = Context.GetPlayerPlayArea.GetPlayerStartPosition(playerPos);
            return startPosition;
        }

        private static bool RotateLocalPlayer(Camera gameCamera, GameObject gameBackground)
        {
            void RotateGameCamera(bool isUpsideDown)
            {
                // Rotate game camera for team red.
                Debug.Log($"RotateGameCamera upsideDown {isUpsideDown}");
                gameCamera.GetComponent<Transform>().Rotate(isUpsideDown);
            }

            void RotateBackground(bool isUpsideDown)
            {
                Debug.Log($"RotateBackground upsideDown {isUpsideDown}");
                gameBackground.GetComponent<Transform>().Rotate(isUpsideDown);
            }

            var features = RuntimeGameConfig.Get().Features;
            var isRotate = features._isRotateGameCamera && gameCamera != null;
            if (isRotate)
            {
                // Rotate game camera.
                RotateGameCamera(true);
            }
            if (features._isRotateGamePlayArea && gameBackground != null)
            {
                // Rotate background.
                RotateBackground(true);
                // Separate sprites for each team gameplay area - these might not be visible in final game
                // - see Battle.Scripts.Room.RoomSetup.SetupLocalPlayer() how this is done in Altzone project.
            }
            return isRotate;
        }
    }
}