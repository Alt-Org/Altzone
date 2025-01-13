#if false
using Battle1.PhotonUnityNetworking.Code;
using UnityEngine.SceneManagement;
using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;

namespace Battle1.Scripts.Test.Elmeri
{
    public class ConnectToServer : MonoBehaviourPunCallbacks
    {
        // Start is called before the first frame update
        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()  //public override void
        {
            PhotonNetwork.JoinLobby();
        }

        public override void OnJoinedLobby()    //public override void
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}
#endif
