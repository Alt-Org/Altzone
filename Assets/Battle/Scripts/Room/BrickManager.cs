using System.Collections.Generic;
using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Battle.Scripts.Room
{
    /// <summary>
    /// Optional brick manager for the room that synchronizes brick state over network.
    /// </summary>
    public class BrickManager : MonoBehaviour
    {
        private const int msgDeleteBrick = PhotonEventDispatcher.eventCodeBase + 5;

        private static BrickManager Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<BrickManager>();
            }
            return _Instance;
        }

        private static BrickManager _Instance;

        [SerializeField] private GameObject upperBricks;
        [SerializeField] private GameObject lowerBricks;
        [SerializeField] private int brickCount;

        private readonly Dictionary<int, BrickMarker> bricks = new Dictionary<int, BrickMarker>();

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            Debug.Log("Awake");
            createBrickMarkersFor(upperBricks.transform);
            createBrickMarkersFor(lowerBricks.transform);
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(msgDeleteBrick, data => { onDeleteBrick(data.CustomData); });
        }

        private void OnDestroy()
        {
            _Instance = null;
        }

        private void sendDeleteBrick(int brickId)
        {
            photonEventDispatcher.RaiseEvent(msgDeleteBrick, brickId);
        }

        private void onDeleteBrick(object data)
        {
            var brickId = (int)data;
            if (bricks.TryGetValue(brickId, out var brickMarker))
            {
                bricks.Remove(brickId);
                brickMarker.destroyBrick();
            }
        }

        private void createBrickMarkersFor(Transform parentTransform)
        {
            var childCount = parentTransform.childCount;
            for (var i = 0; i < childCount; ++i)
            {
                var child = parentTransform.GetChild(i).gameObject;
                var marker = child.AddComponent<BrickMarker>();
                marker.BrickId = ++brickCount;
                bricks.Add(marker.BrickId, marker);
            }
        }

        private void _deleteBrick(int brickId)
        {
            Debug.Log($"deleteBrick id={brickId}");
            sendDeleteBrick(brickId);
        }

        public static void deleteBrick(GameObject gameObject)
        {
            var manager = Get();
            if (!manager)
            {
                return;
            }
            var brickMarker = gameObject.GetComponent<BrickMarker>();
            manager._deleteBrick(brickMarker.BrickId);
        }
    }
}