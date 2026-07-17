using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Raid_ExitConfirmationPopup : MonoBehaviour
{
    private const string PopupResourcePath = "Prefabs/RaidExitConfirmationPopup";

    private static Raid_ExitConfirmationPopup instance;
    private Action confirmAction;

    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private TextMeshProUGUI cancelButtonText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(Confirm);
            confirmButton.onClick.AddListener(Confirm);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(Hide);
            cancelButton.onClick.AddListener(Hide);
        }
    }

    public static void Show(Action onConfirm)
    {
        Raid_ExitConfirmationPopup popup = GetOrCreate();
        if (popup == null)
        {
            return;
        }

        popup.confirmAction = onConfirm;
        popup.gameObject.SetActive(true);
        popup.transform.SetAsLastSibling();
    }

    private static Raid_ExitConfirmationPopup GetOrCreate()
    {
        if (instance != null)
        {
            return instance;
        }

        Raid_ExitConfirmationPopup prefab = Resources.Load<Raid_ExitConfirmationPopup>(PopupResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"Raid exit confirmation popup prefab was not found at Resources/{PopupResourcePath}.");
            return null;
        }

        instance = Instantiate(prefab);
        instance.name = "Raid Exit Confirmation Popup";
        DontDestroyOnLoad(instance.gameObject);
        instance.gameObject.SetActive(false);
        return instance;
    }

    private void Confirm()
    {
        Action action = confirmAction;
        Hide();
        action?.Invoke();
    }

    private void Hide()
    {
        confirmAction = null;
        gameObject.SetActive(false);
    }
}
