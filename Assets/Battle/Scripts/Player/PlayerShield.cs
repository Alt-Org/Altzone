using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.interfaces;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Player
{
    public interface IPlayerShield
    {
        void showShield();
        void ghostShield();
        void hideShield();
    }

    /// <summary>
    /// Controls player shield and synchronizes its state over network using <c>RPC</c>.
    /// </summary>
    /// <remarks>
    /// Shield can be visible or hidden and it can be "bent" when gets hit by the ball.
    /// </remarks>
    public class PlayerShield : MonoBehaviour, IPlayerShield
    {
        [Header("Settings"), SerializeField] private GameObject upperShield;
        [SerializeField] private GameObject lowerShield;
        [SerializeField] private GameObject ghostedUpperShield;
        [SerializeField] private GameObject ghostedLowerShield;

        [Header("Live Data"), SerializeField] protected PhotonView _photonView;
        [SerializeField] protected Transform _transform;
        [SerializeField] protected PlayerShield _otherPlayerShield;
        [SerializeField] protected Transform _otherTransform;
        [SerializeField] private GameObject currentShield;
        [SerializeField] private GameObject currentGhostedShield;
        [SerializeField] private float sqrShieldDistance;
        [SerializeField] private float sqrDistance;
        [SerializeField] private bool isShieldVisible;
        [SerializeField] private bool isShieldGhosted;
        [SerializeField] private bool isShieldActive;

        private void Awake()
        {
            _photonView = PhotonView.Get(this);
            _transform = GetComponent<Transform>();
            var player = _photonView.Owner;
            var model = PhotonBattle.GetPlayerCharacterModel(player);
            var multiplier = RuntimeGameConfig.Get().Variables._shieldDistanceMultiplier;
            sqrShieldDistance = model.Defence * multiplier * 2f;
            upperShield.SetActive(false);
            lowerShield.SetActive(false);
            ghostedUpperShield.SetActive(false);
            ghostedLowerShield.SetActive(false);
            isShieldVisible = false;
            isShieldGhosted = false;
            isShieldActive = false;
            enabled = false;
        }

        private void OnEnable()
        {
            var playerActor = GetComponent<PlayerActor>() as IPlayerActor;
            if (playerActor.TeamNumber == PhotonBattle.TeamBlueValue)
            {
                currentShield = upperShield;
                currentGhostedShield = ghostedUpperShield;
            }
            else
            {
                currentShield = lowerShield;
                currentGhostedShield = ghostedLowerShield;
            }
            Debug.Log($"OnEnable {name} currentShield={currentShield.name}");
            var teamMate = playerActor.TeamMate;
            if (teamMate == null)
            {
                var features = RuntimeGameConfig.Get().Features;
                if (features._isSinglePlayerShieldOn)
                {
                    _otherTransform = _transform; // measure distance to ourself: shield will be always on!
                    Debug.Log($"OnEnable {name} shield is always ON");
                    return;
                }
                enabled = false;
                return;
            }
            _otherTransform = ((PlayerActor)teamMate).transform;
            _otherPlayerShield = _otherTransform.GetComponent<PlayerShield>();
            Debug.Log($"OnEnable {name} teamMate={_otherTransform.gameObject.name} shield={currentShield.name}");
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable name={name}");
            if (_otherPlayerShield != null)
            {
                // Must disable our team mate - for now
                _otherPlayerShield.enabled = false;
            }
        }

        private void Update()
        {
            sqrDistance = Mathf.Abs((_transform.position - _otherTransform.position).sqrMagnitude);
            if (sqrDistance < sqrShieldDistance)
            {
                if (!isShieldVisible)
                {
                    isShieldVisible = true;
                    if (isShieldActive)
                    {
                        if (isShieldGhosted)
                        {
                            currentGhostedShield.SetActive(true);
                            currentShield.SetActive(false);
                        }
                        else
                        {
                            currentShield.SetActive(true);
                            currentGhostedShield.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                if (isShieldVisible)
                {
                    isShieldVisible = false;
                    currentShield.SetActive(false);
                    currentGhostedShield.SetActive(false);
                }
            }
        }

        void IPlayerShield.showShield()
        {
            isShieldActive = true;
            isShieldGhosted = false;
            if (isShieldVisible)
            {
                currentShield.SetActive(true);
                currentGhostedShield.SetActive(false);
            }
        }

        void IPlayerShield.ghostShield()
        {
            isShieldActive = true;
            isShieldGhosted = true;
            if (isShieldVisible)
            {
                currentShield.SetActive(false);
                currentGhostedShield.SetActive(true);
            }
        }

        void IPlayerShield.hideShield() // This is a bit fuzzy semantics because visibility is controlled by distance to team mate
        {
            isShieldActive = false;
            isShieldGhosted = false;
            if (isShieldVisible)
            {
                isShieldVisible = false;
                currentShield.SetActive(false);
                currentGhostedShield.SetActive(false);
            }
        }
    }
}