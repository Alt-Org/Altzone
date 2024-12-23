using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogOutHandler : MonoBehaviour
{
    public void LogOut()
    {
        ServerManager.Instance.LogOut();
    }
}
