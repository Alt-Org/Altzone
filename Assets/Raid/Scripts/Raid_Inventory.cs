using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class Raid_Inventory : MonoBehaviour
{
    public Raid_Slot[,] Inventory = new Raid_Slot[4, 12];

    [SerializeField, Header("Loot manager")]
    private Raid_LootManagement raid_LootManagement;

    [SerializeField, Header("Time manager")]
    private Raid_Timer raid_Timer;

    private Transform _transform;

    [SerializeField, Header("Sprites")]
    public Sprite EmptySlot;

    [SerializeField, Header("Reference GameObjects")]
    public GameObject RedScreen;
    public GameObject EndMenu;
    //public Transform PanelParent;

    /*[SerializeField, Header("Inventory Content")]
    public int AmountOfFives;
    public int AmountOfTens;
    public int AmountOfFifteens;
    public int AmountOfTwentiens;
    public int AmountOfTwentyfives;*/

    private void Start()
    {
        RedScreen.SetActive(false);
        EndMenu.SetActive(false);
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

        Raid_Slot raid_Slot = Inventory[x, y];
        Debug.Log("Multi tap recognized at (" + x + ", " + y + ")");
        if(raid_Timer.CurrentTime <= 0 || raid_LootManagement.CurrentLootWeight > raid_LootManagement.WeightLimit)
        {
            return;
        }
        else if (raid_Timer.CurrentTime > 0)
        {
            if (raid_Slot.slotType == Raid_Slot.SlotType.Empty)
            {
                return;
            }
            else if (raid_Slot.slotType == Raid_Slot.SlotType.Occupied)
            {
                LootFurniture(x, y);
            }
        }
    }

    /*void PlaceEmptySlots()
    {
        for (int y = 0; y < 12; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (Inventory[x, y] == null)
                {
                    Raid_Slot EmptySlot = Instantiate(Resources.Load("Prefabs/EmptySlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
                    Inventory[x, y] = EmptySlot;
                    EmptySlot.transform.SetParent(PanelParent);
                }
            }
        }
    }

    void PlaceFives()
    {
        int x = Random.Range(0, 4);
        int y = Random.Range(0, 12);

        if (Inventory[x, y] == null)
        {
            Raid_Slot WeightFive = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
            WeightFive.GetComponentInChildren<TextMeshPro>().text = "5 kg";
            WeightFive.slotWeight = Raid_Slot.SlotWeight.Five;
            WeightFive.transform.SetParent(PanelParent);

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
        int y = Random.Range(0, 12);

        if (Inventory[x, y] == null)
        {
            Raid_Slot WeightTen = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
            WeightTen.GetComponentInChildren<TextMeshPro>().text = "10 kg";
            WeightTen.slotWeight = Raid_Slot.SlotWeight.Ten;
            WeightTen.transform.SetParent(PanelParent);

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
        int y = Random.Range(0, 12);

        if (Inventory[x, y] == null)
        {
            Raid_Slot WeightFifteen = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
            WeightFifteen.GetComponentInChildren<TextMeshPro>().text = "15 kg";
            WeightFifteen.slotWeight = Raid_Slot.SlotWeight.Fifteen;
            WeightFifteen.transform.SetParent(PanelParent);

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
        int y = Random.Range(0, 12);

        if (Inventory[x, y] == null)
        {
            Raid_Slot WeightTwenty = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
            WeightTwenty.GetComponentInChildren<TextMeshPro>().text = "20 kg";
            WeightTwenty.slotWeight = Raid_Slot.SlotWeight.Twenty;
            WeightTwenty.transform.SetParent(PanelParent);

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
        int y = Random.Range(0, 12);

        if (Inventory[x, y] == null)
        {
            Raid_Slot WeightTwentyFive = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
            WeightTwentyFive.GetComponentInChildren<TextMeshPro>().text = "25 kg";
            WeightTwentyFive.slotWeight = Raid_Slot.SlotWeight.TwentyFive;
            WeightTwentyFive.transform.SetParent(PanelParent);

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
        Raid_Slot raid_Slot = Inventory[x, y];

        if (raid_Slot.slotWeight == Raid_Slot.SlotWeight.Five)
        {
            raid_Slot.GetComponent<SpriteRenderer>().sprite = EmptySlot;
            raid_Slot.GetComponentInChildren<TextMeshPro>().text = "";

            raid_LootManagement.CurrentLootWeight += 5;
            raid_LootManagement.SetLootWeightText();
        }
        else if (raid_Slot.slotWeight == Raid_Slot.SlotWeight.Ten)
        {
            raid_Slot.GetComponent<SpriteRenderer>().sprite = EmptySlot;
            raid_Slot.GetComponentInChildren<TextMeshPro>().text = "";

            raid_LootManagement.CurrentLootWeight += 10;
            raid_LootManagement.SetLootWeightText();
        }
        else if (raid_Slot.slotWeight == Raid_Slot.SlotWeight.Fifteen)
        {
            raid_Slot.GetComponent<SpriteRenderer>().sprite = EmptySlot;
            raid_Slot.GetComponentInChildren<TextMeshPro>().text = "";

            raid_LootManagement.CurrentLootWeight += 15;
            raid_LootManagement.SetLootWeightText();
        }
        else if (raid_Slot.slotWeight == Raid_Slot.SlotWeight.Twenty)
        {
            raid_Slot.GetComponent<SpriteRenderer>().sprite = EmptySlot;
            raid_Slot.GetComponentInChildren<TextMeshPro>().text = "";

            raid_LootManagement.CurrentLootWeight += 20;
            raid_LootManagement.SetLootWeightText();
        }
        else if (raid_Slot.slotWeight == Raid_Slot.SlotWeight.TwentyFive)
        {
            raid_Slot.GetComponent<SpriteRenderer>().sprite = EmptySlot;
            raid_Slot.GetComponentInChildren<TextMeshPro>().text = "";

            raid_LootManagement.CurrentLootWeight += 25;
            raid_LootManagement.SetLootWeightText();
        }

        raid_Slot.slotType = Raid_Slot.SlotType.Empty;

        if(raid_LootManagement.CurrentLootWeight > raid_LootManagement.WeightLimit)
        {
            raid_LootManagement.LootWeightText.color = Color.red;
            RedScreen.SetActive(true);
            EndMenu.SetActive(true);
        }
    }
}
