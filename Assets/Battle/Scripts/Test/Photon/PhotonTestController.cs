using Prg.Scripts.DevUtil;
using UnityEngine;

namespace Battle.Scripts.Test.Photon
{
    public class PhotonTestController : MonoBehaviour
    {
        [SerializeField] private PhotonTestView _view;

        private bool _isStarted;
        private FpsCounterLabel _fpsCounterLabel;

        private void OnEnable()
        {
            _view.ResetView();
            if (!_isStarted)
            {
                _isStarted = true;
                _fpsCounterLabel = GetComponentInParent<FpsCounterLabel>(true);
                if (_fpsCounterLabel != null)
                {
                    _view.FpsToggleButton.onClick.AddListener(OnFpsToggle);
                }
            }
        }

        private void OnFpsToggle()
        {
            _fpsCounterLabel.enabled = !_fpsCounterLabel.enabled;
        }
    }
}
