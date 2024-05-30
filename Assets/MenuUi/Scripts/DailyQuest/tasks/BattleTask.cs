using UnityEngine;
public class ProgressPoint
{
    private bool isActivated = false;
    public bool IsActivated => isActivated;

    public void Activate()
    {
        if (!isActivated)
        {
            isActivated = true;
            Debug.Log("+1");
        }
    }
}
