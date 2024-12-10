using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Chat : MonoBehaviour
{
    [Header("Chat")] 
    public GameObject languageChat; 
    public GameObject globalChat;
    public GameObject clanChat;
    private GameObject currentContent;

    [Header("InputField")]
    public TMP_InputField inputField;

    [Header("Prefab")]
    public GameObject messagePrefabBlue;
    public GameObject messagePrefabRed;
    public GameObject messagePrefabYellow;
    public GameObject messagePrefabOrange;
    public GameObject messagePrefabPink;

    [Header("Scroll Rects")]
    public ScrollRect languageChatScrollRect;
    public ScrollRect globalChatScrollRect;
    public ScrollRect clanChatScrollRect;

    private ScrollRect currentScrollRect;

    private GameObject currentPrefab;

    private bool shouldScroll = false;

    [Header("Commands")]
    public string delete = "/deleteMessage";
    public string deleteAllMessages = "/clear";
    public string deleteById = "/deleteId";

    private int messageIDCounter = 0;
    private Dictionary<int, GameObject> messageByID = new Dictionary<int, GameObject>();


    private  Dictionary<GameObject, List<GameObject>> messagesByChat = new Dictionary<GameObject, List<GameObject>>();

    private void Start()
    {
        currentContent = languageChat;
        Debug.Log("Активен язык чат");

        messagesByChat[languageChat] = new List<GameObject>();
        messagesByChat[globalChat] = new List<GameObject>();
        messagesByChat[clanChat] = new List<GameObject>();

        LanguageCahtActive();
    }
    public void SendChatMessage()
    {
        if (currentContent == null)
        {
            Debug.LogWarning("Aktiivista Chat ei ole valittu");
        }
        
        if (inputField != null && !string.IsNullOrEmpty(inputField.text) && inputField.text.Trim().Length >= 2)
        {
            string inputText = inputField.text.Trim();

            if(inputText == delete)
            {
                Debug.Log("Deleting last message...");
                DeleteLastMessage();
                inputField.text = "";
                return;
            }
            else if(inputText == deleteAllMessages)
            {
                Debug.Log("Deleting last message...");
                DeleteAllMessages();
                inputField.text = "";
                return;
            }
            else if(inputText.StartsWith(deleteById))
            {
                string[] parts = inputText.Split(" ");
                if (parts.Length == 2 && int.TryParse(parts[1], out int messageId))
                {
                    Debug.Log("Deleting message with ID");
                }
            }

            Debug.Log("Current Prefab: " + currentPrefab.name);
            DisplayMessage(inputField.text);
            inputField.text = "";
        }
        else
        {
            Debug.LogWarning("Tyhjää viestiä ei voi lähettää..");
        }
    }

    public void SendQuickMessage(Button button)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            string textFromButton = buttonText.text;
            inputField.text = textFromButton;
        }
        else
        {
            Debug.LogError("Virhe: TMP_Text ei löydy painikkeesta.");
        }
    }


    public void DisplayMessage(string messageText)
    {
        if (currentPrefab != null)
        {
            GameObject newMessage = Instantiate(currentPrefab, currentContent.transform);

            TMP_Text messageUI = newMessage.GetComponentInChildren<TMP_Text>();
            if (messageUI != null)
            {
                messageUI.text = messageText;  
            }
            else
            {
                Debug.LogError("TMP_Text-komponenttia ei löytynyt prefabista!");
            }

            //ID
            int currentMessageID = messageIDCounter++;
            newMessage.name = "Message_" + currentMessageID;
            messageByID[currentMessageID] = newMessage;

            messagesByChat[currentContent].Add(newMessage);
            shouldScroll = true;

            if (shouldScroll)
            {
                if (currentContent != null)
                {
                    Canvas.ForceUpdateCanvases();
                    currentScrollRect.verticalNormalizedPosition = 0f;

                    shouldScroll = false;
                }
                else
                {
                    Debug.LogWarning("Scroll rect not found!!");
                }
            }
        }
        else
        {
            Debug.LogError("Prefab ei ole määritetty.");
        }
    }

    public void DeleteLastMessage()
    {
        if (messagesByChat[currentContent].Count > 0)
        {
            Debug.Log("viimeisimmän viestin poistaminen");
            GameObject lastMessage = messagesByChat[currentContent][messagesByChat[currentContent].Count - 1];
            Destroy(lastMessage);
            messagesByChat[currentContent].RemoveAt(messagesByChat[currentContent].Count - 1);
        }
        else
        {
            Debug.LogWarning("ei viestiä poistettavaksi");
        }
    }

    public void DeleteAllMessages()
    {
        Debug.Log("poistaa kaikki viestit");
        foreach (GameObject message in messagesByChat[currentContent])
        {
            Destroy(message);
        }

        messagesByChat[currentContent].Clear();
    }

    public void DeleteMessageByID(int messageId)
    {
        if(messageByID.ContainsKey(messageId))
        {
            Debug.Log("Deleting message with ID");
            GameObject messageToDelete = messageByID[messageId];
            Destroy(messageToDelete);
            messagesByChat[currentContent].Remove(messageToDelete);
            messageByID.Remove(messageId);
        }
        else
        {
            Debug.LogWarning("Message with ID not found.");
        }
    }

    public void GlobalCahtActive()
    {
        currentContent = globalChat;
        currentScrollRect = globalChatScrollRect;

        globalChat.SetActive(true);
        languageChat.SetActive(false);
        clanChat.SetActive(false);

        Debug.Log("Global Chat aktivoitu");
    }
    public void ClanCahtActive()
    {
        currentContent = clanChat;
        currentScrollRect = clanChatScrollRect;

        clanChat.SetActive(true);
        languageChat.SetActive(false);
        globalChat.SetActive(false);
       
        Debug.Log("Klaani Chat aktivoitu");
    }
    public void LanguageCahtActive()
    {
        currentContent = languageChat;
        currentScrollRect = languageChatScrollRect;

        languageChat.SetActive(true);
        globalChat.SetActive(false);
        clanChat.SetActive(false);

        Debug.Log("Kielivalinnan mukainen Chat aktivoitu");
    }

    public void SetBluePrefab() { currentPrefab = messagePrefabBlue; SendChatMessage(); }
    public void SetRedPrefab() { currentPrefab = messagePrefabRed; SendChatMessage(); }
    public void SetYellowPrefab() { currentPrefab = messagePrefabYellow; SendChatMessage(); }
    public void SetOrangePrefab() { currentPrefab = messagePrefabOrange; SendChatMessage(); }
    public void SetPinkPrefab() { currentPrefab = messagePrefabPink; SendChatMessage(); }
}
