using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KodinController : MonoBehaviour
{
    public static KodinController instance;

    [SerializeField] private ModalWindowPanel _modalWindow;

    public ModalWindowPanel modalWindow => _modalWindow;

    private void Awake()
    {
        instance = this;
    }
}
