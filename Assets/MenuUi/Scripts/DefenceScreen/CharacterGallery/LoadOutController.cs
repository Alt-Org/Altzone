using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.CharacterGallery;
using MenuUi.Scripts.Signals;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadOutController : MonoBehaviour
{
    [SerializeField] private List<Button> _loadoutButtons;
    [SerializeField] private ModelController _modelController;
    [SerializeField] private GameObject _confirmPanel;
    [SerializeField] private TMP_Text _confirmText;
    [SerializeField] private Button _confirmYes;
    [SerializeField] private Button _confirmNo;


    [SerializeField] private ScrollRect _inlineScrollRect;

    //Loadout 2
    [SerializeField] private bool _useZeroSlotButton = true;

    [Header("Dynamic inline loadouts")]
    [SerializeField] private Button _loadoutButtonTemplate;
    [SerializeField] private Button _addLoadoutButton;
    [SerializeField] private Transform _buttonsParent;

    private PlayerData _player;

    private readonly List<ButtonLoadoutEntry> _buttonMap = new List<ButtonLoadoutEntry>();
    private readonly List<Button> _dynamicButtons = new List<Button>();


    private class ButtonLoadoutEntry
    {
        public Button Button;
        public int LoadoutIndex;
    }


    private bool IsInlineMode
    {
        get
        {
            return _loadoutButtonTemplate != null
                && _addLoadoutButton != null
                && _buttonsParent != null;
        }
    }


    private void Awake()
    {
        if (_modelController == null)
            _modelController = GetComponent<ModelController>();


        if (_modelController != null)
            _modelController.OnPlayerDataReady += HandlePlayerDataReady;
    }

    private void OnEnable()
    {
        if (!IsInlineMode) return;

        HookAddButton();

        if (_player != null)
        {
            BuildInlineButtons();
            RefreshButtons();
        }
    }
    private void Start()
    {

        if (IsInlineMode)
        {
            return;
        }

        for (int i = 0; i < _loadoutButtons.Count; i++)
        {
            int buttonIndex = i;
            if (_loadoutButtons[i] == null) continue;

            _loadoutButtons[i].onClick.RemoveAllListeners();
            _loadoutButtons[i].onClick.AddListener(() =>
            {
                int loadoutIndex = ButtonIndexToLoadoutIndex(buttonIndex);
                OnPressLoadout(loadoutIndex, _player);
            });
        }
    }
    private void OnDisable()
    {

        if (!IsInlineMode) return;

        CleanupInlineButtons();
    }
    private void HookAddButton()
    {
        if (_addLoadoutButton == null) return;

        _addLoadoutButton.onClick.RemoveAllListeners();
        _addLoadoutButton.onClick.AddListener(AddNewLoadoutSlot);
    }

    private void CleanupInlineButtons()
    {
        for (int i = 0; i < _dynamicButtons.Count; i++)
        {
            Button button = _dynamicButtons[i];
            if (button != null)
                Destroy(button.gameObject);
        }

        _dynamicButtons.Clear();
        _buttonMap.Clear();

    }

    private void HandlePlayerDataReady(PlayerData data)
    {
        _player = data;


        if (IsInlineMode)
        {
            HookAddButton();

            if (_buttonMap.Count == 0)
            {
                BuildInlineButtons(); 
            }
            else
            {
                RefreshInlineButtons();
                UpdateAddButtonVisibility();
            }
        }

        RefreshButtons();

    }

    //loadout 2

    /// <summary>
    /// Converts a UI button index to a loadout index used in PlayerData:
    /// -If _useZeroSlotButton is true, button index matches loadout index (0,1,2,...).
    /// -If _useZeroSlotButton is false, loadout indexes start from 1, so 0 -> 1, 1 -> 2, etc.
    /// </summary>
    private int ButtonIndexToLoadoutIndex(int buttonIndex)
    {
        if (_useZeroSlotButton)
        {
            return buttonIndex;
        }
        else
        {
            return buttonIndex + 1;
        }
    }

    /// <summary>
    /// Handles loadout button presses, applies or saves loadouts as needed
    /// </summary>
    private void OnPressLoadout(int loadoutIndex, PlayerData player)
    {

        Storefront.Get().GetPlayerData(ServerManager.Instance.Player.uniqueIdentifier, p =>
        {
            if (p == null)
            {
                Debug.LogError("Failed to retrieve PlayerData from Storefront");
                return;
            }

            _player = p;
            player = p;


            if (loadoutIndex == 0)
            {
                _player.ApplyLoadout(0);
                SignalBus.OnReloadCharacterGalleryRequestedSignal();
                RefreshButtons();
                Storefront.Get().SavePlayerData(player, null);
                return;
            }

            if (loadoutIndex < 0 || loadoutIndex > player.LoadOuts.Length)
            {
                Debug.LogError($"Invalid loadout index {loadoutIndex}. Player has {player.LoadOuts.Length} loadouts.");
                return;
            }

            TeamLoadOut slot = _player.LoadOuts[loadoutIndex - 1];
            bool isEmpty = (slot == null) || slot.IsEmpty;

            if (isEmpty)
            {
                if (IsInlineMode)
                {

                    player.ApplyLoadout(loadoutIndex);
                    SignalBus.OnReloadCharacterGalleryRequestedSignal();
                    RefreshButtons();
                    Storefront.Get().SavePlayerData(player, null);
                }
                else
                {

                    SaveToEmptySlot(loadoutIndex, player);
                }

                return;
            }

            player.ApplyLoadout(loadoutIndex);
            SignalBus.OnReloadCharacterGalleryRequestedSignal();
            RefreshButtons();
            Storefront.Get().SavePlayerData(player, null);
        });
    }
    /// <summary>
    /// Saves the current team to an empty loadout slot, optionally asking for confirmation
    /// </summary>
    private void SaveToEmptySlot(int loadoutIndex, PlayerData player)
    {

        if (_confirmPanel == null || _confirmYes == null || _confirmNo == null)
        {

            player.SaveCurrentTeamToLoadout(loadoutIndex);
            player.ApplyLoadout(loadoutIndex);
            SignalBus.OnReloadCharacterGalleryRequestedSignal();
            RefreshButtons();
            Storefront.Get().SavePlayerData(player, null);
            return;
        }

        _confirmPanel.SetActive(true);
        if (_confirmText != null)
            _confirmText.text = $"Tallennetaanko nykyinen tiimi slottiin {loadoutIndex}?";


        _confirmYes.onClick.RemoveAllListeners();
        _confirmNo.onClick.RemoveAllListeners();

        _confirmYes.onClick.AddListener(() =>
        {

            player.SaveCurrentTeamToLoadout(loadoutIndex);
            player.ApplyLoadout(loadoutIndex);
            SignalBus.OnReloadCharacterGalleryRequestedSignal();
            RefreshButtons();
            Storefront.Get().SavePlayerData(player, null);

            _confirmPanel.SetActive(false);
        });

        _confirmNo.onClick.AddListener(() =>
        {

            _confirmPanel.SetActive(false);
        });
    }

    private void RefreshButtons()
    {
        if (_player == null) return;

        if (IsInlineMode)
        {
            RefreshInlineButtons();
            UpdateAddButtonVisibility();
        }
        else
        {
            RefreshPopupButtons();
        }

    }

    private void RefreshPopupButtons()
    {
        int selectedLoadout = _player.SelectedLoadOut;

        for (int i = 0; i < _loadoutButtons.Count; i++)
        {
            Button btn = _loadoutButtons[i];
            if (btn == null) continue;

            int loadoutIndex = ButtonIndexToLoadoutIndex(i);
            bool isSelected = (selectedLoadout == loadoutIndex);

            bool shouldDisable;

            if (_useZeroSlotButton)
            {
                if (isSelected && loadoutIndex != 0)
                {
                    shouldDisable = true;
                }
                else
                {
                    shouldDisable = false;
                }
            }
            else
            {
                shouldDisable = isSelected;
            }

            btn.interactable = !shouldDisable;
        }
    }

    private void RefreshInlineButtons()
    {
        int selectedLoadout = _player.SelectedLoadOut;

        for (int i = 0; i < _buttonMap.Count; i++)
        {
            ButtonLoadoutEntry entry = _buttonMap[i];
            if (entry != null && entry.Button != null)
            {
                entry.Button.interactable = (selectedLoadout != entry.LoadoutIndex);
            }


        }
    }


    /// <summary>
    /// -Adds a new loadout button before the "+" button, if a free slot exists in PlayerData
    /// -Currently limited by the length of PlayerData.LoadOuts (no more buttons can be added once the array is full)
    /// </summary>
    public void AddNewLoadoutSlot()
    {

        if (!IsInlineMode) return;

        if (_player == null)
        {
            Debug.LogWarning("AddNewLoadoutSlot called but _player is null");
            return;
        }
        // Cache the current horizontal scroll position so that
        // adding and reordering buttons does not cause the scroll view to jump
        ScrollRect scroll = _inlineScrollRect;

        if (scroll == null)
        {
            scroll = _buttonsParent.GetComponentInParent<ScrollRect>();
        }

        float saved = 0f;

        if (scroll != null)
        {
            saved = scroll.horizontalNormalizedPosition;
        }

        int next = FindNextHiddenEmptyLoadoutIndex();
        if (next == -1)
        {
            UpdateAddButtonVisibility();
            return;
        }

        CreateDynamicButton(next, false);
        ReorderInlineButtonsByLoadoutIndex();

        // Force an immediate layout rebuild so that the content size
        // is updated before restoring the scroll position
        Canvas.ForceUpdateCanvases();
        RectTransform contentRt = _buttonsParent as RectTransform;
        if (contentRt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(contentRt);
        Canvas.ForceUpdateCanvases();

        // Restore the scroll position so the row does not visually shift
        if (scroll != null) scroll.horizontalNormalizedPosition = saved;

        _player.ApplyLoadout(next);
        SignalBus.OnReloadCharacterGalleryRequestedSignal();
        Storefront.Get().SavePlayerData(_player, null);

        RefreshButtons();

    }
    /// <summary>
    /// Builds inline UI: Always registers buttons 1-3, and creates
    /// dynamic buttons for non-empty loadouts 4.... max (current max 8)
    /// </summary>
    private void BuildInlineButtons()
    {
        if (_player == null) return;

        CleanupInlineButtons();

        // Try to find a ScrollRect that controls this button row
        ScrollRect scroll = null;

        if (_inlineScrollRect != null)
        {
            scroll = _inlineScrollRect;
        }
        else
        {
            scroll = _buttonsParent.GetComponentInParent<ScrollRect>();
        }

        // Get the RectTransform of the content (buttons parent)
        RectTransform contentRt = _buttonsParent as RectTransform;

        // Save current positions so layout rebuild does not cause "jump"
        float savedHorizontalPosition = 0f;
     

        if (scroll != null)
        {
            savedHorizontalPosition = scroll.horizontalNormalizedPosition;
        }

 
        for (int i = 0; i < _loadoutButtons.Count; i++)
        {
            Button btn = _loadoutButtons[i];

            if (btn != null)
            {
                int loadoutIndex = ButtonIndexToLoadoutIndex(i);
                RegisterButton(btn, loadoutIndex);
            }

        }

        int max = _player.LoadOuts.Length;
        for (int loadoutIndex = 4; loadoutIndex <= max; loadoutIndex++)
        {
            TeamLoadOut slot = _player.LoadOuts[loadoutIndex - 1];
            bool isEmpty = (slot == null) || slot.IsEmpty;

            if (!isEmpty)
            {
                CreateDynamicButton(loadoutIndex, false);
            }


        }

        ReorderInlineButtonsByLoadoutIndex();
        UpdateAddButtonVisibility();



        Canvas.ForceUpdateCanvases();

        // If we have a content RectTransform, rebuild its layout immediately
        if (contentRt != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRt);
        }

        // Force UI update again after layout rebuild
        Canvas.ForceUpdateCanvases();

        // Restore the scroll position so there is no movement seen in the UI
        if (scroll != null)
        {
            scroll.horizontalNormalizedPosition = savedHorizontalPosition;
        }
    }

    /// <summary>
    /// Registers a loadout button: Sets the click action,
    /// updates the button label and stores the button and its loadout index
    /// for later use. Used for both fixed (1-3) and dynamically created buttons
    /// </summary>
    /// <param name="btn"> The button to register</param>
    /// <param name="loadoutIndex">Loadout number linked to the button</param>
    private void RegisterButton(Button btn, int loadoutIndex)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnPressLoadout(loadoutIndex, _player));

        ButtonLoadoutEntry entry = new ButtonLoadoutEntry();
        entry.Button = btn;
        entry.LoadoutIndex = loadoutIndex;
        _buttonMap.Add(entry);

        TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = loadoutIndex.ToString();
        }
    }

    /// <summary>
    /// Creates a new dynamic loadout button for loadouts 4 --->
    /// The button is created from a template, but if the button already exists,
    /// nothing is created
    /// </summary>
    /// <param name="loadoutIndex"> Loadout number to create a button for</param>
    /// <param name="reorderAfterCreate">If true, calls the reorder method after the button is created</param>
    private void CreateDynamicButton(int loadoutIndex, bool reorderAfterCreate)
    {
        if (_loadoutButtonTemplate == null || _buttonsParent == null) return;

        for (int i = 0; i < _buttonMap.Count; i++)
        {
            if (_buttonMap[i] != null && _buttonMap[i].LoadoutIndex == loadoutIndex)
            {
                return;
            }
        }

        Button newButton = Instantiate(_loadoutButtonTemplate, _buttonsParent);
        newButton.gameObject.SetActive(true);

        _dynamicButtons.Add(newButton);

        RegisterButton(newButton, loadoutIndex);

        if (reorderAfterCreate)
        {
            ReorderInlineButtonsByLoadoutIndex();
        }

    }
    /// <summary>
    /// Finds the next loadout index (starting from 4) that exists
    /// in PlayerData, is empty and is not currently visible. Returns
    /// -1 if no such loadouts exists
    /// </summary>
    /// <returns>
    /// The loadout index if found, otherwise -1
    /// </returns>
    private int FindNextHiddenEmptyLoadoutIndex()
    {
        if (_player == null) return -1;

        int max = _player.LoadOuts.Length;

        for (int loadoutIndex = 4; loadoutIndex <= max; loadoutIndex++)
        {
            TeamLoadOut slot = _player.LoadOuts[loadoutIndex - 1];

            if (slot != null && !slot.IsEmpty)
            {
                continue;
            }

            bool alreadyVisible = false;
            for (int i = 0; i < _buttonMap.Count; i++)
            {
                if (_buttonMap[i] != null && _buttonMap[i].LoadoutIndex == loadoutIndex)
                {
                    alreadyVisible = true;
                    break;
                }
            }

            if (!alreadyVisible)
            {
                return loadoutIndex;
            }
        }

        return -1;
    }

    /// <summary>
    /// Updates the visibility of the + button, the button is shown only if
    /// there is at least one empty hidden loadout that can be added
    /// </summary>
    private void UpdateAddButtonVisibility()
    {
        if (_addLoadoutButton == null || _player == null) return;

        int next = FindNextHiddenEmptyLoadoutIndex();
        _addLoadoutButton.gameObject.SetActive(next != -1);
    }

    /// <summary>
    /// Reorders all inline buttons so they appear in right order
    /// 1,2,3 etc. The + button is always placed after the last loadout button
    /// </summary>
    private void ReorderInlineButtonsByLoadoutIndex()
    {

        if (IsInlineMode == false) return;

        for (int i = _buttonMap.Count - 1; i >= 0; i--)
        {
            if (_buttonMap[i] == null || _buttonMap[i].Button == null)
            {
                _buttonMap.RemoveAt(i);
            }
        }

        //Sorts buttons by loadout index (ascending order 1,2,3 etc.)
        for (int i = 0; i < _buttonMap.Count - 1; i++)
        {
            for (int j = i + 1; j < _buttonMap.Count; j++)
            {
                if (_buttonMap[j].LoadoutIndex < _buttonMap[i].LoadoutIndex)
                {
                    ButtonLoadoutEntry temp = _buttonMap[i];
                    _buttonMap[i] = _buttonMap[j];
                    _buttonMap[j] = temp;
                }
            }
        }

        // Determines the base sibling index. This defines where the loadout
        //button "area" starts in hierarchy
        // Find the smallest sibling index among loadout buttons and the "+" button
        int baseIndex = int.MaxValue;

        for (int i = 0; i < _buttonMap.Count; i++)
        {
            int sibling = _buttonMap[i].Button.transform.GetSiblingIndex();

            if (sibling < baseIndex)
            {
                baseIndex = sibling;
            }
        }

        if (_addLoadoutButton != null)
        {
            int plusSibling = _addLoadoutButton.transform.GetSiblingIndex();
            if (plusSibling < baseIndex)
            {
                baseIndex = plusSibling;
            }
        }

        // Fallback: if there were no buttons at all, start at 0
        if (baseIndex == int.MaxValue)
        {
            baseIndex = 0;
        }

        // Apply sibling order, loadout buttons first
        for (int i = 0; i < _buttonMap.Count; i++)
        {
            if (_buttonMap[i] == null || _buttonMap[i].Button == null)
            {
                continue;
            }

            _buttonMap[i].Button.transform.SetSiblingIndex(baseIndex + i);
        }

        //  "+" button always after loadout buttons
        if (_addLoadoutButton != null)
        {
            _addLoadoutButton.transform.SetSiblingIndex(baseIndex + _buttonMap.Count);
        }
    }

}







