using UnityEngine;

namespace Altzone.Scripts.Window
{
    public class DisableWindowManager : MonoBehaviour
    {
        private void Awake()
        {
            var handler = FindObjectOfType<EscapeKeyHandler>();
            if (handler != null)
            {
                Debug.LogWarning("EscapeKeyHandler is DISABLED for now!");
                handler.enabled = false;
            }
        }
    }
}