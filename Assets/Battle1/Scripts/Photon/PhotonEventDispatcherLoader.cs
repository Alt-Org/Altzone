#if PHOTON_UNITY_NETWORKING
using Prg.Scripts.Common.Photon;
#endif
using UnityEngine;

namespace Prg.Scripts.Common.Unity.ToastMessages
{
    /// <summary>
    /// Helper to ensure that <c>PhotonEventDispatcher</c> is loaded for this level.
    /// </summary>
    public class PhotonEventDispatcherLoader : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private bool _isLoadScoreFlashNet;

#if PHOTON_UNITY_NETWORKING
        private void Awake()
        {
            PhotonEventDispatcher.Get();
            if (_isLoadScoreFlashNet)
            {
                ScoreFlashNet.RegisterEventListener();
            }
        }
#endif
    }
}