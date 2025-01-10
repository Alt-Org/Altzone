using System.Collections;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Instantiate given Photon Network prefab using <c>PhotonNetwork</c>.<c>Instantiate</c> when we join a room.
    /// </summary>
    public class PhotonPlayerAutoInstantiate : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private GameObject _networkPrefab;
        [SerializeField] private bool _isAutoCreateRoom;

        private IEnumerator Start()
        {
           /* Debug.Log($"wait for room {PhotonNetwork.NetworkClientState}");
            if (!PhotonNetwork.InRoom && _isAutoCreateRoom)
            {
                // Create a room for us
                gameObject.AddComponent<PhotonRoomAutoCreate>();
            }
            // Wait for a room.
            while (enabled)
            {
                if (PhotonNetwork.InRoom)
                {
                    break;
                }
                yield return null;
            }
            if (!enabled || !PhotonNetwork.InRoom)
            {
                // Disabled or not in a room - no can do!
                yield break;
            }
            // Instantiate Photon player.
            Debug.Log($"PhotonNetwork.Instantiate {_networkPrefab.name} {PhotonNetwork.NetworkClientState}", _networkPrefab);
            PhotonNetwork.Instantiate(_networkPrefab.name, Vector3.zero, Quaternion.identity);*/
           yield return null;
        }
    }
}
