using UnityEngine;

namespace MenuUi.Scripts.ChangeRegion
{
    public class ChangeRegionController : MonoBehaviour
    {
        [SerializeField] private ChangeRegionView _view;

        private void OnEnable()
        {
            _view.ResetView();
        }
    }
}