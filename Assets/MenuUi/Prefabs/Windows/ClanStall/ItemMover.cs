using System;
using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Storage;
using Altzone.Scripts.Config;
using Altzone.Scripts;

public class ItemMover : MonoBehaviour
{
    private Transform trayParent;
    private Transform gridParent;
    private KojuItemSlot assignedSlot;

    private KojuPopup popup;

    private KojuItemSlot[] panelSlots;

    private KojuTrayPopulator trayPopulator;

    private StorageFurniture currentFurniture;

    // Event to notify that this item was moved to panel
    public event Action<StorageFurniture> OnItemMovedToPanel;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetParents(Transform tray, Transform panel)
    {
        trayParent = tray;
        gridParent = panel;

        // Cache KojuItemSlot components in the panel to optimize slot lookup
        panelSlots = gridParent.GetComponentsInChildren<KojuItemSlot>(true);
    }

    public void SetPopup(KojuPopup popupRef)
    {
        popup = popupRef;
    }

    public void SetPopulator(KojuTrayPopulator populator)
    {
        trayPopulator = populator;
    }

    public void SetFurniture(StorageFurniture furniture)
    {
        currentFurniture = furniture;
    }

    // Call when clicking a card in the panel or the tray, see KojuPopup.cs
    private void OnClick()
    {
        if (assignedSlot != null)
        {
            popup?.OpenRemovePopup(gameObject);
        }
        else if (HasFreeSlot())
        {
            popup?.Open(gameObject);
        }
        else
        {
            trayPopulator?.ShowPanelFullWarning();
        }
    }

    private bool HasFreeSlot()
    {
        foreach (var slot in panelSlots)
        {
            if (!slot.IsOccupied)
            {
                return true;
            }
        }
        return false;
    }

    // Call when user confirms the moving of a furniture
    public void ExecuteMove()
    {
        if (assignedSlot == null)
        {
            // Move from tray to panel
            foreach (var slot in panelSlots)
            {
                if (slot.transform.GetSiblingIndex() == 0) continue; // Skips the slot meant for the poster card

                if (!slot.IsOccupied)
                {
                    assignedSlot = slot;
                    assignedSlot.AssignCard(gameObject);
                    transform.SetSiblingIndex(assignedSlot.transform.GetSiblingIndex());

                    // Notify trayPopulator this item was moved
                    OnItemMovedToPanel?.Invoke(currentFurniture);
                    return;
                }
            }

            Debug.LogWarning("No available slot found");
        }
        else
        {
            // Move from panel back to tray
            transform.SetParent(trayParent, false);

            var furnitureData = GetComponent<KojuFurnitureData>();
            if (furnitureData != null)
            {
                furnitureData.ResetPrice();

                // Clear slot assignment
                assignedSlot.ClearSlot();
                assignedSlot = null;

                // Notify trayPopulator this item was returned
                trayPopulator?.HandleItemReturnedToTray(currentFurniture);

                var store = Storefront.Get();
                store.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, player =>
                {
                    if (player != null && !string.IsNullOrEmpty(player.ClanId))
                    {
                        store.GetClanData(player.ClanId, clan =>
                        {
                            if (clan != null)
                            {
                                store.SaveClanData(clan, saved =>
                                {
                                    Debug.Log("Clan data saved after moving item back to tray.");
                                });
                            }
                            else
                            {
                                Debug.LogWarning("Clan data not found for saving.");
                            }
                        });
                    }
                    else
                    {
                        Debug.LogWarning("Player data or ClanId missing for saving clan data.");
                    }
                });
            }
        }
    }

}
