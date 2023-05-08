using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raid_SlotRowGenerator : MonoBehaviour
{
    [SerializeField, Header("Reference GameObjects")]
    private GameObject SlotRowPrefab;
    public Transform PanelParent;

    [SerializeField, Header("Inventory Content")]
    private int AmountOfSlotRows;

    private void Start()
    {
        for (int i = 0; i < AmountOfSlotRows; i++)
        {
            PlaceSlotRow();
        }
    }

    void PlaceSlotRow()
    {
        GameObject SlotRow = Instantiate(SlotRowPrefab);
        SlotRow.transform.SetParent(PanelParent);
        SlotRow.transform.localScale = new Vector3(1, 1, 0);
    }
}
