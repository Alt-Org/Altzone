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

    //Loadout 2
    [SerializeField] private bool _useZeroSlotButton = true;

    [Header("Dynamic inline loadouts")]
    [SerializeField] private Button _loadoutButtonTemplate;
    [SerializeField] private Button _addLoadoutButton;        
    [SerializeField] private Transform _buttonsParent;        

    private PlayerData _player;

    private void Awake()
    {
        if (_modelController == null)
            _modelController = GetComponent<ModelController>();

      
        if (_modelController != null)
            _modelController.OnPlayerDataReady += HandlePlayerDataReady;
    }
    private void Start()
    {
        for (int i = 0; i < _loadoutButtons.Count; i++)
        {
            int buttonIndex = i;
            if (_loadoutButtons[i] == null) continue;

            _loadoutButtons[i].onClick.AddListener(() =>
            {
                int loadoutIndex = ButtonIndexToLoadoutIndex(buttonIndex);
                OnPressLoadout(loadoutIndex, _player);
            });
        }
    }

    private void HandlePlayerDataReady(PlayerData data)
    {
        _player = data;
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

        if (player == null)
        {
            Debug.LogError("OnPressLoadout called with null PlayerData parameter");
        }
   

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
                SaveToEmptySlot(loadoutIndex, player);
                
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
                
                if (isSelected)
                {
                    shouldDisable = true;
                }
                else
                {
                    shouldDisable = false;
                }
            }

            btn.interactable = !shouldDisable;
        }
    }

    /// <summary>
    /// -Adds a new loadout button before the "+" button, if a free slot exists in PlayerData
    /// -Currently limited by the length of PlayerData.LoadOuts (no more buttons can be added once the array is full)
    /// </summary>
    public void AddNewLoadoutSlot()
    {

        if (_player == null)
        {
            Debug.LogWarning("AddNewLoadoutSlot called but _player is null");
            return;
        }
        if (_loadoutButtonTemplate == null || _buttonsParent == null)
        {
            Debug.LogWarning("AddNewLoadoutSlot: template or parent is not set");
            return;
        }

        int newLoadoutIndex = ButtonIndexToLoadoutIndex(_loadoutButtons.Count);

        
        if (newLoadoutIndex <= 0 || newLoadoutIndex > _player.LoadOuts.Length)
        {
            Debug.LogWarning($"No more loadout slots available in PlayerData (trying to use {newLoadoutIndex})");
            return;
        }

        
        Button newButton = Instantiate(_loadoutButtonTemplate, _buttonsParent);
        newButton.gameObject.SetActive(true);

        int insertIndex = _addLoadoutButton.transform.GetSiblingIndex();
        newButton.transform.SetSiblingIndex(insertIndex);

        TMP_Text numberLabel = newButton.GetComponentInChildren<TMP_Text>();
        if (numberLabel != null)
        {
            numberLabel.text = newLoadoutIndex.ToString();
        }

        
        _loadoutButtons.Add(newButton);

        int buttonIndex = _loadoutButtons.Count - 1;
        newButton.onClick.RemoveAllListeners();
        newButton.onClick.AddListener(() =>
        {
            int loadoutIdx = ButtonIndexToLoadoutIndex(buttonIndex);
            OnPressLoadout(loadoutIdx, _player);
        });

        RefreshButtons();
    }
}
    




