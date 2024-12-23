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
        if (GameObject.FindWithTag("OverlayPanel").activeSelf)
        {
            return;
        }
        else _overlayObject.SetActive(true);

    }
}
