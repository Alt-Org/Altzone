using System;
using Altzone.Scripts.Battle;
using Battle.Scripts.interfaces;
using Battle.Scripts.Scene;
using UnityEngine;

namespace Battle.Scripts.Ball
{
    /// <summary>
    /// Collision manager for the ball.
    /// </summary>
    public class BallCollision : MonoBehaviour, IBallCollisionSource
    {
        [Header("Live Data"), SerializeField] private bool isUpper;
        [SerializeField] private bool isLower;

        private Collider2D upperTeam;
        private Collider2D lowerTeam;

        Action<int> IBallCollisionSource.onCurrentTeamChanged { get; set; }

        Action<Collision2D> IBallCollisionSource.onCollision2D { get; set; }

        private void Awake()
        {
            var sceneConfig = SceneConfig.Get();
            upperTeam = sceneConfig.upperTeamCollider;
            lowerTeam = sceneConfig.lowerTeamCollider;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (other.gameObject.layer == 0)
            {
                return; // Ignore colliders without a specific layer(s)!
            }
            Debug.Log($"OnCollisionEnter2D {other.gameObject.name}");
            ((IBallCollisionSource)this).onCollision2D?.Invoke(other);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (other.Equals(upperTeam))
            {
                isUpper = true;
                checkBallAndTeam();
                return;
            }
            if (other.Equals(lowerTeam))
            {
                isLower = true;
                checkBallAndTeam();
                return;
            }
            Debug.Log($"OnTriggerEnter2D UNHANDLED {other.gameObject.name}");
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.Equals(upperTeam))
            {
                isUpper = false;
                checkBallAndTeam();
            }
            else if (other.Equals(lowerTeam))
            {
                isLower = false;
                checkBallAndTeam();
            }
        }

        private void checkBallAndTeam()
        {
            if (isUpper && !isLower)
            {
                // activate upper team
                ((IBallCollisionSource)this).onCurrentTeamChanged?.Invoke(PhotonBattle.TeamRedValue);
            }
            else if (isLower && !isUpper)
            {
                // Activate lower team
                ((IBallCollisionSource)this).onCurrentTeamChanged?.Invoke(PhotonBattle.TeamBlueValue);
            }
            else
            {
                // between teams
                ((IBallCollisionSource)this).onCurrentTeamChanged?.Invoke(PhotonBattle.NoTeamValue);
            }
        }
    }
}