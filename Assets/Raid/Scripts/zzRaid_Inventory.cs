using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class zzRaid_Inventory : MonoBehaviour
{
    public zzRaid_Slot[,] Inventory = new zzRaid_Slot[4, 6];

    [SerializeField, Header("Loot manager")]
    private zzRaid_LootManagement zzraid_LootManagement;

    [SerializeField, Header("Time manager")]
    private Raid_Timer raid_Timer;
    private Transform _transform;

    [SerializeField, Header("Sprites")]
    public Sprite EmptySlot;

    /*[SerializeField, Header("Reference GameObjects")]
    public Transform PanelParent;

    [SerializeField, Header("Inventory Content")]
    public int AmountOfFives;
    public int AmountOfTens;
    public int AmountOfFifteens;
    public int AmountOfTwentiens;
    public int AmountOfTwentyfives;*/

    private void Start()
    {
        /*for (int i = 0; i < AmountOfFives; i++)
        {
            PlaceFives();
        }
        for (int i = 0; i < AmountOfTens; i++)
        {
            PlaceTens();
        }
        for (int i = 0; i < AmountOfFifteens; i++)
        {
            PlaceFifteens();
        }
        for (int i = 0; i < AmountOfTwentiens; i++)
        {
            PlaceTwenties();
        }
        for (int i = 0; i < AmountOfTwentyfives; i++)
        {
            PlaceTwentyFives();
        }

        PlaceEmptySlots();*/
    }

    public void QuickTapPerformed(Vector2 pointerPosition)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(pointerPosition);

        int x = Mathf.RoundToInt(mousePosition.x);
        int y = Mathf.RoundToInt(mousePosition.y);

        zzRaid_Slot zzraid_Slot = Inventory[x, y];
        Debug.Log("Multi tap recognized at (" + x + ", " + y + ")");
        if (raid_Timer.CurrentTime <= 0 || zzraid_LootManagement.CurrentLootWeight > zzraid_LootManagement.WeightLimit)
        {
            return;
        }
        else if (raid_Timer.CurrentTime > 0)
        {
            if (zzraid_Slot.slotType == zzRaid_Slot.SlotType.Empty)
            {
                return;
            }
            else if (zzraid_Slot.slotType == zzRaid_Slot.SlotType.Occupied)
            {
                LootFurniture(x, y);
            }
        }
    }

    /*void PlaceEmptySlots()
    {
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (Inventory[x, y] == null)
                {
                    zzRaid_Slot EmptySlot = Instantiate(Resources.Load("Prefabs/EmptySlot", typeof(zzRaid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as zzRaid_Slot;
                    Inventory[x, y] = EmptySlot;
                    //EmptySlot.transform.SetParent(PanelParent);
                }
            }
        }
    }

    void PlaceFives()
    {
        int x = Random.Range(0, 4);
        int y = Random.Range(0, 6);

        if (Inventory[x, y] == null)
        {
            zzRaid_Slot WeightFive = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(zzRaid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as zzRaid_Slot;
            WeightFive.GetComponentInChildren<TextMeshPro>().text = "5 kg";
            WeightFive.slotWeight = zzRaid_Slot.SlotWeight.Five;
            //WeightFive.transform.SetParent(PanelParent);

            Inventory[x, y] = WeightFive;
            Debug.Log("Inventory slot (" + x + ", " + y + ") is empty.");
        }
        else
        {
            PlaceFives();
        }
    }

    void PlaceTens()
    {
        int x = Random.Range(0, 4);
        int y = Random.Range(0, 6);

        if (Inventory[x, y] == null)
        {
            zzRaid_Slot WeightTen = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(zzRaid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as zzRaid_Slot;
            WeightTen.GetComponentInChildren<TextMeshPro>().text = "10 kg";
            WeightTen.slotWeight = zzRaid_Slot.SlotWeight.Ten;
            //WeightTen.transform.SetParent(PanelParent);

            Inventory[x, y] = WeightTen;
            Debug.Log("Inventory slot (" + x + ", " + y + ") is empty.");
        }
        else
        {
            PlaceTens();
        }
    }

    void PlaceFifteens()
    {
        int x = Random.Range(0, 4);
        int y = Random.Range(0, 6);

        if (Inventory[x, y] == null)
        {
            zzRaid_Slot WeightFifteen = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(zzRaid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as zzRaid_Slot;
            WeightFifteen.GetComponentInChildren<TextMeshPro>().text = "15 kg";
            WeightFifteen.slotWeight = zzRaid_Slot.SlotWeight.Fifteen;
            //WeightFifteen.transform.SetParent(PanelParent);

            Inventory[x, y] = WeightFifteen;
            Debug.Log("Inventory slot (" + x + ", " + y + ") is empty.");
        }
        else
        {
            PlaceFifteens();
        }
    }

    void PlaceTwenties()
    {
        int x = Random.Range(0, 4);
        int y = Random.Range(0, 6);

        if (Inventory[x, y] == null)
        {
            zzRaid_Slot WeightTwenty = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(zzRaid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as zzRaid_Slot;
            WeightTwenty.GetComponentInChildren<TextMeshPro>().text = "20 kg";
            WeightTwenty.slotWeight = zzRaid_Slot.SlotWeight.Twenty;
            //WeightTwenty.transform.SetParent(PanelParent);

            Inventory[x, y] = WeightTwenty;
            Debug.Log("Inventory slot (" + x + ", " + y + ") is empty.");
        }
        else
        {
            PlaceTwenties();
        }
    }

    void PlaceTwentyFives()
    {
        int x = Random.Range(0, 4);
        int y = Random.Range(0, 6);

        if (Inventory[x, y] == null)
        {
            zzRaid_Slot WeightTwentyFive = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(zzRaid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as zzRaid_Slot;
            WeightTwentyFive.GetComponentInChildren<TextMeshPro>().text = "25 kg";
            WeightTwentyFive.slotWeight = zzRaid_Slot.SlotWeight.TwentyFive;
            //WeightTwentyFive.transform.SetParent(PanelParent);

            Inventory[x, y] = WeightTwentyFive;
            Debug.Log("Inventory slot (" + x + ", " + y + ") is empty.");
        }
        else
        {
            PlaceTwentyFives();
        }
    }*/

    void LootFurniture(int x, int y)
    {
        zzRaid_Slot zzraid_Slot = Inventory[x, y];

        if (zzraid_Slot.slotWeight == zzRaid_Slot.SlotWeight.Five)
        {
            zzraid_Slot.GetComponent<SpriteRenderer>().sprite = EmptySlot;
            zzraid_Slot.GetComponentInChildren<TextMeshPro>().text = "";

            zzraid_LootManagement.CurrentLootWeight += 5;
            zzraid_LootManagement.SetLootWeightText();
        }
        else if (zzraid_Slot.slotWeight == zzRaid_Slot.SlotWeight.Ten)
        {
            zzraid_Slot.GetComponent<SpriteRenderer>().sprite = EmptySlot;
            zzraid_Slot.GetComponentInChildren<TextMeshPro>().text = "";

            zzraid_LootManagement.CurrentLootWeight += 10;
            zzraid_LootManagement.SetLootWeightText();
        }
        else if (zzraid_Slot.slotWeight == zzRaid_Slot.SlotWeight.Fifteen)
        {
            zzraid_Slot.GetComponent<SpriteRenderer>().sprite = EmptySlot;
            zzraid_Slot.GetComponentInChildren<TextMeshPro>().text = "";

            zzraid_LootManagement.CurrentLootWeight += 15;
            zzraid_LootManagement.SetLootWeightText();
        }
        else if (zzraid_Slot.slotWeight == zzRaid_Slot.SlotWeight.Twenty)
        {
            zzraid_Slot.GetComponent<SpriteRenderer>().sprite = EmptySlot;
            zzraid_Slot.GetComponentInChildren<TextMeshPro>().text = "";

            zzraid_LootManagement.CurrentLootWeight += 20;
            zzraid_LootManagement.SetLootWeightText();
        }
        else if (zzraid_Slot.slotWeight == zzRaid_Slot.SlotWeight.TwentyFive)
        {
            zzraid_Slot.GetComponent<SpriteRenderer>().sprite = EmptySlot;
            zzraid_Slot.GetComponentInChildren<TextMeshPro>().text = "";

            zzraid_LootManagement.CurrentLootWeight += 25;
            zzraid_LootManagement.SetLootWeightText();
        }

        zzraid_Slot.slotType = zzRaid_Slot.SlotType.Empty;

        if (zzraid_LootManagement.CurrentLootWeight > zzraid_LootManagement.WeightLimit)
        {
            zzraid_LootManagement.LootWeightText.color = Color.red;
            // RedScreen.SetActive(true);
            // EndMenu.SetActive(true);
        }
    }
}
