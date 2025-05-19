using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KojuPopup : MonoBehaviour
{
    public Button confirmButton;
    public Button denyButton;
    public TMP_InputField priceInputField;

    private GameObject currentCard;
    private ItemMover currentItemMover;
    private FurnitureGenerator furniture;

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        denyButton.onClick.AddListener(OnDeny);
    }

    //Called when clicking item on the tray. Opens the confirmation window by activating the Popup window
    public void Open(GameObject card)
    {
        currentCard = card;
        currentItemMover = card.GetComponent<ItemMover>();
        furniture = card.GetComponent<FurnitureGenerator>();

        if (furniture != null)
        {
            priceInputField.text = furniture.Value.ToString();
        }

        gameObject.SetActive(true);
    }

    private void OnConfirm()
    {
        if (furniture != null && int.TryParse(priceInputField.text, out int newValue))
        {
            furniture.SetValue(newValue);
        }

        currentItemMover?.ExecuteMove();
        Close();
    }

    private void OnDeny() => Close();

    private void Close()
    {
        gameObject.SetActive(false);
        currentCard = null;
        currentItemMover = null;
        furniture = null;
    }
}
