using System;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Battle.Scripts.Battle.Room;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.ToastMessages;
using Prg.Scripts.Common.Util;
using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle.Ball
{
    /// <summary>
    /// <c>BallManager</c> listens events from the <c>IBall</c> and forwards them where applicable - even to the <c>IBall</c> itself.
    /// </summary>
    public class BallManager : MonoBehaviour
    {
        private static readonly StringCache HeadMessage = new StringCache(new[] { string.Empty, "HEAD1", "HEAD2", "HEAD3", "HEAD4" });
        private static readonly StringCache HitMessage = new StringCache(new[] { string.Empty, "HIT1", "HIT2", "HIT3", "HIT4" });

        [Serializable]
        internal class State
        {
            public bool _isRedTeamActive;
            public bool _isBlueTeamActive;
        }

        [SerializeField] private State _state;

        private IBall _ball;
        private IBrickManager _brickManager;
        private PhotonView _photonView;

        private void Awake()
        {
            Debug.Log("Awake");
            _state = new State();
            _ball = Context.GetBall;
            _brickManager = Context.GetBrickManager;
            _photonView = PhotonView.Get(this);
            var ballCollision = _ball.BallCollision;
            ballCollision.OnHeadCollision = OnHeadCollision;
            ballCollision.OnShieldCollision = OnShieldCollision;
            ballCollision.OnBrickCollision = OnBrickCollision;
            ballCollision.OnWallCollision = OnWallCollision;
            ballCollision.OnEnterTeamArea = OnEnterTeamArea;
            ballCollision.OnExitTeamArea = OnExitTeamArea;

            ScoreFlashNet.RegisterEventListener();
        }

        #region IBallCollision callback events

        private void OnHeadCollision(Collision2D collision)
        {
            var contactPoint = collision.GetFirstContactPoint();
            var other = collision.gameObject;
            Debug.Log($"{other.GetFullPath()} @ point {contactPoint.point} {_photonView.Owner.GetDebugLabel()}");
            var playerActor = other.GetComponentInParent<IPlayerActor>();
            playerActor.HeadCollision();
            var scoreType = playerActor.TeamNumber == PhotonBattle.TeamBlueValue ? ScoreType.RedHead : ScoreType.BlueHead;
            this.Publish(new ScoreManager.ScoreEvent(scoreType));
            if (PhotonNetwork.IsMasterClient)
            {
                ScoreFlashNet.Push(HeadMessage[playerActor.PlayerPos], contactPoint.point);
            }
        }

        private void OnShieldCollision(Collision2D collision)
        {
            var contactPoint = collision.GetFirstContactPoint();
            var other = collision.gameObject;
            Debug.Log($"{other.GetFullPath()}");
            var playerActor = other.GetComponentInParent<IPlayerActor>();
            playerActor.ShieldCollision();
            if (_photonView.Owner.IsMasterClient)
            {
                ScoreFlashNet.Push(HitMessage[playerActor.PlayerPos], contactPoint.point);
            }
        }

        private void OnBrickCollision(Collision2D collision)
        {
            //Debug.Log($"onBrickCollision {other.name}");
            _brickManager.DeleteBrick(collision.gameObject);
        }

        private void OnWallCollision(Collision2D collision)
        {
            var contactPoint = collision.GetFirstContactPoint();
            var other = collision.gameObject;
            Debug.Log($"onWallCollision {other.name} {other.tag} @ point {contactPoint.point}");
            if (other.CompareTag(Tags.BlueTeam))
            {
                this.Publish(new ScoreManager.ScoreEvent(ScoreType.BlueWall));
                if (_photonView.Owner.IsMasterClient)
                {
                    ScoreFlashNet.Push("WALL", contactPoint.point);
                }
            }
            else if (other.CompareTag(Tags.RedTeam))
            {
                this.Publish(new ScoreManager.ScoreEvent(ScoreType.RedWall));
                if (_photonView.Owner.IsMasterClient)
                {
                    ScoreFlashNet.Push("WALL", contactPoint.point);
                }
            }
        }

        private void OnEnterTeamArea(GameObject other)
        {
            //Debug.Log($"{other.tag}");
            switch (other.tag)
            {
                case Tags.RedTeam:
                    _state._isRedTeamActive = true;
                    SetBallColor(_ball, _state);
                    return;
                case Tags.BlueTeam:
                    _state._isBlueTeamActive = true;
                    SetBallColor(_ball, _state);
                    return;
            }
            Debug.Log($"UNHANDLED onEnterTeamArea {other.name} {other.tag}");
        }

        private void OnExitTeamArea(GameObject other)
        {
            //Debug.Log($"{other.tag}");
            switch (other.tag)
            {
                case Tags.RedTeam:
                    _state._isRedTeamActive = false;
                    SetBallColor(_ball, _state);
                    return;
                case Tags.BlueTeam:
                    _state._isBlueTeamActive = false;
                    SetBallColor(_ball, _state);
                    return;
            }
            Debug.Log($"UNHANDLED onExitTeamArea {other.name} {other.tag}");
        }

        private static void SetBallColor(IBall ball, State state)
        {
            if (state._isRedTeamActive && !state._isBlueTeamActive)
            {
                //Debug.Log($"blue {(state._isBlueTeamActive ? 1 : 0)} red {(state._isRedTeamActive ? 1 : 0)} RED");
                ball.SetColor(BallColor.RedTeam);
                ball.Publish(new ActiveTeamEvent(PhotonBattle.TeamRedValue));
                return;
            }
            if (state._isBlueTeamActive && !state._isRedTeamActive)
            {
                //Debug.Log($"blue {(state._isBlueTeamActive ? 1 : 0)} red {(state._isRedTeamActive ? 1 : 0)} BLUE");
                ball.SetColor(BallColor.BlueTeam);
                ball.Publish(new ActiveTeamEvent(PhotonBattle.TeamBlueValue));
                return;
            }
            //Debug.Log($"blue {(state._isBlueTeamActive ? 1 : 0)} red {(state._isRedTeamActive ? 1 : 0)} INDETERMINATE");
            ball.SetColor(BallColor.NoTeam);
            ball.Publish(new ActiveTeamEvent(PhotonBattle.NoTeamValue));
        }

        #endregion

        /// <summary>
        /// Active team event is sent whenever active team is changed.
        /// </summary>
        /// <remarks>
        /// <c>TeamIndex</c> is -1 when no team is active.
        /// </remarks>
        internal class ActiveTeamEvent
        {
            public readonly int TeamIndex;

            public ActiveTeamEvent(int teamIndex)
            {
                TeamIndex = teamIndex;
            }
        }
    }
}