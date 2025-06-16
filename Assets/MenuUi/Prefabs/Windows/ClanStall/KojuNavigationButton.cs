using Altzone.Scripts.Window;
using UnityEngine;

public class KojuNavigationButton : MonoBehaviour
{
    private int windowIdToOpen = 1; // This represents the ID for Koju, change this if the ID for koju is changed

    public void OnClickNavigateToWindow()
    {
        DataCarrier.AddData(DataCarrier.RequestedWindow, windowIdToOpen);
    }
}
