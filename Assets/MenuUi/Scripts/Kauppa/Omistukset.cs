using Altzone.Scripts.Model.Poco.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts
{
    public class Omistukset
    {
        // TODO: PlayerPrefs -> LocalModels.PlayerData

        public List<string> ownedAvatarPiece_Ids;   // I'll focus on this now
        // public List<string> ownedAnimation_Ids;

        PlayerData _playerData;

        DataStore storefront = Storefront.Get();

        private void Start()
        {            
            GetItems();
        }

        public void AddItem(string id)
        {
            ownedAvatarPiece_Ids.Add(id);
            SaveItems();
        }

        public void RemoveItem(string id)
        {
            if (ownedAvatarPiece_Ids.Contains(id)) ownedAvatarPiece_Ids.Remove(id);
            SaveItems();
        }

        public void GetItems()
        {
            //storefront.GetPlayerData(null, null);   // I don't know
            ownedAvatarPiece_Ids = _playerData.OwnedAvatarPiece_Ids;
        }

        public void SaveItems()
        {
            //storefront.SavePlayerData(_playerData, null);
            _playerData.UpdateOwnedAvatarPieceIDs(ownedAvatarPiece_Ids);    // Propably doesn't work like this
        }     
    }
}

