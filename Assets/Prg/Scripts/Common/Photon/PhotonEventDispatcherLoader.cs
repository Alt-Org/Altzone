using Prg.Scripts.Common.Unity.ToastMessages;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Ensures that <c>PhotonEventDispatcher</c> is loaded for convenience.
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