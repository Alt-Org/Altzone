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
    public string characterId { get; set; }
    public string name { get; set; }
    public int speed { get; set; }
    public int resistance { get; set; }
    public int attack { get; set; }
    public int defence { get; set; }
    public int hp { get; set; }
    public int level { get; set; }

    ServerCharacter(){}

    public ServerCharacter(CustomCharacter character)
    {
        _id = character.ServerID;
        characterId = ((int)character.Id).ToString();
        name = character.Name;
        speed = character.Speed;
        resistance = character.Resistance;
        attack = character.Attack;
        defence = character.Defence;
        hp = character.Hp;
        level = 0;
    }

    public ServerCharacter(CharacterID id)
    {
        _id = string.Empty;
        characterId = ((int)id).ToString();

        var store = Storefront.Get();

        ReadOnlyCollection<BaseCharacter> allItems = null;
        store.GetAllBaseCharacterYield(result => allItems = result);

        BaseCharacter character = allItems.FirstOrDefault(x => x.Id == id);

        name = CustomCharacter.GetCharacterName(id);
        speed = character.Speed;
        resistance = character.Resistance;
        attack = character.Attack;
        defence = character.Defence;
        hp = character.Hp;
        level = 0;

    }
}
