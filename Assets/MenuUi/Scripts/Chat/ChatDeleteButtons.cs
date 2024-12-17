using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatDeleteButtons : MonoBehaviour
{
    public GameObject delete;
    public string messageTag = "ChatMessage";

    List<GameObject> messages = new List<GameObject>();

    private void Start()
    {
        messages.AddRange(GameObject.FindGameObjectsWithTag(messageTag));
        Debug.Log($"Find {messages.Count} messages with Tag '{messageTag}'.");

        foreach(var message in messages)
        {
            AddEventTrigger(message);
        }
    }

    private void AddEventTrigger(GameObject message)
    {
        EventTrigger trigger = message.GetComponent<EventTrigger>();
        if (trigger != null)
            trigger = message.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entry.callback.AddListener((data) => MessagePointerDown(message));
        trigger.triggers.Add(entry);
    }

    public void MessagePointerDown(GameObject message)
   {
        if(!delete.activeSelf)
        {
            delete.SetActive(true);
            Debug.Log("Delete panel is active");
        }
   }
}
