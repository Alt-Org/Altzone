using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURL : MonoBehaviour
{
    [SerializeField] private string _url = "https://altzone.fi/en/privacy" ;
    // Start is called before the first frame update

    public void OpenLink()
    {
        Application.OpenURL(_url);
    }
}
