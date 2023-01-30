using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GameObject[] slots;

    [Header("Control")]
    [SerializeField] InputActionReference press;

    private List<GameObject> invStored;

    [Header("Test purpose")]
    [SerializeField] private List<GameObject> testStored; // Used to test that the images work correctly, the actual inventory will be taken from elsewhere
    private void Start()
    {
        invStored = testStored;

        SortStored(); // Sorts the invStored by name

        FillSlots(); // Fills in the images to the UI
    }

    private void Update()
    {
        Debug.Log("" + press.action.ReadValue<Vector2>());
        //transform.localPosition += new Vector3(0, difBetween(transform.position.y, press.action.ReadValue<Vector2>().x), 0);
    }

    private void SortStored()
    {
        invStored.OrderBy(x => x.name);
    }

    public void FillSlots()
    {
        int i = 0;
        foreach (GameObject _slot in slots)
        {
            try
            {
                GameObject slotImage = _slot.transform.Find("Image").gameObject;
                SpriteRenderer furnitureImage = invStored[i].GetComponent<SpriteRenderer>();

                slotImage.SetActive(true);

                slotImage.GetComponent<Image>().sprite = furnitureImage.sprite;
                slotImage.GetComponent<Image>().color = furnitureImage.color;
                i++;
            }
            catch {  break; /* Either all the slots are filled or it had problems doing so */}
        }
    }

    private float difBetween(float a, float b)
    {
        // Returns the difference between float a and float b
        return a - b;
    }

    // Task List
    // - Visible Inventory (Done)
    // - Sorting (Done)
    // - Scroll functionality 
}
