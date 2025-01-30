using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

namespace Battle1.Scripts.Battle.Game
{
    public class SpawnPlayers : MonoBehaviour
    {
        public GameObject playerPrefab;

        public float minX;
        public float maxX;
        public float minY;
        public float maxY;

        private void Start()
        {
            Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            /*PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);*/
        }
    }
}
