using Battle.View.Game;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace Battle.View.Player
{
    public class BattlePlayerClassProjectorViewController : BattlePlayerClassBaseViewController
    {
        [SerializeField] private GameObject _movingShield;
        [SerializeField] private bool _movingShieldFlipped;

        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Projector;
        public override void OnShieldTakeDamage(EventBattleShieldTakeDamage e)
        {
            if (e.DefenceValue <= FP._0)
            {
                _movingShield.SetActive(false);
            }
        }

        public override void OnUpdateView()
        {
            if (_movingShield == null) return;

            GameObject projectileRef = BattleGameViewController.ProjectileReference;
            if (projectileRef == null) return;

            Vector3 origin = transform.position;
            Vector3 toProjectile = projectileRef.transform.position - origin;

            if (toProjectile.sqrMagnitude < 0.0001f) return;

            toProjectile.y = 0f;

            if (toProjectile.sqrMagnitude > 0.0001f)
            {
                Quaternion yawRot = Quaternion.LookRotation(toProjectile, Vector3.up);

                if (_movingShieldFlipped)
                {
                    yawRot *= Quaternion.Euler(0f, 180f, 0f);
                }

                Vector3 currentAngle = _movingShield.transform.rotation.eulerAngles;
                _movingShield.transform.rotation = Quaternion.Euler(currentAngle.x, yawRot.eulerAngles.y, currentAngle.z);
            }

            float radius = Vector3.Distance(_movingShield.transform.position, origin);
            _movingShield.transform.position = origin + toProjectile.normalized * radius;
        }
    }
}
