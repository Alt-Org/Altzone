using UnityEngine;

namespace Raid.Scripts
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class RaidPosition : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private Collider2D _areaCollider;

        [Header("Live Data"), SerializeField] private Bounds _areaBounds;

        public Bounds GetRaidArea => _areaBounds;

        private void Awake()
        {
            _areaBounds = _areaCollider.bounds;
            _areaCollider.enabled = false;
            Debug.Log($"Raid area {GetRaidArea}");
        }
    }
}