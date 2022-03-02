using Altzone.Scripts.Battle;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Battle.Scripts.Battle.PlayerConnect
{
    internal class PlayerLineConnector : MonoBehaviourPunCallbacks, IPlayerLineConnector
    {
        [Header("Settings"), SerializeField] private LineRenderer _line;
        [SerializeField] private Vector3 _referencePoint;

        [Header("Live Data"), SerializeField] private Transform _transformA;
        [SerializeField] private Transform _transformB;
        [SerializeField] private float _distanceA;
        [SerializeField] private float _distanceB;

        private IPlayerActor _playerActorA;
        private IPlayerActor _playerActorB;
        private Vector3 _positionA;
        private Vector3 _positionB;

        private void Update()
        {
            _positionA = _transformA.position;
            _positionB = _transformB.position;
            _distanceA = Mathf.Abs(_referencePoint.y - _positionA.y);
            _distanceB = Mathf.Abs(_referencePoint.y - _positionB.y);
            // Index 1 is the arrow head!
            if (_distanceA < _distanceB)
            {
                _line.SetPosition(0, _positionB);
                _line.SetPosition(1, _positionA);
            }
            else
            {
                _line.SetPosition(0, _positionA);
                _line.SetPosition(1, _positionB);
            }
        }

        void IPlayerLineConnector.Connect(IPlayerActor playerActor)
        {
            _transformA = playerActor.Transform;
            _playerActorA = playerActor;
            if (playerActor.TeamMate != null)
            {
                _transformB = playerActor.TeamMate.Transform;
                _playerActorB = playerActor.TeamMate;
            }
            else
            {
                // We assume that we are positioned somewhere in team's gameplay area so that line renderer can work with one player.
                _transformB = GetComponent<Transform>();
                _playerActorB = null;
            }
            Debug.Log($"Connect {_transformA.name} <-> {_transformB.name}");
            gameObject.SetActive(true);
        }

        PlayerLineResult IPlayerLineConnector.GetNearest()
        {
            Debug.Log($"GetNearest {_transformA.name} a={_distanceA} b={_distanceB}");
            if (_playerActorB == null || _distanceA < _distanceB)
            {
                return new PlayerLineResult(_playerActorA, _distanceA, _positionA - _positionB);
            }
            return new PlayerLineResult(_playerActorB, _distanceB, _positionB - _positionA);
        }

        void IPlayerLineConnector.Hide()
        {
            Debug.Log($"Hide {_transformA.name}");
            gameObject.SetActive(false);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (!PhotonBattle.IsRealPlayer(otherPlayer))
            {
                return;
            }
            var playerActor = Context.GetPlayer(PhotonBattle.GetPlayerPos(otherPlayer));
            if (playerActor.TeamNumber == _playerActorA.TeamNumber)
            {
                ((IPlayerLineConnector)this).Hide();
            }
        }
    }
}