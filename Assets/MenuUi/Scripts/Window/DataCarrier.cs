using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;

public class DataCarrier : MonoBehaviour
{
    public static DataCarrier Instance { get; private set; }
    public ServerClan clanToView;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        clanToView = null;
    }
}
