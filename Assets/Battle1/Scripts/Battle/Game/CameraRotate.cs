using Photon.Realtime;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
using Player = Battle1.PhotonRealtime.Code.Player;*/

namespace Battle1.Scripts.Battle.Game
{
    internal class CameraRotate : MonoBehaviour
    {
        // UiGridRenderer laittaa tämän koodin päälle kun grid on luotu, jotta sitä voi kääntää tämän koodin startissa

        // Serialized Fields
        [SerializeField] private Transform _camera;
        [SerializeField] private Transform _background;
        [SerializeField] private Transform _gridOverlay;
        [SerializeField] private RectTransform _alphaDiamonds;
        [SerializeField] private RectTransform _betaDiamonds;

        // Private fields
        private int _teamNumber;


        private struct AnchorPreset {
            public Vector2 min;
            public Vector2 max;
            public Vector2 anchoredPos;
        }

        private AnchorPreset _anchorPresetAlpha;
        private AnchorPreset _anchorPresetBeta;

        //Private methods
        private void Start()
        {
            /*if (!PhotonNetwork.InRoom) return;

            Player localPlayer = PhotonNetwork.LocalPlayer;
            int localPlayerPos = PhotonBattle.GetPlayerPos(localPlayer);
            _teamNumber = (int)PhotonBattle.GetTeamNumber(localPlayerPos);

            Debug.Log($"TeamNumber {_teamNumber} pos {localPlayerPos} {localPlayer.GetDebugLabel()}");

            _anchorPresetAlpha = new()
            {
                min = _alphaDiamonds.anchorMin,
                max = _alphaDiamonds.anchorMax,
                anchoredPos = _alphaDiamonds.anchoredPosition
            };

            _anchorPresetBeta = new()
            {
                min = _betaDiamonds.anchorMin,
                max = _betaDiamonds.anchorMax,
                anchoredPos = _betaDiamonds.anchoredPosition
            };

            if (_teamNumber == 2)
            {
                RotateCamera();
            }*/
        }

        private void RotateCamera()
        {
            _camera.eulerAngles = new Vector3(0, 0, 180);
            _background.eulerAngles = new Vector3(0, 0, 180);
            _gridOverlay.eulerAngles = new Vector3(0, 0, 180);

            _alphaDiamonds.anchorMin = _anchorPresetBeta.min;
            _alphaDiamonds.anchorMax = _anchorPresetBeta.max;
            _alphaDiamonds.anchoredPosition = _anchorPresetBeta.anchoredPos;

            _betaDiamonds.anchorMin = _anchorPresetAlpha.min;
            _betaDiamonds.anchorMax = _anchorPresetAlpha.max;
            _betaDiamonds.anchoredPosition = _anchorPresetAlpha.anchoredPos;
        }
    }
}
