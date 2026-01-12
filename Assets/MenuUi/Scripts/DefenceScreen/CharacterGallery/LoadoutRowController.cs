using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ModelV2;
using MenuUi.Scripts.Lobby;
using MenuUi.Scripts.Lobby.SelectedCharacters;
using MenuUi.Scripts.Signals;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Context for the popup: which row is active for live preview (-1 = none)
/// </summary>
public static class LoadoutPopupContext
{
    public static int ActiveRowIndex = -1;

    public static int SelectedPopupIndex = -1;
}

public class LoadoutRowController : AltMonoBehaviour
{
    [Header("Loadout slot index for this row (1..8)")]
    [SerializeField] private int _loadoutIndex = 1;
    public int LoadoutIndex => _loadoutIndex;


    [Header("Row background for selection highlight")]
    [SerializeField] private Image _rowBackground;
    [SerializeField] private Color _normalColor = Color.blue;
    [SerializeField] private Color _selectedColor = Color.yellow;


    [SerializeField]
    private BattlePopupSelectedCharacter[] _slotViews = new BattlePopupSelectedCharacter[3];

    LoadoutSaveConfirmPopup _saveConfirmPopup;


    private void Awake()
    {

        _saveConfirmPopup = FindObjectOfType<LoadoutSaveConfirmPopup>(true);

        if (_saveConfirmPopup == null)
        {
            Debug.LogError("LoadoutRowController: SaveConfirmPopup not found in scene!");
        }

        for (int i = 0; i < _slotViews.Length; i++)
        {
            BattlePopupSelectedCharacter view = _slotViews[i];
            if (view == null) continue;

            Button btn;
            if (view.ButtonComponent != null)
            {
                btn = view.ButtonComponent;
            }
            else
            {
                btn = view.GetComponent<Button>();
            }

            if (btn == null)
            {
                Debug.LogWarning("LoadoutRowController: Button not found on slot view index " + i);
                continue;
            }


            btn.onClick.RemoveAllListeners();
            //btn.onClick.AddListener(OnCharSlotClicked);


            btn.interactable = true;
            btn.enabled = true;
        }
    }

    private void OnEnable()
    {
        SignalBus.OnReloadCharacterGalleryRequested += Redraw;
        Redraw();
    }

    private void OnDisable()
    {
        SignalBus.OnReloadCharacterGalleryRequested -= Redraw;
    }

    /// <summary>
    /// Called when any character slot in this row is clicked,
    /// sets this row active and opens the edit panel
    /// </summary>
    //private void OnCharSlotClicked()
    //{
    //    LoadoutPopupContext.ActiveRowIndex = _loadoutIndex;
    //    SignalBus.OnDefenceGalleryEditPanelRequestedSignal();
    //}

    /// <summary>
    /// Called from the row background Button OnClick.
    /// Selects this loadout as the active team.
    /// </summary>
    public void OnRowBackgroundClicked()
    {
        SetLoadoutAsCurrentTeam();
    }

    /// <summary>
    /// Fetches PlayerData and draws either live preview (current team)
    /// or this row's saved loadout
    /// </summary>
    private void Redraw()
    {
        StartCoroutine(GetPlayerData(delegate (PlayerData player)
        {
            if (player == null) return;

            if (LoadoutPopupContext.ActiveRowIndex == _loadoutIndex)
            {
                DrawFromCurrentTeam(player);
            }
            else
            {
                DrawFromSavedLoadout(player);
            }

            UpdateSelectionVisual();
        }));
    }

    /// <summary>
    /// Draws PlayerData.SelectedCharacterIds (current team)
    /// </summary>
    private void DrawFromCurrentTeam(PlayerData player)
    {
        for (int i = 0; i < _slotViews.Length; i++)
        {
            BattlePopupSelectedCharacter view = _slotViews[i];
            if (view == null) continue;

            CustomCharacterListObject current = null;
            if (i < player.SelectedCharacterIds.Length)
            {
                current = player.SelectedCharacterIds[i];
            }

            UpdateSlotView(view, current, true);
        }
    }

    /// <summary>
    /// Draws PlayerData.PopupLoadOuts[_loadoutIndex-1] (saved row)
    /// </summary>
    private void DrawFromSavedLoadout(PlayerData player)
    {
        TeamLoadOut loadout = null;
        int index = _loadoutIndex - 1;
        
            if (player.LoadOuts != null &&
                index >= 0 &&
                index < player.LoadOuts.Length)
            {
                loadout = player.LoadOuts[index];
            }
        
        for (int i = 0; i < _slotViews.Length; i++)
        {
            BattlePopupSelectedCharacter view = _slotViews[i];
            if (view == null) continue;

            CustomCharacterListObject entry = null;
            if (loadout != null &&
                loadout.Slots != null &&
                i < loadout.Slots.Length)
            {
                entry = loadout.Slots[i];
            }

            UpdateSlotView(view, entry, true);
        }
    }

    /// <summary>
    /// Updates a single slot view based on a CustomCharacterListObject,
    /// and keeps character slots editable
    /// </summary>
    private void UpdateSlotView(BattlePopupSelectedCharacter view, CustomCharacterListObject data, bool isEditable)
    {
        if (data == null || data.CharacterID == CharacterID.None)
        {
            view.SetEmpty(isEditable);
            return;
        }

        PlayerCharacterPrototype proto =
            PlayerCharacterPrototypes.GetCharacter(((int)data.CharacterID).ToString());

        if (proto != null)
        {
            view.SetInfo(proto.GalleryHeadImage, data.CharacterID, isEditable);
        }
        else
        {
            view.SetEmpty(isEditable);
        }
    }

    /// <summary>
    /// Called from the Save button. Shows confirmation popup
    /// if one is assigned
    /// </summary>
    public void OnSaveButtonClicked()
    {

        //SetLoadoutAsCurrentTeam();

        StartCoroutine(GetPlayerData(player =>
        {
            if (player == null) return;


            player.ApplyLoadout(_loadoutIndex);

            LoadoutPopupContext.ActiveRowIndex = _loadoutIndex;
            LoadoutPopupContext.SelectedPopupIndex = _loadoutIndex;

            Storefront.Get().SavePlayerData(player, null);

           
            LoadoutPopupContext.ActiveRowIndex = _loadoutIndex;

            
            SignalBus.OnDefenceGalleryEditPanelRequestedSignal();
            SignalBus.OnReloadCharacterGalleryRequestedSignal();
        }));


       
    }

    /// <summary>
    /// Saves current team into this row's saved slot and exits "live" preview
    /// </summary>
    public void SaveCurrentTeamIntoThisLoadout()
    {
        StartCoroutine(GetPlayerData(delegate (PlayerData player)
        {
            if (player == null) return;


            player.SaveCurrentTeamToLoadout(_loadoutIndex);

            Storefront.Get().SavePlayerData(player, null);

            if (LoadoutPopupContext.ActiveRowIndex == _loadoutIndex)
            {
                LoadoutPopupContext.ActiveRowIndex = -1;
            }

            SignalBus.OnReloadCharacterGalleryRequestedSignal();
            Redraw();
        }
            ));
    }

    /// <summary>
    /// Copies this row's saved loadout into SelectedCharacterIds (current team)
    /// and marks this row as the selected popup loadout.
    /// </summary>
    private void SetLoadoutAsCurrentTeam()
    {
        StartCoroutine(GetPlayerData(delegate (PlayerData player)
        {
            if (player == null) return;


            player.ApplyLoadout(_loadoutIndex);

            LoadoutPopupContext.SelectedPopupIndex = _loadoutIndex;
            LoadoutPopupContext.ActiveRowIndex = _loadoutIndex;
            Storefront.Get().SavePlayerData(player, null);

            SignalBus.OnReloadCharacterGalleryRequestedSignal();
            return;
            
        }));
    }


    /// <summary>
    /// Updates the row background color based on whether this row is selected.
    /// </summary>
    private void UpdateSelectionVisual()
    {
        if (_rowBackground == null) return;

        if (LoadoutPopupContext.SelectedPopupIndex == _loadoutIndex)
        {
            _rowBackground.color = _selectedColor;
        }
        else
        {
            _rowBackground.color = _normalColor;
        }
    }
}



