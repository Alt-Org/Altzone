using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroWindow : MonoBehaviour
{
    public string title;
    public Sprite sprite;
    public string message;
    public bool triggerOnEnable;

    public void OnEnable()
    {
        if (!triggerOnEnable) { return; }
        KodinController.instance.modalWindow.ShowAsHero(title, sprite, message, null, null, null);
    }
}
