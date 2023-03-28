using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class Raid_Inventory : MonoBehaviour
{
    public Raid_Slot[,] Inventory = new Raid_Slot[4, 6];

    private Transform _transform;

    [Header("Inventory Content")]
    public int AmountOfFives;
    public int AmountOfTens;
    public int AmountOfFifteens;
    public int AmountOfTwentiens;
    public int AmountOfTwentyfives;

    private void Start()
    {
        for (int i = 0; i < AmountOfFives; i++)
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

        PlaceEmptySlots();
    }

    public void DoupleTapPerformed(Vector2 pointerPosition)
    {

    }
    void PlaceEmptySlots()
    {
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (Inventory[x, y] == null)
                {
                    Raid_Slot EmptySlot = Instantiate(Resources.Load("Prefabs/EmptySlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
                    Inventory[x, y] = EmptySlot;
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
            Raid_Slot WeightFive = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
            WeightFive.GetComponentInChildren<TextMeshPro>().text = "5 kg";

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
            Raid_Slot WeightTen = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
            WeightTen.GetComponentInChildren<TextMeshPro>().text = "10 kg";

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
            Raid_Slot WeightFifteen = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
            WeightFifteen.GetComponentInChildren<TextMeshPro>().text = "15 kg";

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
            Raid_Slot WeightTwenty = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
            WeightTwenty.GetComponentInChildren<TextMeshPro>().text = "20 kg";

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
            Raid_Slot WeightTwentyFive = Instantiate(Resources.Load("Prefabs/FurnitureSlot", typeof(Raid_Slot)), new Vector3(x, y, 0), Quaternion.identity, _transform) as Raid_Slot;
            WeightTwentyFive.GetComponentInChildren<TextMeshPro>().text = "25 kg";

            Inventory[x, y] = WeightTwentyFive;
            Debug.Log("Inventory slot (" + x + ", " + y + ") is empty.");
        }
        else
        {
            PlaceTwentyFives();
        }
    }
}
