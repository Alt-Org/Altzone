using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using MenuUi.Scripts.TabLine;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Game;

public class Chat : AltMonoBehaviour
{
    [Header("Chat")]
    [SerializeField] private GameObject _languageChat;
    [SerializeField] private GameObject _globalChat;
    [SerializeField] private GameObject _clanChat;
    private GameObject _currentContent; // Tällä hetkellä aktiivinen chatin content

    [Header("Send buttons")]
    [SerializeField] private GameObject _sendButtonSadness;
    [SerializeField] private GameObject _sendButtonAnger;
    [SerializeField] private GameObject _sendButtonJoy;
    [SerializeField] private GameObject _sendButtonPlayful;
    [SerializeField] private GameObject _sendButtonLove;

    [Header("InputField")]
    [SerializeField] private TMP_InputField _inputField;

    [Header("Delete Ui")]
    [SerializeField] private GameObject _deleteButtons;

    [Header("Add reactions UI")]
    [SerializeField] private GameObject _addReactionsPanel;
    [SerializeField] private GameObject _commonReactions;
    [SerializeField] private GameObject _allReactions;
    [SerializeField] private GameObject _usersWhoAdded;

    [Header("Chat Reactions")]
    [SerializeField] private CharacterResponseList _chatResponseList;
    [SerializeField] private GameObject _chatResponseContent;

    [Header("Prefab")]
    [SerializeField] private GameObject _messagePrefabBlue;
    [SerializeField] private GameObject _messagePrefabRed;
    [SerializeField] private GameObject _messagePrefabYellow;
    [SerializeField] private GameObject _messagePrefabOrange;
    [SerializeField] private GameObject _messagePrefabPink;
    [SerializeField] private GameObject _quickMessagePrefab;

    [Header("Scroll Rects")]
    [SerializeField] private ScrollRect _languageChatScrollRect;
    [SerializeField] private ScrollRect _globalChatScrollRect;
    [SerializeField] private ScrollRect _clanChatScrollRect;

    [Header("Minimize")]
    [SerializeField] private GameObject _quickMessages;
    [SerializeField] private GameObject _quickMessagesScrollBar;
    [SerializeField] private GameObject[] _sendButtons;
    
    // Public getters
    public GameObject QuickMessages => _quickMessages;
    public GameObject QuickMessagesScrollBar => _quickMessagesScrollBar;
    public GameObject[] SendButtons => _sendButtons;


    [Header("TablineScript reference")]
    [SerializeField] private TabLine _tablineScript;

    private ScrollRect _currentScrollRect; // Tällä hetkellä aktiivinen Scroll Rect

    private bool shouldScroll = false;

    private GameObject _selectedMessage; // Viesti, joka on tällä hetkellä valittuna

    // Public getter
    public GameObject SelectedMessage => _selectedMessage;  

    // Commands
    private string _delete = "/deleteMessage";
    private string _deleteAllMessages = "/clear";

    // Sanakirja (List), jossa viestit järjestetään chat-tyypin mukaan
    private Dictionary<GameObject, List<GameObject>> messagesByChat = new Dictionary<GameObject, List<GameObject>>();

    private GameObject _lastSendButtonUsed;
    private bool _sendButtonsAreClosed = true;

    [SerializeField] private GameObject _InputArea;

    private void Start()
    {
        // Alustaa chatit ja asettaa kielichatin oletukseksi
        _currentContent = _languageChat;
        Debug.Log("Language Chat is Active");

        messagesByChat[_languageChat] = new List<GameObject>();
        messagesByChat[_globalChat] = new List<GameObject>();
        messagesByChat[_clanChat] = new List<GameObject>();

        LanguageChatActive();
        _tablineScript.ActivateTabButton(1);
        AddResponses();

        _lastSendButtonUsed = _sendButtonJoy;

        // Add send button listeners
        foreach (GameObject sendButton in _sendButtons)
        {
            Button button = sendButton.GetComponent<Button>();
            button.onClick.AddListener(() => CheckSendButton(sendButton));
        }
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
            List<string> messageList = _chatResponseList.GetChatResponses((CharacterClassID)((data.SelectedCharacterId / 100) * 100));
            foreach (string message in messageList)
            {
                GameObject messageObject = Instantiate(_quickMessagePrefab, _chatResponseContent.transform);
                Button button = messageObject.GetComponent<QuickResponceHandler>().SetData(message);
                button.onClick.AddListener(() => SendQuickMessage(messageObject.GetComponent<Button>()));
            }
        }));

    }

    /// <summary>
    /// Checks if other send buttons should be opened or a message sent.
    /// </summary>
    /// <param name="buttonUsed"></param>
    private void CheckSendButton(GameObject buttonUsed)
    {
        if (_sendButtonsAreClosed) // Open other send buttons
        {
            foreach (GameObject sendButton in _sendButtons)
            {
                sendButton.SetActive(true);
            }

            CloseOnButtonClick(true);

            _sendButtonsAreClosed = false;
        }
        else // send a message
        {
            _lastSendButtonUsed = buttonUsed;

            // Check which message prefab should be used
            if(buttonUsed == _sendButtonSadness)
            {
                SendChatMessage(_messagePrefabBlue);
            }
            else if (buttonUsed == _sendButtonAnger)
            {
                SendChatMessage(_messagePrefabRed);
            }
            else if (buttonUsed == _sendButtonJoy)
            {
                SendChatMessage(_messagePrefabYellow);
            }
            else if (buttonUsed == _sendButtonPlayful)
            {
                SendChatMessage(_messagePrefabOrange);
            }
            else if (buttonUsed == _sendButtonLove)
            {
                SendChatMessage(_messagePrefabPink);
            }
        }
    }

    public void SendChatMessage(GameObject messagePrefab)
    {
        // Lähettää käyttäjän syöttämän viestin aktiiviseen chattiin
        if (_currentContent == null)
        {
            Debug.LogWarning("Aktiivista Chat ei ole valittu");
        }

        if (_inputField != null && !string.IsNullOrEmpty(_inputField.text) && _inputField.text.Trim().Length >= 3)
        {
            string inputText = _inputField.text.Trim();

            // Tarkistaa, onko syöte komento
            if (inputText == _delete)
            {
                Debug.Log("Deleting last message...");
                DeleteLastMessage();
                _inputField.text = "";
                return;
            }
            else if (inputText == _deleteAllMessages)
            {
                Debug.Log("Deleting last message...");
                DeleteAllMessages();
                _inputField.text = "";
                return;
            }

            DisplayMessage(_inputField.text, messagePrefab);
            _inputField.text = "";
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
            MinimizeOptions();
            _inputField.text = textFromButton;
            GameObject sendButton = _sendButtons[0];
            CheckSendButton(sendButton);
        }
        else
        {
            Debug.LogError("Virhe: TMP_Text ei löydy painikkeesta.");
        }
    }

    // Näyttää viestin aktiivisessa chatti-ikkunassa
    public void DisplayMessage(string messageText, GameObject messagePrefab)
    {
        if (messagePrefab != null)
        {
            GameObject newMessage = Instantiate(messagePrefab, _currentContent.transform);

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

            messagesByChat[_currentContent].Add(newMessage);

            // Vierittää viestinäkymän alas
            shouldScroll = true;
            if (shouldScroll)
            {
                if (_currentContent != null)
                {
                    StartCoroutine(UpdateLayoutAndScroll(newMessage, _currentContent));
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

    private IEnumerator UpdateLayoutAndScroll(GameObject message, GameObject contentLayout)
    {
        yield return null;
        message.GetComponentInChildren<ChatMessageScript>().MessageSetHeight();

        yield return null;
        Canvas.ForceUpdateCanvases();

        yield return null;
        RectTransform rectTransform = contentLayout.GetComponent<RectTransform>();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

        yield return null;
        _currentScrollRect.verticalNormalizedPosition = 0f;
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
        if (_selectedMessage != null)
        {
            DeselectMessage(_selectedMessage);
        }

        _selectedMessage = message;
        HighlightMessage(_selectedMessage);

        Vector3 deletePosition = _deleteButtons.transform.position;
        deletePosition.y = _selectedMessage.transform.position.y;
        _deleteButtons.transform.position = deletePosition;

        StartCoroutine(SetReactionPanelPosition());

        _deleteButtons.SetActive(true);// Näytä poistopainikkeet, jos viesti on valittuna
        _addReactionsPanel.SetActive(true);
    }

    /// <summary>
    /// Sets the reaction panel at the bottom of the selected message's reaction field
    /// </summary>
    private IEnumerator SetReactionPanelPosition()
    {
        yield return null;

        Vector3 reactionPosition = _addReactionsPanel.transform.position;
        RectTransform reactionPanelTransfrom = _commonReactions.GetComponent<RectTransform>();

        HorizontalLayoutGroup reactionField = _selectedMessage.GetComponentInChildren<HorizontalLayoutGroup>();
        RectTransform reactionFieldTransform = reactionField.GetComponent<RectTransform>();

        float fieldBottomY = reactionField.transform.position.y - (reactionFieldTransform.rect.height * reactionFieldTransform.pivot.y);
        float newPanelY = fieldBottomY - (reactionPanelTransfrom.rect.height * reactionPanelTransfrom.pivot.y);
        reactionPosition.y = newPanelY;

        float fieldEdgeX = reactionField.transform.position.x - (reactionFieldTransform.rect.width * reactionFieldTransform.pivot.x);
        float newPanelX = fieldEdgeX + (reactionPanelTransfrom.rect.width * reactionPanelTransfrom.pivot.x);
        reactionPosition.x = newPanelX;

        _addReactionsPanel.transform.position = reactionPosition;
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
        if (_selectedMessage != null)
        {
            if (message.GetComponentInChildren<Image>() != null)
            {
                message.GetComponentInChildren<Image>().color = Color.white;
            }

            _deleteButtons.SetActive(false);

            DisableReactionPanel();
        }
    }

    // Poistaa valitun viestin
    public void DeleteChoseMessage()
    {
        if (_selectedMessage != null)
        {
            Debug.Log("Удаляем выбранное сообщение");
            messagesByChat[_currentContent].Remove(_selectedMessage);
            Destroy(_selectedMessage);
            _selectedMessage = null;

            // Disable message interaction elements
            _deleteButtons.SetActive(false);
            DisableReactionPanel();
        }
        else
        {
            Debug.LogWarning("Сообщение для удаления не выбрано");
        }
    }

    // Poistaa viimeisen viestin
    public void DeleteLastMessage()
    {
        if (messagesByChat[_currentContent].Count > 0)
        {
            Debug.Log("viimeisimmän viestin poistaminen");
            GameObject lastMessage = messagesByChat[_currentContent][messagesByChat[_currentContent].Count - 1];
            Destroy(lastMessage);
            messagesByChat[_currentContent].RemoveAt(messagesByChat[_currentContent].Count - 1);
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
        foreach (GameObject message in messagesByChat[_currentContent])
        {
            Destroy(message);
        }

        messagesByChat[_currentContent].Clear();

        // Disable message interaction elements
        _deleteButtons.SetActive(false);
        DisableReactionPanel();
    }

    // Aktivoi globaalin chatin
    public void GlobalChatActive()
    {
        _currentContent = _globalChat;
        _currentScrollRect = _globalChatScrollRect;

        _globalChat.SetActive(true);
        _languageChat.SetActive(false);
        _clanChat.SetActive(false);

        Debug.Log("Global Chat aktivoitu");
    }

    // Aktivoi klaanichatin
    public void ClanChatActive()
    {
        _currentContent = _clanChat;
        _currentScrollRect = _clanChatScrollRect;

        _clanChat.SetActive(true);
        _languageChat.SetActive(false);
        _globalChat.SetActive(false);

        Debug.Log("Klaani Chat aktivoitu");
    }

    // Aktivoi kielichatin
    public void LanguageChatActive()
    {
        _currentContent = _languageChat;
        _currentScrollRect = _languageChatScrollRect;

        _languageChat.SetActive(true);
        _globalChat.SetActive(false);
        _clanChat.SetActive(false);

        Debug.Log("Kielivalinnan mukainen Chat aktivoitu");
    }

    public void OpenQuickMessages()
    {
        _quickMessages.SetActive(true);
        _InputArea.SetActive(false);
        CloseOnButtonClick(true);
    }

    /// <summary>
    /// Minimizes quick messages panel and send buttons. Last used send button is the one left visible.
    /// </summary>
    public void MinimizeOptions()
    {
        _quickMessages.SetActive(false);
        _InputArea.SetActive(true);

        // Deactivate all but last used button
        foreach (var button in _sendButtons)
        {
            button.SetActive(_lastSendButtonUsed == button);
        }
        _sendButtonsAreClosed = true;
    }

    /// <summary>
    /// Added to buttons to deselect messages and close the sending options
    /// </summary>
    /// <param name="onlyMessages"></param>
    public void CloseOnButtonClick(bool onlyDeselectMessages)
    {
        if (onlyDeselectMessages)
        {
            DeselectMessage(_selectedMessage);
        }
        else
        {
            DeselectMessage(_selectedMessage);
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

        if (reactionsField != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(reactionsField.GetComponent<RectTransform>());
        }

        VerticalLayoutGroup currentLayout = _currentContent.GetComponentInChildren<VerticalLayoutGroup>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(currentLayout.GetComponent<RectTransform>());
    }


    private void DisableReactionPanel()
    {
        _commonReactions.SetActive(true);
        _allReactions.SetActive(false);
        _addReactionsPanel.SetActive(false);
        _usersWhoAdded.SetActive(false);
    }
     

    public void OpenUsersWhoAddedReactionPanel()
    {
        _addReactionsPanel.SetActive(true);
        _commonReactions.SetActive(false);
        _allReactions.SetActive(false);
        _usersWhoAdded.SetActive(true);
        SetReactionPanelPosition();
    }

    public GameObject giveJoyPref()
    {
        return _messagePrefabYellow;
    }
}
