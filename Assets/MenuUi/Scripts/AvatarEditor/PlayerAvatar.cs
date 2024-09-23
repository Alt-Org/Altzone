using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAvatar
{
    private string _characterName;

    public PlayerAvatar(string name)
    {
        _characterName = name;
    }

    public string Name
    {
        get => _characterName;
        set => _characterName = value;
    }
}
