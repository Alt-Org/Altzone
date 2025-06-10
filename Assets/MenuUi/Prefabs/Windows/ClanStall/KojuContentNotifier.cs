using UnityEngine;

public class KojuContentNotifier : MonoBehaviour
{
    public delegate void ActiveStateChanged(bool isActive);
    public static event ActiveStateChanged OnActiveStateChanged;

    private void OnEnable()
    {
        OnActiveStateChanged?.Invoke(true);
    }

    private void OnDisable()
    {
        OnActiveStateChanged?.Invoke(false);
    }
}
