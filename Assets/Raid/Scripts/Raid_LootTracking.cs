using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Photon.Pun;

public class Raid_LootTracking : MonoBehaviourPunCallbacks
{
    [SerializeField, Header("Reference scripts")] private Raid_References raid_References;

    [SerializeField, Header("Reference game components")] private TMP_Text CurrentLootText;
    [SerializeField] private TMP_Text OutOfText;
    [SerializeField] private TMP_Text MaxLootText;
    [SerializeField] private TMP_Text HeartLootText;

    [SerializeField, Header("Variables")] public float CurrentLootWeight;
    [SerializeField] public float MaxLootWeight;

    public PhotonView _photonView { get; private set; }

    public void Awake()
    {
        _photonView = gameObject.AddComponent<PhotonView>();
        _photonView.ViewID = 3;
        if (PhotonNetwork.IsMasterClient)
        {
            float randomlootWeight = Random.Range(140, 241);
            _photonView.RPC(nameof(SetRandomMaxLootWeightRPC), RpcTarget.AllBuffered, randomlootWeight);
        }

        ResetLootCount();
    }

    [PunRPC]
    public void SetRandomMaxLootWeightRPC(float maxLootWeight)
    {
        MaxLootWeight = maxLootWeight;
        //Debug.Log("maxLootWeight has been set");
        this.MaxLootText.text = MaxLootWeight.ToString() + " kg";
    }

    public void ResetLootCount()
    {
        CurrentLootWeight = 0;
        CurrentLootText.text = CurrentLootWeight.ToString() + " kg";
        OutOfText.text = "Out of";
        MaxLootText.text =  MaxLootWeight.ToString() + " kg";
    }

    public void SetLootCount(float AddedLootWeight, float MaxLootWeight)
    {
        float NewLootWeight = CurrentLootWeight + AddedLootWeight;
        CurrentLootWeight = NewLootWeight;

        CurrentLootText.text = NewLootWeight.ToString() + " kg";
        MaxLootText.text = MaxLootWeight.ToString() + " kg";
        float weightTMP = MaxLootWeight - NewLootWeight;
        HeartLootText.text = (MaxLootWeight - NewLootWeight).ToString("F0");
        if (CurrentLootWeight > MaxLootWeight)
        {
            //EndScreen
            raid_References.RedScreen.SetActive(true);
            raid_References.EndMenu.SetActive(true);
            raid_References.OutOfSpace.enabled = true;

            //HeartBreak animation
            Color tmp = raid_References.Heart.GetComponent<Image>().color;
            Image[] children = raid_References.HeartHalves.GetComponentsInChildren<Image>();
            foreach (Image image in children)
            {
                image.color = tmp;
            }
            raid_References.HeartHalves.SetActive(true);
            raid_References.Heart.GetComponent<Image>().enabled = false;
        }
    }
    
}
