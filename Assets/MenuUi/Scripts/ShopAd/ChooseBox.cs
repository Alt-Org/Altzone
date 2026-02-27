using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseBox : MonoBehaviour
{
    public Transform ChooseEffect;
    public Transform ChooseBorder;
    public Button buttonEffect;
    public Button buttonBorder;

    void Start()
    {
        buttonEffect.onClick.AddListener(() => BringToFront(ChooseEffect));
        buttonBorder.onClick.AddListener(() => BringToFront(ChooseBorder));
    }

    void BringToFront(Transform folder)
    {
        folder.SetAsLastSibling();
    }
}
