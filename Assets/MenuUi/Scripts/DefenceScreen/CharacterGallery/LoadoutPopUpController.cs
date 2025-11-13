using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LoadoutPopUpController : MonoBehaviour
{
    [SerializeField] GameObject swipeBlocker;

    void Awake() => gameObject.SetActive(false);

    public void Open()
    {
        gameObject.SetActive(true);
        if (swipeBlocker) swipeBlocker.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (swipeBlocker) swipeBlocker.SetActive(false);
    }
}

