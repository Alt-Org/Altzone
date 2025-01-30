using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

public class ServerCharacter 
{
    public string _id { get; set; }
    public string unityKey { get; set; }
    public string name { get; set; }
    public string speed { get; set; }
    public string resistance { get; set; }
    public string attack { get; set; }
    public string defence { get; set; }
    public string hp { get; set; }

    public ServerCharacter(CustomCharacter character)
    {
        _id = character.ServerID;
        unityKey = ((int)character.Id).ToString();
        name = character.Name;
        speed = character.Speed.ToString();
        resistance = character.Resistance.ToString();
        attack = character.Attack.ToString();
        defence = character.Defence.ToString();
        hp = character.Hp.ToString();

    }

    public ServerCharacter(CharacterID id)
    {
        _id = string.Empty;
        unityKey = ((int)id).ToString();

        var store = Storefront.Get();

        ReadOnlyCollection<BaseCharacter> allItems = null;
        store.GetAllBaseCharacterYield(result => allItems = result);

        BaseCharacter character = allItems.FirstOrDefault(x => x.Id == id);

        name = CustomCharacter.GetCharacterName(id);
        speed = character.Speed.ToString();
        resistance = character.Resistance.ToString();
        attack = character.Attack.ToString();
        defence = character.Defence.ToString();
        hp = character.Hp.ToString();

    }
}
