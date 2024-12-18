using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

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

    private GameObject selectedMessage;

    [Header("Commands")]
    public string delete = "/deleteMessage";
    public string deleteAllMessages = "/clear";

    private  Dictionary<GameObject, List<GameObject>> messagesByChat = new Dictionary<GameObject, List<GameObject>>();


    private void Start()
    {
        currentContent = languageChat;
        Debug.Log("Активен язык чат");

        messagesByChat[languageChat] = new List<GameObject>();
        messagesByChat[globalChat] = new List<GameObject>();
        messagesByChat[clanChat] = new List<GameObject>();

        LanguageChatActive();
    }

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                GameObject touchedObject = EventSystem.current.currentSelectedGameObject;

                if (touchedObject != null && touchedObject.CompareTag("ChatMessage"))  // Замените "YourTag" на нужный тег
                {
                    Debug.Log("Touched UI object with the specified tag ChatMessage");
                }
            }
        }
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

            AddMessageInteraction(newMessage);

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

    public void AddMessageInteraction(GameObject message)
    {
        Button button = message.GetComponent<Button>();
        if (button == null)
        {
            button = message.AddComponent<Button>();
        }

        button.onClick.AddListener(() => SelectMessage(message)); // Обработка выбора сообщения
    }

    public void SelectMessage(GameObject message)
    {
        if (selectedMessage != null)
        {
            DeselectMessage(selectedMessage);
        }

        selectedMessage = message;
        HighlightMessage(selectedMessage);
    }

    private void DeselectMessage(GameObject message)
    {
        if (message.GetComponent<Image>() != null)
        {
            message.GetComponent<Image>().color = Color.white;
        }
    }

    private void HighlightMessage(GameObject message)
    {
        if (message.GetComponent<Image>() != null)
        {
            message.GetComponent<Image>().color = Color.gray;
        }
    }

    public void DeleteChoseMessage()
    {
        if (selectedMessage != null)
        {
            Debug.Log("Удаляем выбранное сообщение");
            messagesByChat[currentContent].Remove(selectedMessage);
            Destroy(selectedMessage);
            selectedMessage = null; // Сбрасываем выбор
        }
        else
        {
            Debug.LogWarning("Сообщение для удаления не выбрано");
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
    public void LanguageChatActive()
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
