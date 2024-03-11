using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerClass : MonoBehaviour
    {
        [SerializeField] private float impactForce;
        [SerializeField] private PlayerActor playerActor; 

        private void Awake()
        {
            if (playerActor == null)
            {
                Debug.LogError("PlayerActor is not assigned to PlayerClass script!", this);
                return;
            }

            impactForce = playerActor.ImpactForce;
        }

        private void OnValidate()
        {
            if (playerActor != null)
            {
                playerActor.SetImpactForce(impactForce);
            }
        }

        public void SetImpactForce(float newImpactForce)
        {
            if (playerActor != null)
            {
                playerActor.SetImpactForce(newImpactForce);
            }
        }
    }
}



