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
            int loadoutIndex = i;
            if (_loadoutButtons[i] != null)
                _loadoutButtons[i].onClick.AddListener(() => OnPressLoadout(loadoutIndex, _player));
        } 
    }

    private void HandlePlayerDataReady(PlayerData data)
    {
        _player = data;
        RefreshButtons();
    }

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
        int selected = _player.SelectedLoadOut;

        for (int i = 0; i < _loadoutButtons.Count; i++)
        {
            //if (_loadoutButtons[i] != null)
            //    _loadoutButtons[i].interactable = (selected != i);
            bool isSelected = (selected == i);
            bool shouldDisable = isSelected && i != 0; // 0 pysyy aina klikattavana
            _loadoutButtons[i].interactable = !shouldDisable;
        }
    }
    


}

