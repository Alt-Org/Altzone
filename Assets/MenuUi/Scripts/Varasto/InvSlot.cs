using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;

public class InvSlot : MonoBehaviour
{
    public GameFurniture contains;

    private void Start()
    {
        // Icon - Not done
        //Image slotIcon = transform.GetChild(0).GetComponent<Image>();

        // Name
        transform.GetChild(1).GetComponent<TMP_Text>().text = contains.Name;

        // Weight
        transform.GetChild(2).GetComponent<TMP_Text>().text = contains.Weight + "KG";

        // Shape - Not done
        //transform.GetChild(3).GetComponent<Image>().sprite = 
    }
}
