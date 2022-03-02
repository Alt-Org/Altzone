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
    public class PlayerInstantiate : MonoBehaviour
    {
        private const int PlayerPosition1 = PhotonBattle.PlayerPosition1;
        private const int PlayerPosition2 = PhotonBattle.PlayerPosition2;
        private const int PlayerPosition3 = PhotonBattle.PlayerPosition3;
        private const int PlayerPosition4 = PhotonBattle.PlayerPosition4;

        [Header("Player Settings"), SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Vector2 _playerPosition1;
        [SerializeField] private Vector2 _playerPosition2;
        [SerializeField] private Vector2 _playerPosition3;
        [SerializeField] private Vector2 _playerPosition4;

        [Header("Level Settings"), SerializeField] private GameObject _gameBackground;

        private void OnEnable()
        {
            var player = PhotonNetwork.LocalPlayer;
            if (!PhotonBattle.IsRealPlayer(player))
            {
                Debug.Log($"OnEnable SKIP player {player.GetDebugLabel()}");
                return;
            }
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var isNormalPos = playerPos <= PhotonBattle.PlayerPosition2;
            var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
            if (teamNumber == PhotonBattle.TeamRedValue)
            {
                var gameCameraInstance = Context.GetGameCamera;
                var gameCamera = gameCameraInstance != null ? gameCameraInstance.Camera : null;
                var isRotated = RotateLocalPlayer(gameCamera, _gameBackground);
                if (isRotated)
                {
                    isNormalPos = false;
                }
            }
            var pos = GetPlayerPosition(playerPos, isNormalPos);
            var instantiationPosition = new Vector3(pos.x, pos.y);
            Debug.Log($"OnEnable create player {player.GetDebugLabel()} @ {instantiationPosition} from {_playerPrefab.name}");
            PhotonNetwork.Instantiate(_playerPrefab.name, instantiationPosition, Quaternion.identity);
        }

        private Vector2 GetPlayerPosition(int playerPos, bool isNormalPos)
        {
            Vector2 pos;
            if (isNormalPos)
            {
                switch (playerPos)
                {
                    case PlayerPosition1:
                        pos = _playerPosition1;
                        break;
                    case PlayerPosition2:
                        pos = _playerPosition2;
                        break;
                    case PlayerPosition3:
                        pos = _playerPosition3;
                        break;
                    case PlayerPosition4:
                        pos = _playerPosition4;
                        break;
                    default:
                        throw new UnityException($"invalid playerPos: {playerPos}");
                }
            }
            else
            {
                switch (playerPos)
                {
                    case PlayerPosition1:
                        pos = _playerPosition2;
                        break;
                    case PlayerPosition2:
                        pos = _playerPosition1;
                        break;
                    case PlayerPosition3:
                        pos = _playerPosition4;
                        break;
                    case PlayerPosition4:
                        pos = _playerPosition3;
                        break;
                    default:
                        throw new UnityException($"invalid playerPos: {playerPos}");
                }
            }
            return pos;
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
                gameBackground.transform.Rotate(isUpsideDown);
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