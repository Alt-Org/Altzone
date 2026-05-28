using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine;

// In order to sort items and other purchases, we'll have enum for each type.
public enum ItemType
{
    Misc,
    AvatarPiece,
    Animation
}

public class Ownership
{
    public List<string> _ownedMiscItems_Ids;

    public List<string> _ownedAvatarPiece_Ids;
    public List<string> _ownedAnimation_Ids;
    public List<string> _ownedUIStyle_ids;   // Item not implemented yet

    // public List<AvatarDefault> _defaultOwnerships;     // Add default avatarparts
    public List<string> _defaultOwnerships;

    private PlayerData _playerData = null;

    public void AddItem(string id, ItemType type)
    {
        switch (type)
        {
            case ItemType.AvatarPiece:
                AddToList(id, _ownedAvatarPiece_Ids);
                break;
            case ItemType.Animation:
                AddToList(id, _ownedAnimation_Ids);
                break;
            default:
                AddToList(id, _ownedMiscItems_Ids);
                break;
        }
        //SaveItems();
    }

    private void AddToList(string id, List<string> list)
    {
        if (!list.Contains(id)) list.Add(id);
    }

    public void RemoveItem(string id, ItemType type)
    {
        if (_ownedAvatarPiece_Ids.Contains(id)) _ownedAvatarPiece_Ids.Remove(id);
        if (_ownedAnimation_Ids.Contains(id)) _ownedAnimation_Ids.Remove(id);
        //SaveItems();
    }

    public void SaveItems()
    {
        _playerData.SaveOwnerships();    // Propably doesn't work like this
    }

    public bool CheckItemOwnership(string id)
    {
        AssignDefaultParts();   // Check if we have defaultparts already, and if not, assigns them.
        foreach (string partId in _defaultOwnerships) { if (partId == id) { return true; } }
        if (_ownedAvatarPiece_Ids.Contains(id)) return true;
        if (_ownedAnimation_Ids.Contains(id)) return true;
        
        return false;
    }

    private void ArrangeDefaultParts(AvatarDefault list)
    {
        _defaultOwnerships.Add(list.HairId);
        _defaultOwnerships.Add(list.EyesId);
        _defaultOwnerships.Add(list.NoseId);
        _defaultOwnerships.Add(list.MouthId);
        _defaultOwnerships.Add(list.BodyId);
        _defaultOwnerships.Add(list.HandsId);
        _defaultOwnerships.Add(list.FeetId);
    }

    /// <summary>
    /// Checks if player has default avatarparts.
    /// If not, this method checks characterclasstype and takes that one's defaultparts.
    /// </summary>
    public void AssignDefaultParts()
    {
        if (_defaultOwnerships == null)
        {
            Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => _playerData = p);
            AvatarDefault playerDefault = AvatarReference.Instance.GetDefaultAvatar((CharacterClassType)(_playerData.SelectedCharacterId / 100 * 100));
            ArrangeDefaultParts(playerDefault);
        }
        return;
    }
}

