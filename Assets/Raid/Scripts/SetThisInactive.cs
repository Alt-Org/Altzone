using UnityEngine;

public class SetThisInactive : MonoBehaviour
{
    public void TurnThisOff()
    {
        gameObject.SetActive(false);
    }
}
