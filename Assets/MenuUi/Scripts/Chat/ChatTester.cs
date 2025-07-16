using System;
using UnityEngine;
using Random = UnityEngine.Random;


/// <summary>
/// Sends random messages to chat for testing purposes.
/// </summary>
public class ChatTester : MonoBehaviour
{

    [SerializeField]
    Chat _chat;


    public void DoShortMessage()
    {
        _chat.SendChatMessage(_chat.giveJoyPref());
    }

    public void DoLongMessage()
    {

    }
}
