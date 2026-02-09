using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LogOutHandler : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(LogOut);
    }

    public void LogOut()
    {
        Debug.Log("aaaaaaaaaaaaaa");
        ServerManager.Instance.LogOut();
    }
}
