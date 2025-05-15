using UnityEngine;
using UnityEngine.UI;

public class KojuPopup : MonoBehaviour
{
    public Button confirmButton;
    public Button denyButton;

    private GameObject currentCard;
    private ItemMover currentItemMover;

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        denyButton.onClick.AddListener(OnDeny);  
    }

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
