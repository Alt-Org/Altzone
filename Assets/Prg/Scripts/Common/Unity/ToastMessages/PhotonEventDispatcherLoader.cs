using Prg.Scripts.Common.Photon;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.ToastMessages
{
    /// <summary>
    /// Helper to ensure that <c>PhotonEventDispatcher</c> is loaded for this level.
    /// </summary>
    public class PhotonEventDispatcherLoader : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private bool _isLoadScoreFlashNet;

        private void Awake()
        {
            PhotonEventDispatcher.Get();
            if (_isLoadScoreFlashNet)
            {
                ScoreFlashNet.RegisterEventListener();
            }
        }
    }
}