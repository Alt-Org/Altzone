using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Raid_References : MonoBehaviour
{
    [SerializeField, Header("Reference GameObjects")]
    public GameObject RedScreen;
    public GameObject EndMenu;

    [SerializeField, Header("Reference game components")]
    public TextMeshProUGUI OutOfTime;
    public TextMeshProUGUI OutOfSpace;
    public TextMeshProUGUI RaidEndedText;

    private void Start()
    {
        RedScreen.SetActive(false);
        EndMenu.SetActive(false);
        OutOfTime.enabled = false;
        OutOfSpace.enabled = false;
        RaidEndedText.enabled = false;
    }
}
