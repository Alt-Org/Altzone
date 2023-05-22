using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_References : MonoBehaviour
{
    [SerializeField, Header("Reference GameObjects")]
    public GameObject RedScreen;
    public GameObject EndMenu;

    private void Start()
    {
        RedScreen.SetActive(false);
        EndMenu.SetActive(false);
    }
}
