using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Players2
{
    [RequireComponent(typeof(PhotonView))]
    public class PhotonPlayerRpc : MonoBehaviour
    {
        private PhotonView _photonView;

        private Action<int> _onSendPlayMode;
        private Action<bool> _onSendShieldVisibility;
        private Action<int> _onSendShieldRotation;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
        }

        public void SendPlayMode(Action<int> callback)
        {
            _onSendPlayMode = callback;
        }

        public void SendShieldVisibility(Action<bool> callback)
        {
            _onSendShieldVisibility = callback;
        }

        public void SendShieldRotation(Action<int> callback)
        {
            _onSendShieldRotation = callback;
        }

        public void SendPlayMode(Action<int> callback, int playMode)
        {
            Assert.IsTrue(callback == _onSendPlayMode, "callback == _onSendPlayMode");
            _photonView.RPC(nameof(SendPlayModeRpc), RpcTarget.All, playMode);
        }

        public void SendShieldVisibility(Action<bool> callback, bool isVisible)
        {
            Assert.IsTrue(callback == _onSendShieldVisibility, "callback == _onSendShieldVisibility");
            _photonView.RPC(nameof(SendShieldVisibilityRpc), RpcTarget.All, isVisible);
        }

        public void SendShieldRotation(Action<int> callback, int rotationIndex)
        {
            Assert.IsTrue(callback == _onSendShieldRotation, "callback == _onSendShieldRotation");
            _photonView.RPC(nameof(SendShieldRotationRpc), RpcTarget.All, rotationIndex);
        }

        [PunRPC]
        private void SendPlayModeRpc(int playMode)
        {
            _onSendPlayMode(playMode);
        }

        [PunRPC]
        private void SendShieldVisibilityRpc(bool isVisible)
        {
            _onSendShieldVisibility(isVisible);
        }

        [PunRPC]
        private void SendShieldRotationRpc(int rotationIndex)
        {
            _onSendShieldRotation(rotationIndex);
        }
    }
}
