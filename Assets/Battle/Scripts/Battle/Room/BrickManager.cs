using System.Collections.Generic;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.interfaces;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Room
{
    /// <summary>
    /// Brick manager for the room that synchronizes brick state over network.
    /// </summary>
    internal class BrickManager : MonoBehaviour, IBrickManager
    {
        private const int MsgDeleteBrick = PhotonEventDispatcher.EventCodeBase + 0;

        [Header("Settings"), SerializeField] private GameObject _upperBricks;
        [SerializeField] private GameObject _lowerBricks;

        private readonly Dictionary<int, IdMarker> _bricks = new Dictionary<int, IdMarker>();

        private PhotonEventDispatcher _photonEventDispatcher;

        private void Awake()
        {
            var isBricksVisible = RuntimeGameConfig.Get().Features._isBricksVisible;
            Debug.Log($"Awake isBricksVisible {isBricksVisible}");
            if (!isBricksVisible)
            {
                _upperBricks.SetActive(false);
                _lowerBricks.SetActive(false);
                return;
            }
            _upperBricks.SetActive(true);
            _lowerBricks.SetActive(true);
            CreateBrickMarkersFrom(_upperBricks.transform, _bricks);
            CreateBrickMarkersFrom(_lowerBricks.transform, _bricks);
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.RegisterEventListener(MsgDeleteBrick, data => { OnDeleteBrick(data.CustomData); });
        }

        #region Photon Events

        private void OnDeleteBrick(object data)
        {
            var payload = (byte[])data;
            Assert.AreEqual(2, payload.Length, "Invalid message length");
            Assert.AreEqual((byte)MsgDeleteBrick, payload[0], "Invalid message id");
            var brickId = (int)payload[1];
            if (_bricks.TryGetValue(brickId, out var marker))
            {
                _bricks.Remove(brickId);
                DestroyBrick(marker);
            }
        }

        private void SendDeleteBrick(GameObject brick)
        {
            var brickId = brick.GetComponent<IdMarker>().Id;
            var payload = new[] { (byte)MsgDeleteBrick, (byte)brickId };
            _photonEventDispatcher.RaiseEvent(MsgDeleteBrick, payload);
        }

        #endregion

        #region IBrickManager

        void IBrickManager.DeleteBrick(GameObject brick)
        {
            SendDeleteBrick(brick);
        }

        #endregion

        #region Brick Management

        private static void CreateBrickMarkersFrom(Transform parentTransform, IDictionary<int, IdMarker> bricks)
        {
            var childCount = parentTransform.childCount;
            for (var i = 0; i < childCount; ++i)
            {
                var child = parentTransform.GetChild(i).gameObject;
                var marker = child.AddComponent<IdMarker>();
                bricks.Add(marker.Id, marker);
            }
        }

        private static void DestroyBrick(IdMarker marker)
        {
            Debug.Log($"destroyBrick #{marker.Id} {marker.name}");
            var gameObject = marker.gameObject;
            Assert.IsTrue(gameObject.activeSelf, "GameObject is not active for destroy");
            gameObject.SetActive(false);
        }

        #endregion
    }
}