using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Battle0.Scripts.Test
{
    internal class SimplePingHelper : MonoBehaviour
    {
        [SerializeField] private string _pingFormat = "ping\r\n{0}";
        [SerializeField] private TextMeshProUGUI _pingText;

        private void FixedUpdate()
        {
            if (Time.frameCount % 10 != 0)
            {
                return;
            }
            _pingText.text = string.Format(_pingFormat, PhotonNetwork.GetPing());
        }
    }
}