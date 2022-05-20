using System;
using System.Diagnostics;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Ball
{
    internal enum BallState : byte
    {
        NoTeam = 0,
        RedTeam = 1,
        BlueTeam = 2,
        Ghosted = 3,
        Hidden = 4,
    }

    internal class BallManager : MonoBehaviour
    {
        [Serializable]
        internal class DebugSettings
        {
            public bool _isShowBallText;
            public TextMeshPro _ballText;
        }

        private static readonly BallState[] BallStates =
            { BallState.NoTeam, BallState.RedTeam, BallState.BlueTeam, BallState.Ghosted, BallState.Hidden };

        private static readonly bool[] ColliderStates = { true, true, true, false, false };

        [Header("Settings"), SerializeField] private GameObject _ballCollider;
        [SerializeField] private GameObject _spriteNoTeam;
        [SerializeField] private GameObject _spriteRedTeam;
        [SerializeField] private GameObject _spriteBlueTeam;
        [SerializeField] private GameObject _spriteGhosted;
        [SerializeField] private GameObject _spriteHidden;

        [Header("Live Data"), SerializeField] private BallState _ballState;

        [Header("Ball Constraints")] public float _minBallSpeed;
        public float _maxBallSpeed;
        
        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private PhotonView _photonView;
        private Rigidbody2D _rigidbody;
        private GameObject[] _sprites;

        private void Awake()
        {
            Debug.Log($"{name}");
            _photonView = PhotonView.Get(this);
            _rigidbody = GetComponent<Rigidbody2D>();
            _sprites = new[] { _spriteNoTeam, _spriteRedTeam, _spriteBlueTeam, _spriteGhosted, _spriteHidden };
            if (_debug._ballText == null)
            {
                _debug._isShowBallText = false;
            }
            else if (!_debug._isShowBallText)
            {
                _debug._ballText.gameObject.SetActive(false);
            }
            SetBallState(BallState.Ghosted);
        }

        private void SetBallState(BallState ballState)
        {
            _ballState = ballState;
            var stateIndex = (int)ballState;
            _ballCollider.SetActive(ColliderStates[stateIndex]);
            for (var i = 0; i < BallStates.Length; ++i)
            {
                _sprites[i].SetActive(BallStates[i] == ballState);
            }
            if (_debug._isShowBallText)
            {
                _debug._ballText.gameObject.SetActive(_ballState != BallState.Hidden);
            }
            UpdateBallText();
        }

        #region Debugging

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void UpdateBallText()
        {
            if (!_debug._isShowBallText)
            {
                return;
            }
            _debug._ballText.text = $"{_rigidbody.velocity.magnitude}";
        }

        #endregion
    }
}