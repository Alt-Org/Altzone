using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using MenuUi.Scripts.TabLine;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Model.Poco.Game;

public class Chat : AltMonoBehaviour
{
    [Header("Chat")] 
    public GameObject languageChat; 
    public GameObject globalChat;
    public GameObject clanChat;
    public GameObject currentContent; // Tällä hetkellä aktiivinen chatin content

    [Header("InputField")]
    public TMP_InputField inputField;

    [Header("Delete Ui")]
    public GameObject deleteButtons;

    [Header("Add reactions UI")]
    public GameObject addReactionsPanel;
    public GameObject commonReactions;
    public GameObject allReactions;
    public GameObject usersWhoAdded;

    [Header("Chat Reactions")]
    [SerializeField] private ChatResponseList _chatResponseList;
    [SerializeField] private GameObject _chatResponseContent;

    [Header("Prefab")]
    public GameObject messagePrefabBlue;
    public GameObject messagePrefabRed;
    public GameObject messagePrefabYellow;
    public GameObject messagePrefabOrange;
    public GameObject messagePrefabPink;
    [SerializeField] private GameObject _quickMessagePrefab;

    [Header("Scroll Rects")]
    public ScrollRect languageChatScrollRect;
    public ScrollRect globalChatScrollRect;
    public ScrollRect clanChatScrollRect;

    [Header("Minimize")]
    public GameObject quickMessages;
    public GameObject[] sendButtons;
    public GameObject buttonOpenSendButtons;

    [Header("TablineScript reference")]
    public TabLine tablineScript;

    private ScrollRect currentScrollRect; // Tällä hetkellä aktiivinen Scroll Rect

    private GameObject currentPrefab; // Tällä hetkellä valittu message Prefab

    private bool shouldScroll = false;

    [HideInInspector] public GameObject selectedMessage; // Viesti, joka on tällä hetkellä valittuna

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
        tablineScript.ActivateTabButton(1);
        AddResponses();
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

    private void AddResponses()
    {
        StartCoroutine(GetPlayerData(data =>
        {
            List<string> messageList = _chatResponseList.GetResponses((CharacterClassID)((data.SelectedCharacterId/100)*100));
            foreach (string message in messageList)
            {
                GameObject messageObject = Instantiate(_quickMessagePrefab, _chatResponseContent.transform);
                Button button= messageObject.GetComponent<QuickResponceHandler>().SetData(message);
                button.onClick.AddListener(() => SendQuickMessage(messageObject.GetComponent<Button>()));
            }
        }));

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
                    StartCoroutine(UpdateLayoutAndScroll());
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

    private IEnumerator UpdateLayoutAndScroll()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        yield return null;

        currentScrollRect.verticalNormalizedPosition = 0f;
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
        button.onClick.AddListener(() => MinimizeOptions());
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

        Vector3 deletePosition = deleteButtons.transform.position;
        deletePosition.y = selectedMessage.transform.position.y;
        deleteButtons.transform.position = deletePosition;

        SetReactionPanelPosition();

        deleteButtons.SetActive(true);// Näytä poistopainikkeet, jos viesti on valittuna
        addReactionsPanel.SetActive(true);
    }

    /// <summary>
    /// Sets the reaction panel at the bottom of the selected message's reaction field
    /// </summary>
    private void SetReactionPanelPosition()
    {
        Vector3 reactionPosition = addReactionsPanel.transform.position;
        RectTransform reactionPanelTransfrom = commonReactions.GetComponent<RectTransform>();

        HorizontalLayoutGroup reactionField = selectedMessage.GetComponentInChildren<HorizontalLayoutGroup>();
        RectTransform reactionFieldTransform = reactionField.GetComponent<RectTransform>();

        float fieldBottomY = reactionField.transform.position.y - (reactionFieldTransform.rect.height * reactionFieldTransform.pivot.y);
        float newPanelY = fieldBottomY - (reactionPanelTransfrom.rect.height * reactionPanelTransfrom.pivot.y);
        reactionPosition.y = newPanelY;

        float fieldEdgeX = reactionField.transform.position.x - (reactionFieldTransform.rect.width * reactionFieldTransform.pivot.x);
        float newPanelX = fieldEdgeX + (reactionPanelTransfrom.rect.width * reactionPanelTransfrom.pivot.x);
        reactionPosition.x = newPanelX;

        addReactionsPanel.transform.position = reactionPosition;
    }


    // Korostaa valitun viestin
    private void HighlightMessage(GameObject message)
    {
        if (message.GetComponentInChildren<Image>() != null)
        {
            message.GetComponentInChildren<Image>().color = Color.gray;
        }
    }

    // Poistaa valinnan viestistä
    public void DeselectMessage(GameObject message)
    {
        if (selectedMessage != null)
        {
            if (message.GetComponentInChildren<Image>() != null)
            {
                message.GetComponentInChildren<Image>().color = Color.white;
            }

            deleteButtons.SetActive(false);

            commonReactions.SetActive(true);
            allReactions.SetActive(false);
            addReactionsPanel.SetActive(false);
            usersWhoAdded.SetActive(false);
        }
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

            commonReactions.SetActive(true);
            allReactions.SetActive(false);
            addReactionsPanel.SetActive(false);
            usersWhoAdded.SetActive(false);
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


    private int chosenButton = 2;
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
            else
            {
                sendButtons[chosenButton].SetActive(true);
            }
        }

        buttonOpenSendButtons.SetActive(true);
    }

    /// <summary>
    /// Added to buttons to deselect messages and close the sending options
    /// </summary>
    /// <param name="onlyMessages"></param>
    public void CloseOnButtonClick(bool onlyMessages)
    {
        if (onlyMessages)
        {
            DeselectMessage(selectedMessage);
        }
        else
        {
            DeselectMessage(selectedMessage);
            MinimizeOptions();
        }
    }

    public void UpdateContentLayout(HorizontalLayoutGroup reactionsField)
    {
        StartCoroutine(UpdateLayout(reactionsField));
    }

    private IEnumerator UpdateLayout(HorizontalLayoutGroup reactionsField)
    {
        yield return null;

        if(reactionsField != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(reactionsField.GetComponent<RectTransform>());
        }
        
        VerticalLayoutGroup currentLayout = currentContent.GetComponentInChildren<VerticalLayoutGroup>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(currentLayout.GetComponent<RectTransform>());
    }

    public void OpenUsersWhoAddedReactionPanel()
    {
        addReactionsPanel.SetActive(true);
        commonReactions.SetActive(false);
        allReactions.SetActive(false);
        usersWhoAdded.SetActive(true);
        SetReactionPanelPosition();
    }
}
