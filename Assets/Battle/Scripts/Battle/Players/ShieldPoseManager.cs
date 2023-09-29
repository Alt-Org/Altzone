using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class ShieldPoseManager : MonoBehaviour
    {
        private class Shield
        {
            public GameObject ShieldGameObject { get; }
            public GameObject ShieldHitbox { get; }
            public SpriteRenderer ShieldSpriteRenderer { get; }

            public Shield(GameObject shieldGameObject)
            {
                ShieldGameObject = shieldGameObject;
                ShieldHitbox = shieldGameObject.transform.Find("Colliders").gameObject;
                ShieldSpriteRenderer = shieldGameObject.GetComponentInChildren<SpriteRenderer>();
            }
        }

        private Shield[] _shields;
        private Shield _currentPose;
        private int _maxPoseIndex;
        private bool _hitboxActive;
        private bool _showShield;

        private void Awake()
        {
            int childCount = transform.childCount;
            _maxPoseIndex = childCount - 1;
            _shields = new Shield[childCount];
            for (int i = 0; i <= _maxPoseIndex; i++)
            {
                _shields[i] = new Shield(transform.GetChild(i).gameObject);
                _shields[i].ShieldGameObject.SetActive(false);
            }
            _shields[0].ShieldGameObject.SetActive(true);
        }

        public int MaxPoseIndex => _maxPoseIndex;

        public void SetPose(int poseIndex)
        {
            if (_currentPose != null)
            {
                _currentPose.ShieldGameObject.SetActive(false);
            }
            _currentPose = _shields[poseIndex];
            _currentPose.ShieldGameObject.transform.localPosition = Vector3.zero;
            _currentPose.ShieldGameObject.SetActive(true);
            _currentPose.ShieldHitbox.SetActive(_hitboxActive);
            _currentPose.ShieldSpriteRenderer.enabled = _showShield;
        }

        public void SetHitboxActive(bool active)
        {
            _hitboxActive = active;
            _currentPose.ShieldHitbox.SetActive(active);
        }

        public void SetShow(bool show)
        {
            _showShield = show;
            _currentPose.ShieldSpriteRenderer.enabled = show;
        }
    }
}
