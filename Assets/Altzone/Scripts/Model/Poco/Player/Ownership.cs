using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
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

    public List<AvatarDefault> _defaultOwnerships;     // Add default avatarparts

    public void AddItem(string id, ItemType type)
    {
        // Differentiating types of items seems encumbaring currently
        // Option 1: add patterns to item_ids can be used to sort them.
        // Option 2: add every item into one list.
        // Option 3: first item in the list is identifier
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
        //SaveItems(type);
    }

    private void AddToList(string id, List<string> list)
    {
        if (!list.Contains(id)) list.Add(id);
    }

    public void RemoveItem(string id, ItemType type)
    {
        if (_ownedAvatarPiece_Ids.Contains(id)) _ownedAvatarPiece_Ids.Remove(id);
        if (_ownedAnimation_Ids.Contains(id)) _ownedAnimation_Ids.Remove(id);
        //SaveItems(type);
    }

    public void SaveItems()
    {
        //_playerData.SavePlayerData();    // Propably doesn't work like this
        // Save PlayerData
    }

    public bool CheckItemOwnership(string id)
    {
        foreach (AvatarDefault part in _defaultOwnerships)
        {
            if (part.HairId == id) { return true; }
            if (part.EyesId == id) { return true; }
            if (part.NoseId == id) { return true; }
            if (part.MouthId == id) { return true; }
            if (part.BodyId == id) { return true; }
            if (part.HandsId == id) { return true; }
            if (part.FeetId == id) { return true; }
        }
        if (_ownedAvatarPiece_Ids.Contains(id)) return true;
        if (_ownedAnimation_Ids.Contains(id)) return true;
        
        return false;
    }  
}

