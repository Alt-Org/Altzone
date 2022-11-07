using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle0.Scripts.Battle.Players
{
    /// <summary>
    /// Helper class for <c>PlayerActor2</c> for visual representation.
    /// </summary>
    /// <remarks>
    /// Neither naming nor API is perfect for this ad hoc implementation.
    /// </remarks>
    internal class PlayerSettings2 : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private Transform _geometryRoot;
        [SerializeField] private Transform _avatarsRoot;
        [SerializeField] private Transform _shieldsRoot;

        private SimplePoseManager _avatar;
        private SimplePoseManager _shield;
        public IPoseManager GetAvatarPoseManager => _avatar;

        public IPoseManager GetShieldPoseManager => _shield;

        private void Awake()
        {
            _avatar = new SimplePoseManager(_avatarsRoot);
            _shield = new SimplePoseManager(_shieldsRoot);
        }

        public void Rotate(bool isUpsideDown)
        {
            _geometryRoot.Rotate(isUpsideDown);
        }

        public void SetAvatarsColliderCallback(Action<Collision2D> callback)
        {
            _avatar.SetColliderCallback(callback);
        }

        public void SetShieldsColliderCallback(Action<Collision2D> callback)
        {
            _shield.SetColliderCallback(callback);
        }
    }

    /// <summary>
    /// Internal state that defines how this pose behaves.
    /// </summary>
    internal class PoseState
    {
        public bool IsVisible;
        public BattlePlayMode PlayMode;
    }

    /// <summary>
    /// Helper class to manage <c>GameObject</c> hierarchy for <c>PlayerActor2</c> (shield and avatar state).
    /// </summary>
    internal class SimplePoseManager : IPoseManager
    {
        private readonly PoseState _state;
        private readonly int _childCount;
        private readonly GameObject[] _avatar;
        private readonly Collider2D[][] _colliders;

        public bool IsVisible => _state.IsVisible;
        public int MaxPoseIndex { get; }

        private GameObject _currentAvatar;
        private Collider2D[] _currentColliders;

        private Transform _parentTransform;

        public SimplePoseManager(Transform root)
        {
            _state = new PoseState();
            _childCount = root.childCount;
            _avatar = new GameObject[_childCount];
            _colliders = new Collider2D[_childCount][];
            for (var index = 0; index < _childCount; ++index)
            {
                var child = root.GetChild(index).gameObject;
                _avatar[index] = child;
                _colliders[index] = child.GetComponentsInChildren<Collider2D>(true);
            }
            MaxPoseIndex = _avatar.Length - 1;
        }

        public void SetColliderCallback(Action<Collision2D> callback)
        {
            foreach (var colliderArray in _colliders)
            {
                foreach (var collider in colliderArray)
                {
                    var parent = collider.gameObject;
                    var playerCollider = parent.GetComponent<PlayerCollider2>();
                    if (callback != null)
                    {
                        if (playerCollider == null)
                        {
                            playerCollider = parent.AddComponent<PlayerCollider2>();
                        }
                        playerCollider.Callback = callback;
                    }
                    else
                    {
                        if (playerCollider != null)
                        {
                            playerCollider.Callback = null;
                        }
                    }
                }
            }
        }

        public void SetVisible(bool isVisible)
        {
            _state.IsVisible = isVisible;
            _currentAvatar.SetActive(isVisible);
            var canCollide = _state.PlayMode.CanCollide();
            foreach (var collider in _currentColliders)
            {
                collider.enabled = canCollide;
            }
        }

        public void SetPlayMode(BattlePlayMode playMode)
        {
            _state.PlayMode = playMode;
            SetVisible(IsVisible);
        }

        public void SetPose(int poseIndex)
        {
            Assert.IsTrue(poseIndex >= 0 && poseIndex < _childCount);
            _currentAvatar.SetActive(false);
            _currentAvatar = _avatar[poseIndex];
            _currentColliders = _colliders[poseIndex];
            SetVisible(IsVisible);
        }

        public void Reset(int poseIndex, BattlePlayMode playMode, bool isVisible, string tag)
        {
            _currentAvatar = _avatar[0];
            _currentColliders = _colliders[0];

            var firstPosition = _currentAvatar.transform.position;
            var isSetTag = !string.IsNullOrWhiteSpace(tag);
            for (var i = 0; i < _childCount; ++i)
            {
                var child = _avatar[i];
                if (isSetTag)
                {
                    child.tag = tag;
                }
                child.transform.position = firstPosition;
                child.gameObject.SetActive(false);
            }

            _state.PlayMode = playMode;
            _state.IsVisible = isVisible;
            SetPose(poseIndex);
        }
    }
}