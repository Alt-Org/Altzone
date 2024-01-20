using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

using static Battle0.Scripts.Lobby.InRoom.RoomSetupManager;

public class GraphicsChanger : MonoBehaviour
{
    [Header("LoserGraphics")] 
    [SerializeField] private Sprite LoserHeart;
    [SerializeField] private Sprite LoserBackGround;
    [SerializeField] private Sprite Loser_Loot_BackGround;
    [SerializeField] private Sprite LoserTextBox;
    [SerializeField] private Sprite LoserLungsEmpty;
    [SerializeField] private Sprite LoserLungsFull;
    [Header("Replace These")]
    [SerializeField] private Image Heart;
    [SerializeField] private Image BackGround;
    [SerializeField] private Image Loot_BackGround;
    [SerializeField] private Image TextBox;
    [SerializeField] private Image LungsEmpty;
    [SerializeField] private Image LungsFull;
    void Start()
    {
        Debug.Log("GraphicsChanger");
        if ((PlayerRole)PhotonNetwork.LocalPlayer.CustomProperties["Role"] == PlayerRole.Spectator)
        {
            Debug.Log("Switching to Loser graphics");
            SwitchToLoserGraphics();
        }
    }
    private void SwitchToLoserGraphics()
    {
        Heart.sprite = LoserHeart;
        BackGround.sprite = LoserBackGround;
        Loot_BackGround.sprite = Loser_Loot_BackGround;
        TextBox.sprite = LoserTextBox;
        LungsEmpty.sprite = LoserLungsEmpty;
        LungsFull.sprite = LoserLungsFull;
    }
}
