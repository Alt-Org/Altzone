using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

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
        private PlayerActor2 _playerActor;
        private bool _isLocal;
        private bool _isApplicationQuitting;

        private void Awake()
        {
            Application.quitting += () => _isApplicationQuitting = true;
        }
        
        private void OnEnable()
        {
            if (_playerActor != null)
            {
                return;
            }
            print("++");
            name = name.Replace("(Clone)", string.Empty);
            var photonView = PhotonView.Get(this);
            var player = photonView.Owner;
            var playerPrefab = GetPlayerPrefab(player);
            Debug.Log($"{name} prefab {playerPrefab.name}");
            Debug.Log($"owner {player}");
            var instance = Instantiate(playerPrefab);
            _playerActor = instance.GetComponent<PlayerActor2>();
            Assert.IsNotNull(_playerActor, "_playerActor != null");
            var myTransform = GetComponent<Transform>();
            var playerTransform = _playerActor.GetComponent<Transform>();
            playerTransform.position = myTransform.position;
            _isLocal = photonView.Owner.IsLocal;
            _playerActor.SetPhotonView(photonView);
            _playerActor.gameObject.SetActive(true);
            Debug.Log($"done {name}");

            StartCoroutine(WaitForSystemToStabilize(playerPrefab.name));
        }

        private void OnDestroy()
        {
            if (_isApplicationQuitting)
            {
                return;
            }
            if (_isLocal)
            {
                return;
            }
            Debug.Log($"{name}");
            if (_playerActor != null)
            {
                _playerActor.OnNetworkLost();
            }
        }

        private IEnumerator WaitForSystemToStabilize(string playerPrefabName)
        {
            var networkSync = GetComponent<NetworkSync>();
            if (networkSync.enabled)
            {
                yield return new WaitUntil(() => networkSync.enabled = false);
            }
            Debug.Log($"OnEnable re-parent {name} for {playerPrefabName}");
            // Re-parent us under player so that we can be detached without disturbing the player so much.
            var myTransform = GetComponent<Transform>();
            var playerTransform = _playerActor.GetComponent<Transform>();
            myTransform.parent = playerTransform;
        }

        private static GameObject GetPlayerPrefab(Player player)
        {
            var model = PhotonBattle.GetCharacterModelForRoom(player);
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var playerPrefab = runtimeGameConfig.Prefabs.GetPlayerPrefab(model.MainDefence);
            return playerPrefab;
        }
    }
}