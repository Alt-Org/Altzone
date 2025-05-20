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

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        denyButton.onClick.AddListener(OnDeny);
    }

    // Called when clicking item on the tray. Opens the confirmation window by activating the Popup window
    public void Open(GameObject card)
    {
        currentCard = card;
        currentItemMover = card.GetComponent<ItemMover>();

        gameObject.SetActive(true);
    }

    private void OnConfirm()
    {
        currentItemMover?.ExecuteMove();
        Close();
    }

    private void OnDeny() => Close();

    private void Close()
    {
        gameObject.SetActive(false);
        currentCard = null;
        currentItemMover = null;
    }
}
