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
    private GameObject currentContent; // Tällä hetkellä aktiivinen chatin content

    [Header("InputField")]
    public TMP_InputField inputField;

    [Header("Delete Ui")]
    public GameObject deleteButtons;

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

    [Header("Minimize")]
    public GameObject quickMessages;
    public GameObject[] sendButtons;
    public GameObject buttonOpenSendButtons;
    public GameObject optionsMinimizeButton;

    private ScrollRect currentScrollRect; // Tällä hetkellä aktiivinen Scroll Rect

    private GameObject currentPrefab; // Tällä hetkellä valittu message Prefab

    private bool shouldScroll = false;

    private GameObject selectedMessage; // Viesti, joka on tällä hetkellä valittuna

    [Header("Commands")]
    public string delete = "/deleteMessage";
    public string deleteAllMessages = "/clear";

    // Sanakirja (List), jossa viestit järjestetään chat-tyypin mukaan
    private Dictionary<GameObject, List<GameObject>> messagesByChat = new Dictionary<GameObject, List<GameObject>>();


    private void Start()
    {
        // Alustaa chatit ja asettaa kielichatin oletukseksi
        currentContent = languageChat;
        Debug.Log("Language Chat is Active");

        messagesByChat[languageChat] = new List<GameObject>();
        messagesByChat[globalChat] = new List<GameObject>();
        messagesByChat[clanChat] = new List<GameObject>();

        LanguageChatActive();
    }

    private void Update()
    {
        // Tarkistaa kosketuksen ja valitsee viestin, jos sitä klikataan
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                GameObject touchedObject = EventSystem.current.currentSelectedGameObject;

                if (touchedObject != null && touchedObject.CompareTag("ChatMessage")) 
                {
                    Debug.Log("Touched UI object with the specified tag ChatMessage");
                }
            }
        }
    }

    public void SendChatMessage()
    {
        // Lähettää käyttäjän syöttämän viestin aktiiviseen chattiin
        if (currentContent == null)
        {
            Debug.LogWarning("Aktiivista Chat ei ole valittu");
        }
        
        if (inputField != null && !string.IsNullOrEmpty(inputField.text) && inputField.text.Trim().Length >= 3)
        {
            string inputText = inputField.text.Trim();

            // Tarkistaa, onko syöte komento
            if (inputText == delete)
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
            this.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
            MinimizeOptions();
        }
        else
        {
            Debug.LogWarning("Tyhjää viestiä ei voi lähettää..");
        }
    }

    // Asettaa pikaviestin tekstikenttään quickMessage text => InputField text
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

    // Näyttää viestin aktiivisessa chatti-ikkunassa
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

            // Vierittää viestinäkymän alas
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

    // Lisää vuorovaikutuksen viestiin (klikkauksen)
    public void AddMessageInteraction(GameObject message)
    {
        Button button = message.GetComponent<Button>();
        if (button == null)
        {
            button = message.AddComponent<Button>();
        }

        button.onClick.AddListener(() => SelectMessage(message)); 
    }

    // Valitsee viestin
    public void SelectMessage(GameObject message)
    {
        if (selectedMessage != null)
        {
            DeselectMessage(selectedMessage);
        }

        selectedMessage = message;
        HighlightMessage(selectedMessage);

        deleteButtons.SetActive(true);// Näytä poistopainikkeet, jos viesti on valittuna
    }


    // Korostaa valitun viestin
    private void HighlightMessage(GameObject message)
    {
        if (message.GetComponent<Image>() != null)
        {
            message.GetComponent<Image>().color = Color.gray;
        }
    }

    // Poistaa valinnan viestistä
    private void DeselectMessage(GameObject message)
    {
        if (message.GetComponent<Image>() != null)
        {
            message.GetComponent<Image>().color = Color.white;
        }

        deleteButtons.SetActive(false);
    }

    // Poistaa valitun viestin
    public void DeleteChoseMessage()
    {
        if (selectedMessage != null)
        {
            Debug.Log("Удаляем выбранное сообщение");
            messagesByChat[currentContent].Remove(selectedMessage);
            Destroy(selectedMessage);
            selectedMessage = null;

            //Poistopainikkeiden piilottaminen viestin poistamisen jälkeen
            deleteButtons.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Сообщение для удаления не выбрано");
        }
    }

    // Poistaa viimeisen viestin
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

    // Poistaa kaikki viestit aktiivisessa chatissa
    public void DeleteAllMessages()
    {
        Debug.Log("poistaa kaikki viestit");
        foreach (GameObject message in messagesByChat[currentContent])
        {
            Destroy(message);
        }

        messagesByChat[currentContent].Clear();
    }

    // Aktivoi globaalin chatin
    public void GlobalCahtActive()
    { 
        currentContent = globalChat;
        currentScrollRect = globalChatScrollRect;

        globalChat.SetActive(true);
        languageChat.SetActive(false);
        clanChat.SetActive(false);

        Debug.Log("Global Chat aktivoitu");
    }

    // Aktivoi klaanichatin
    public void ClanCahtActive()
    {
        currentContent = clanChat;
        currentScrollRect = clanChatScrollRect;

        clanChat.SetActive(true);
        languageChat.SetActive(false);
        globalChat.SetActive(false);
       
        Debug.Log("Klaani Chat aktivoitu");
    }

    // Aktivoi kielichatin
    public void LanguageChatActive()
    {
        currentContent = languageChat; 
        currentScrollRect = languageChatScrollRect;

        languageChat.SetActive(true);
        globalChat.SetActive(false);
        clanChat.SetActive(false);

        Debug.Log("Kielivalinnan mukainen Chat aktivoitu");
    }


    private int chosenButton;
    // Asettaa sinisen viestipohjan ja lähettää viestin
    public void SetBluePrefab() { currentPrefab = messagePrefabBlue; chosenButton = 0; SendChatMessage(); }
    // Asettaa punaisen viestipohjan ja lähettää viestin
    public void SetRedPrefab() { currentPrefab = messagePrefabRed; chosenButton = 1; SendChatMessage(); }
    // Asettaa keltaisen viestipohjan ja lähettää viestin
    public void SetYellowPrefab() { currentPrefab = messagePrefabYellow; chosenButton = 2; SendChatMessage(); }
    // Asettaa oranssin viestipohjan ja lähettää viestin
    public void SetOrangePrefab() { currentPrefab = messagePrefabOrange; chosenButton = 3; SendChatMessage(); }
    // Asettaa vaaleanpunaisen viestipohjan ja lähettää viestin
    public void SetPinkPrefab() { currentPrefab = messagePrefabPink; chosenButton = 4; SendChatMessage(); }

    /// <summary>
    /// Minimizes quick messages panel and send buttons. Last used send button is the one left visible.
    /// </summary>
    public void MinimizeOptions()
    {
        quickMessages.SetActive(false);

        for (int i = 0; i < sendButtons.Length; i++)
        {
            if(i != chosenButton)
            {
                sendButtons[i].SetActive(false); 
            } 
        }

        buttonOpenSendButtons.SetActive(true);
        optionsMinimizeButton.SetActive(false);
    }
}
