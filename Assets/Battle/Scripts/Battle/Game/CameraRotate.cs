using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    internal class CameraRotate : MonoBehaviour
    {
        // UiGridRenderer laittaa tämän koodin päälle kun grid on luotu, jotta sitä voi kääntää tämän koodin startissa

        // Serialized Fields
        [SerializeField] private Transform _camera;
        [SerializeField] private Transform _background;
        [SerializeField] private Transform _gridOverlay;
        [SerializeField] private Transform _diamondCounters;
        [SerializeField] private RectTransform _betaDiamonds;
        [SerializeField] private RectTransform _alphaDiamonds;
        [SerializeField] private Transform _diamondCounters2;
        [SerializeField] private RectTransform _betaDiamonds2;
        [SerializeField] private RectTransform _alphaDiamonds2;

        // Private fields
        private int _teamNumber;

        //Private methods
        private void Start()
        {
            if (!PhotonNetwork.InRoom) return;

            Player localPlayer = PhotonNetwork.LocalPlayer;
            int localPlayerPos = PhotonBattle.GetPlayerPos(localPlayer);
            _teamNumber = (int)PhotonBattle.GetTeamNumber(localPlayerPos);

            Debug.Log($"TeamNumber {_teamNumber} pos {localPlayerPos} {localPlayer.GetDebugLabel()}");

            if (_teamNumber == 2)
            {
                RotateCamera();
            }
        }

        private void RotateCamera()
        {
            _camera.eulerAngles = new Vector3(0, 0, 180);
            _background.eulerAngles = new Vector3(0, 0, 180);
            _gridOverlay.eulerAngles = new Vector3(0, 0, 180);
            _alphaDiamonds.anchoredPosition = new Vector2(_betaDiamonds.anchoredPosition.x, _betaDiamonds.anchoredPosition.y);
            _alphaDiamonds2.anchoredPosition = new Vector2(_betaDiamonds2.anchoredPosition.x, _betaDiamonds2.anchoredPosition.y);
            _betaDiamonds.anchoredPosition = new Vector2(-_betaDiamonds2.anchoredPosition.x, -_betaDiamonds2.anchoredPosition.y);
            _betaDiamonds2.anchoredPosition = new Vector2(-_alphaDiamonds.anchoredPosition.x, -_alphaDiamonds.anchoredPosition.y);
        }
    }
}
