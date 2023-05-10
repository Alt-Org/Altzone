using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.Test.Photon
{
    public class PhotonTestView : MonoBehaviour
    {
        [SerializeField] private Button _fpsToggleButton;

        public Button FpsToggleButton => _fpsToggleButton;

        public void ResetView()
        {
        }
    }
}
