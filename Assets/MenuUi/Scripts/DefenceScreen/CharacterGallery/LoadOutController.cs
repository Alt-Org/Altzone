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
    //Skripti kiinnitetty DefenceViewiin samaan objektiin missä ModelController ja LoadOutButtonit vedetty sinne laatikoihin.. ?
    [SerializeField] private List<Button> _loadoutButtons;

    private ModelController _modelController;
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
                _loadoutButtons[i].onClick.AddListener(() => OnPressLoadout(loadoutIndex));
        } 
    }
    //Pitäisikö tähän lisätä OnDestroy-metodi? 

    private void HandlePlayerDataReady(PlayerData data)
    {
        _player = data;
        RefreshButtons();
    }

    private void OnPressLoadout(int loadoutIndex)
    {
        if (_player == null) return;

        if (loadoutIndex == 0)
        {
            
            _player.ApplyLoadout(0);
            RefreshButtons();
            Storefront.Get().SavePlayerData(_player, null);
            return;
        }

        TeamLoadOut slot = _player.LoadOuts[loadoutIndex - 1];
        bool isEmpty = (slot == null) || slot.IsEmpty;

        if (isEmpty)
        {
            // Tämä nyt väliaikainen ratkaisu, hyväksyy tallenuksen aina automaattisesti
            bool userConfirmed = true;
            if (userConfirmed)
            {
                _player.SaveCurrentTeamToLoadout(loadoutIndex);
                _player.ApplyLoadout(loadoutIndex);
                RefreshButtons();
                Storefront.Get().SavePlayerData(_player, null);
            }
            return;
        }

        
        _player.ApplyLoadout(loadoutIndex);
        RefreshButtons();
        Storefront.Get().SavePlayerData(_player, null);
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

