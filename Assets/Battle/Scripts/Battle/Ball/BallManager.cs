using System;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Battle.Scripts.Battle.Room;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity;
using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle.Ball
{
    /// <summary>
    /// <c>BallManager</c> listens events from the <c>IBall</c> and forwards them where applicable - even to the <c>IBall</c> itself.
    /// </summary>
    public class BallManager : MonoBehaviour
    {
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
            Debug.Log($"onHeadCollision {other.GetFullPath()} @ point {contactPoint.point}");
            var playerActor = other.GetComponentInParent<IPlayerActor>();
            playerActor.HeadCollision();
            var scoreType = playerActor.TeamNumber == PhotonBattle.TeamBlueValue ? ScoreType.RedHead : ScoreType.BlueHead;
            this.Publish(new ScoreManager.ScoreEvent(scoreType));
            if (_photonView.Owner.IsMasterClient)
            {
                ScoreFlashNet.Push("HEAD", contactPoint.point);
            }
        }

        private void OnShieldCollision(Collision2D collision)
        {
            var other = collision.gameObject;
            Debug.Log($"onShieldCollision {other.GetFullPath()}");
            var playerActor = other.GetComponentInParent<IPlayerActor>();
            playerActor.ShieldCollision();
            if (_photonView.Owner.IsMasterClient)
            {
                ScoreFlashNet.Push("HIT");
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
            //Debug.Log($"onEnterTeamArea {other.name} {other.tag}");
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
            //Debug.Log($"onExitTeamArea {other.name} {other.tag}");
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
                ball.SetColor(BallColor.RedTeam);
                ball.Publish(new ActiveTeamEvent(PhotonBattle.TeamRedValue));
                return;
            }
            if (state._isBlueTeamActive && !state._isRedTeamActive)
            {
                ball.SetColor(BallColor.BlueTeam);
                ball.Publish(new ActiveTeamEvent(PhotonBattle.TeamBlueValue));
                return;
            }
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
