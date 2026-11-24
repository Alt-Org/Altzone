using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseBox : MonoBehaviour
{
    public Transform ChooseBorder;
    public Transform ChooseFont;
    public Transform ChooseIcon;
    public Button buttonBorder;
    public Button buttonFont;
    public Button buttonIcon;

    void Start()
    {
        buttonBorder.onClick.AddListener(() => BringToFront(ChooseBorder));
        buttonFont.onClick.AddListener(() => BringToFront(ChooseFont)); // Uusi muokkaus (Perttu)
        buttonIcon.onClick.AddListener(() => BringToFront(ChooseIcon)); // Uusi muokkaus (Perttu)
    }

    void BringToFront(Transform folder)
    {
        folder.SetAsLastSibling();
    }
}
