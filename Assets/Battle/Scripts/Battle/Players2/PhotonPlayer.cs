using System.Collections;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Photon;
using Photon.Pun;
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

        private void OnEnable()
        {
            if (_playerActor != null)
            {
                return;
            }
            name = name.Replace("(Clone)", string.Empty);
            var playerPrefab = GetLocalPlayerPrefab();
            Debug.Log($"OnEnable start {name} for {playerPrefab.name}");
            var instance = Instantiate(playerPrefab);
            _playerActor = instance.GetComponent<PlayerActor2>();
            Assert.IsNotNull(_playerActor, "_playerActor != null");
            var myTransform = GetComponent<Transform>();
            var playerTransform = _playerActor.GetComponent<Transform>();
            playerTransform.position = myTransform.position;
            _playerActor.SetPhotonView(PhotonView.Get(this));
            _playerActor.gameObject.SetActive(true);
            Debug.Log($"OnEnable done {name} for {playerPrefab.name}");

            StartCoroutine(WaitForSystemToStabilize(playerPrefab.name));
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
            enabled = false;
        }

        private static GameObject GetLocalPlayerPrefab()
        {
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var playerDataCache = runtimeGameConfig.PlayerDataCache;
            var defence = playerDataCache.CharacterModel.MainDefence;
            var playerPrefab = runtimeGameConfig.Prefabs.GetPlayerPrefab(defence);
            return playerPrefab;
        }
    }
}