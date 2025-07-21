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
    [SerializeField]
    GameObject _messagePrefab;


    public void DoShortMessage()
    {
        _chat.DisplayMessage("test", _messagePrefab);
    }

    public void DoLongMessage()
    {
        _chat.DisplayMessage("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus consequat velit non purus auctor pharetra. Maecenas sit amet porttitor lectus, et pellentesque neque. Aenean quis lectus accumsan, impe", _messagePrefab);
    }
}
