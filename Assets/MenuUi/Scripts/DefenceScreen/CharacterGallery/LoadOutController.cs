using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.CharacterGallery;
using MenuUi.Scripts.Signals;
using UnityEngine;
using UnityEngine.UI;

public class LoadOutController : MonoBehaviour
{
    [SerializeField] private List<Button> _loadoutButtons;
    [SerializeField] private ModelController _modelController;

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
                // Tämä on nyt väliaikainen ratkaisu, hyväksyy tallennuksen aina automaattisesti
                bool userConfirmed = true;
                if (userConfirmed)
                {
                    player.SaveCurrentTeamToLoadout(loadoutIndex);
                    player.ApplyLoadout(loadoutIndex);
                    RefreshButtons();
                    Storefront.Get().SavePlayerData(player, null);
                }
                return;
            }

            player.ApplyLoadout(loadoutIndex);
            RefreshButtons();
            Storefront.Get().SavePlayerData(player, null);
        });
    }

    private void RefreshButtons()
    { 
        int selected = _player.SelectedLoadOut;

        for (int i = 0; i < _loadoutButtons.Count; i++)
        {
            if (_loadoutButtons[i] != null)
                _loadoutButtons[i].interactable = (selected != i);
        }
    }
    


}

