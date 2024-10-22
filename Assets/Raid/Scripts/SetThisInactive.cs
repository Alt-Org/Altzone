using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetThisInactive : MonoBehaviour
{
    public void TurnThisOff()
    {
        this.gameObject.SetActive(false);
    }
}
