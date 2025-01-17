using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayPanelCheck : MonoBehaviour
{
    [SerializeField] private GameObject _overlayObject;

    private void Awake()
    {
        if(_overlayObject == null) _overlayObject = transform.Find("UIOverlayPanel").GetComponent<GameObject>();
    }

    private void OnEnable()
    {
        if (GameObject.FindWithTag("OverlayPanel")?GameObject.FindWithTag("OverlayPanel").activeSelf: true) //If OverlayPanel cannot be found, return otherwise check if it is active, if not set the OverlayPanel tied to this active.
        {
            return;
        }
        else _overlayObject.SetActive(true);

    }
}
