using System.Collections;
using Photon.Pun;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Instantiate given Photon Network prefab using <c>PhotonNetwork</c>.<c>Instantiate</c> when we join a room.
    /// </summary>
    public class PhotonPlayerAutoInstantiate : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private GameObject _networkPrefab;

        private IEnumerator Start()
        {
            Debug.Log($"wait for room {PhotonNetwork.NetworkClientState}");
            while (enabled)
            {
                if (PhotonNetwork.InRoom)
                {
                    break;
                }
                yield return null;
            }
            if (!PhotonNetwork.InRoom)
            {
                yield break;
            }
            Debug.Log($"PhotonNetwork.Instantiate {_networkPrefab.name} {PhotonNetwork.NetworkClientState}", _networkPrefab);
            PhotonNetwork.Instantiate(_networkPrefab.name, Vector3.zero, Quaternion.identity);
        }
    }
}
