using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSortHandler : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;
    [SerializeField] private GameObject _notInUsePanel;

    public Toggle Toggle { get => _toggle; }
    public GameObject NotInUsePanel { get => _notInUsePanel; }
}
