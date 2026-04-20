using Altzone.Scripts.Model.Poco.Player;
using Assets.Altzone.Scripts.Model.Poco.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Altzone.Scripts.Model.Poco.Player
{
    public enum ItemType
    {
        AvatarPiece,
        Animation
    }

    public class Omistukset : MonoBehaviour
    {
        // TODO: PlayerPrefs -> LocalModels.PlayerData
        // TODO: How to differentiate AvatarPiece Ids - Animation Ids
        // TODO: implement connections with shop

        public List<string> ownedAvatarPiece_Ids;   // I'll focus on this now
        public List<string> ownedAnimation_Ids;
        public List<string> OwnedUIStyle_ids;   // Item not implemented yet

        readonly PlayerData _playerData;

        //DataStore _storefront = Storefront.Get();

        private void Start()
        {            
            GetItems();
        }

        public void AddItem(string id, ItemType type)
        {
            // Differentiating types of items seems encumbaring currently
            // Option 1: add patterns to item_ids can be used to sort them.
            // Option 2: add every item into one list.
            // Option 3: first item in the list is identifier
            switch (type)
            {
                case ItemType.AvatarPiece:
                    ownedAvatarPiece_Ids.Add(id);
                    break;
                case ItemType.Animation:
                    ownedAnimation_Ids.Add(id);
                    break;
            }
            
            SaveItems(type);
        }

        public void RemoveItem(string id, ItemType type)
        {
            if (ownedAvatarPiece_Ids.Contains(id)) ownedAvatarPiece_Ids.Remove(id);
            if (ownedAnimation_Ids.Contains(id)) ownedAnimation_Ids.Remove(id);
            SaveItems(type);
        }

        public void GetItems()
        {
            //storefront.GetPlayerData(null, null);   // I don't know
            ownedAvatarPiece_Ids = _playerData.OwnedAvatarPiece_Ids;
            ownedAnimation_Ids = _playerData.OwnedAnimation_Ids;
        }

        public void SaveItems(ItemType type)
        {
            //storefront.SavePlayerData(_playerData, null);
            _playerData.UpdateOwnedAvatarPieceIDs((int)type, ownedAvatarPiece_Ids);    // Propably doesn't work like this
            _playerData.UpdateOwnedAvatarAnimationIDs(ownedAnimation_Ids);
        }     
    }
}

