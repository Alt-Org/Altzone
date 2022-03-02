using System.Collections;
using Battle.Scripts.Battle.Photon;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.Players2
{
    /// <summary>
    /// Prefab for <c>Photon</c> to instantiate over network without any visual <c>GameObject</c>.
    /// </summary>
    /// <remarks>
    /// Functional <c>GameObject</c> is added later (by us) and
    /// we can be detached from it any time if required or when connection is lost etc. errors.
    /// </remarks>
    [RequireComponent(typeof(PhotonView))]
    public class PhotonPlayer : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private PlayerActor2 _playerActorPrefab;

        private PlayerActor2 _playerActor;

        private void OnEnable()
        {
            if (_playerActor != null)
            {
                return;
            }
            name = name.Replace("(Clone)", string.Empty);
            Debug.Log($"OnEnable start {name} for {_playerActorPrefab.name}");
            _playerActor = Instantiate(_playerActorPrefab);
            var myTransform = GetComponent<Transform>();
            var playerTransform = _playerActor.GetComponent<Transform>();
            playerTransform.position = myTransform.position;
            _playerActor.SetPhotonView(PhotonView.Get(this));
            _playerActor.gameObject.SetActive(true);
            Debug.Log($"OnEnable done {name} for {_playerActorPrefab.name}");

            StartCoroutine(WaitForSystemToStabilize());
        }

        private IEnumerator WaitForSystemToStabilize()
        {
            var networkSync = GetComponent<NetworkSync>();
            if (networkSync.enabled)
            {
                yield return new WaitUntil(() => networkSync.enabled = false);
            }
            Debug.Log($"OnEnable re-parent {name} for {_playerActorPrefab.name}");
            // Re-parent us under player so that we can be detached without disturbing the player so much.
            var myTransform = GetComponent<Transform>();
            var playerTransform = _playerActor.GetComponent<Transform>();
            myTransform.parent = playerTransform;
            enabled = false;
        }
    }
}