using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using Photon.Pun;

public class Raid_LootTracking : MonoBehaviour
{
    [SerializeField, Header("Reference scripts")] private Raid_References raid_References;

    [SerializeField, Header("Reference game components")] private TMP_Text CurrentLootText;
    [SerializeField] private TMP_Text OutOfText;
    [SerializeField] private TMP_Text MaxLootText;

    [SerializeField, Header("Variables")] public float CurrentLootWeight;
    [SerializeField] public float MaxLootWeight;

    public PhotonView _photonView { get; private set; }

    public void Awake()
    {
        _photonView = gameObject.AddComponent<PhotonView>();
        _photonView.ViewID = 3;
        if (PhotonNetwork.IsMasterClient)
        {
            float randomlootWeight = Random.Range(200, 501);
            _photonView.RPC(nameof(SetRandomMaxLootWeightRPC), RpcTarget.All, randomlootWeight);
        }
        ResetLootCount();
    }

    [PunRPC]
    public void SetRandomMaxLootWeightRPC(float maxLootWeight)
    {
        MaxLootWeight = maxLootWeight;
        this.MaxLootText.text = MaxLootWeight.ToString() + " kg";
    }

    public void ResetLootCount()
    {
        CurrentLootWeight = 0;
        this.CurrentLootText.text = CurrentLootWeight.ToString() + " kg";
        this.OutOfText.text = "Out of";
        this.MaxLootText.text =  MaxLootWeight.ToString() + " kg";
    }

    public void SetLootCount(float AddedLootWeight, float MaxLootWeight)
    {
        float NewLootWeight = CurrentLootWeight + AddedLootWeight;
        CurrentLootWeight = NewLootWeight;
        this.CurrentLootText.text = NewLootWeight.ToString() + " kg";
        this.MaxLootText.text = MaxLootWeight.ToString() + " kg";
        if(CurrentLootWeight > MaxLootWeight)
        {
            raid_References.RedScreen.SetActive(true);
            raid_References.EndMenu.SetActive(true);
            raid_References.OutOfSpace.enabled = true;
        }
    }
}
