using UnityEngine;

public class KojuContentNotifier : MonoBehaviour
{
    public delegate void ActiveStateChanged(bool isActive);
    public static event ActiveStateChanged OnActiveStateChanged;

    // When enabled, change status to true and inform VarastoVisibilityManager
    private void OnEnable()
    {
        OnActiveStateChanged?.Invoke(true);
    }

    // When enabled, change status to false and inform VarastoVisibilityManager
    private void OnDisable()
    {
        OnActiveStateChanged?.Invoke(false);
    }
}
