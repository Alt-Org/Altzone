using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using MenuUi.Scripts.TabLine;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Common;
using Altzone.Scripts.Chat;

public class Chat : AltMonoBehaviour
{
    [Header("Chat")]
    [SerializeField] private GameObject _languageChat;
    [SerializeField] private GameObject _languageChatContent;
    [SerializeField] private GameObject _globalChat;
    [SerializeField] private GameObject _globalChatContent;
    [SerializeField] private GameObject _clanChat;
    [SerializeField] private GameObject _clanChatContent;
    private GameObject _currentContent; // Tällä hetkellä aktiivinen chatin content

    [Header("Send buttons")]
    [SerializeField] private GameObject _sendButtonUI;
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

    [Header("Other Prefab")]
    [SerializeField] private GameObject[] _otherMessages;


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

    private MessageObjectHandler _selectedMessage; // Viesti, joka on tällä hetkellä valittuna

    // Public getter
    public MessageObjectHandler SelectedMessage => _selectedMessage;  

    // Commands
    private string _delete = "/deleteMessage";
    private string _deleteAllMessages = "/clear";

    // Sanakirja (List), jossa viestit järjestetään chat-tyypin mukaan
    private Dictionary<GameObject, List<MessageObjectHandler>> messagesByChat = new Dictionary<GameObject, List<MessageObjectHandler>>();

    private GameObject _lastSendButtonUsed;
    private bool _sendButtonsAreClosed = true;

    [SerializeField] private GameObject _InputArea;

    public delegate void SelectedMessageChanged(MessageObjectHandler handler);
    public static event SelectedMessageChanged OnSelectedMessageChanged;
    private bool _reactionAvailable = false; //Katsoo jos textboxissa on tekstiä tai ei
    public static Chat instance;
    public CharacterResponseList characterResponseList;
    public Mood currentMood = Mood.Neutral;
    public GameObject _responsesData;

    private void Start()
    {
        ChatChannel.OnMessageHistoryReceived += RefreshChat;
        ChatChannel.OnMessageReceived += DisplayMessage;

        // Alustaa chatit ja asettaa kielichatin oletukseksi
        _currentContent = _clanChat;
        Debug.Log("Clan Chat is Active");

        messagesByChat[_languageChatContent] = new List<MessageObjectHandler>();
        messagesByChat[_globalChatContent] = new List<MessageObjectHandler>();
        messagesByChat[_clanChatContent] = new List<MessageObjectHandler>();

        ClanChatActive();
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

    private void OnDestroy()
    {
        ChatChannel.OnMessageHistoryReceived -= RefreshChat;
        ChatChannel.OnMessageReceived -= DisplayMessage;
    }

    private void AddResponses()
    {
        //Clears the current Responses
        if(_responsesData.transform.childCount > 0)
         foreach (Transform child in _responsesData.transform)
         {
             
             Destroy(child.gameObject);
         }

        Debug.LogWarning("FIND ME THE MOODS VERSIONS");
        StartCoroutine(GetPlayerData(data =>
        {
            List<string> messageList = _chatResponseList.GetChatResponses(currentMood);
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
            _reactionAvailable = false;
            _lastSendButtonUsed = buttonUsed;

            //Prob should have better place for AddResponses when changing the message options but this will do for now

            // Check which message prefab should be used
            if(buttonUsed == _sendButtonSadness)
            {
                currentMood = Mood.Sad;

                SendChatMessage(Mood.Sad);
                gameObject.GetComponent<UseAllChatFeelings>().FeelingUsed(UseAllChatFeelings.Feeling.Sadness);
            }
            else if (buttonUsed == _sendButtonAnger)
            {
                currentMood = Mood.Angry;
                SendChatMessage(Mood.Angry);
                gameObject.GetComponent<UseAllChatFeelings>().FeelingUsed(UseAllChatFeelings.Feeling.Anger);
            }
            else if (buttonUsed == _sendButtonJoy)
            {
                currentMood = Mood.Happy;
                SendChatMessage(Mood.Happy);
                gameObject.GetComponent<UseAllChatFeelings>().FeelingUsed(UseAllChatFeelings.Feeling.Joy);
            }
            else if (buttonUsed == _sendButtonPlayful)
            {
                currentMood = Mood.Wink;
                SendChatMessage(Mood.Wink);
                gameObject.GetComponent<UseAllChatFeelings>().FeelingUsed(UseAllChatFeelings.Feeling.Playful);
            }
            else if (buttonUsed == _sendButtonLove)
            {
                currentMood = Mood.Love;
                SendChatMessage(Mood.Love);
                gameObject.GetComponent<UseAllChatFeelings>().FeelingUsed(UseAllChatFeelings.Feeling.Love);
            }
            AddResponses();
        }
    }

    private GameObject GetMessagePrefab(Mood mood, bool ownMsg)
    {
        if (ownMsg)
        {
            return mood switch
            {
                Mood.Love => _messagePrefabPink,
                Mood.Happy => _messagePrefabYellow,
                Mood.Sad => _messagePrefabBlue,
                Mood.Wink => _messagePrefabOrange,
                Mood.Angry => _messagePrefabRed,
                _ => null,
            };
        }
        else
        {
            return mood switch
            {
                Mood.Love => _otherMessages[4],
                Mood.Happy => _otherMessages[2],
                Mood.Sad => _otherMessages[0],
                Mood.Wink => _otherMessages[3],
                Mood.Angry => _otherMessages[1],
                _ => null,
            };
        }
    }

    public void SendChatMessage(Mood mood)
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
            ChatListener.Instance.SendMessage(_inputField.text, mood, ChatListener.Instance.ActiveChatChannel);
            //DisplayMessage(_inputField.text, GetMessagePrefab(mood, true));
            _inputField.text = "";
            GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
            if (_currentContent == _clanChat)
                _clanChat.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
            if (_currentContent == _globalChat)
                _globalChat.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
            MinimizeOptions();
            _lastSendButtonUsed.GetComponent<Button>().interactable = false;
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
            _reactionAvailable = true;
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
    private void RefreshChat(ChatChannelType chatChannelType) => StartCoroutine(RefreshChatCoroutine(chatChannelType));
    private IEnumerator RefreshChatCoroutine(ChatChannelType chatChannelType)
    {
        if (chatChannelType is ChatChannelType.Global)
            yield return new WaitUntil(() => ChatListener.Instance.GlobalChatFetched);
        if (chatChannelType is ChatChannelType.Clan)
            yield return new WaitUntil(()=> ChatListener.Instance.ClanChatFetched);
        if (gameObject.activeSelf)
        {
            DeleteAllMessages();
            List<ChatMessage> messageList = ChatListener.Instance.GetChatChannel(chatChannelType).ChatMessages;
            if(messageList != null)
            foreach(ChatMessage message in messageList)
            {
                bool ownMsg = message?.SenderId == ServerManager.Instance.Player._id;
                DisplayMessage(message, GetMessagePrefab(message.Mood, ownMsg));
            }
        }
    }

    private void DisplayMessage(ChatChannelType channelType,ChatMessage message)
    {
        Debug.LogWarning($"Test1: {channelType} {ChatListener.Instance.ActiveChatChannel}");
        if (channelType != ChatListener.Instance.ActiveChatChannel) return;
        Debug.LogWarning("Test2");
        bool ownMsg = message?.SenderId == ServerManager.Instance.Player._id;
        GameObject messagePrefab = GetMessagePrefab(message.Mood, ownMsg);

        DisplayMessage(message, messagePrefab);
    }

    // Näyttää viestin aktiivisessa chatti-ikkunassa
    public void DisplayMessage(ChatMessage message, GameObject messagePrefab)
    {
        if (messagePrefab != null)
        {
            GameObject newMessage = Instantiate(messagePrefab, _currentContent.transform);

            newMessage.GetComponent<MessageObjectHandler>().SetMessageInfo(message, SelectMessage);

            //AddMessageInteraction(newMessage);

            messagesByChat[_currentContent].Add(newMessage.GetComponent<MessageObjectHandler>());

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

        //button.onClick.AddListener(() => SelectMessage(message));
        button.onClick.AddListener(() => MinimizeOptions());
    }

    // Valitsee viestin
    public void SelectMessage(MessageObjectHandler handler)
    {
        _selectedMessage = handler;
        OnSelectedMessageChanged?.Invoke(_selectedMessage);
        MinimizeOptions();
    }

    // Poistaa valinnan viestistä
    public void DeselectMessage()
    {
        _selectedMessage = null;
        OnSelectedMessageChanged?.Invoke(null);
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
            MessageObjectHandler lastMessage = messagesByChat[_currentContent][messagesByChat[_currentContent].Count - 1];
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
        foreach (MessageObjectHandler message in messagesByChat[_currentContent])
        {
            Destroy(message.gameObject);
        }

        messagesByChat[_currentContent].Clear();

        DisableReactionPanel();
    }

    // Aktivoi globaalin chatin
    public void GlobalChatActive()
    {
        _currentContent = _globalChatContent;
        ChatListener.Instance.ActiveChatChannel = ChatChannelType.Global;
        _currentScrollRect = _globalChatScrollRect;

        _globalChat.SetActive(true);
        _languageChat.SetActive(false);
        _clanChat.SetActive(false);

        gameObject.GetComponent<FindAllChatOptions>().ChatOptionFound(FindAllChatOptions.ChatType.Global);
        RefreshChat(ChatChannelType.Global);

        Debug.Log("Global Chat aktivoitu");
    }

    // Aktivoi klaanichatin
    public void ClanChatActive()
    {
        _currentContent = _clanChatContent;
        ChatListener.Instance.ActiveChatChannel = ChatChannelType.Clan;
        _currentScrollRect = _clanChatScrollRect;

        _clanChat.SetActive(true);
        _languageChat.SetActive(false);
        _globalChat.SetActive(false);

        gameObject.GetComponent<FindAllChatOptions>().ChatOptionFound(FindAllChatOptions.ChatType.Clan);
        RefreshChat(ChatChannelType.Clan);
        
        Debug.Log("Klaani Chat aktivoitu");
    }

    // Aktivoi kielichatin
    public void LanguageChatActive()
    {
        _currentContent = _languageChatContent;
        ChatListener.Instance.ActiveChatChannel = ChatChannelType.Country;
        _currentScrollRect = _languageChatScrollRect;

        _languageChat.SetActive(true);
        _globalChat.SetActive(false);
        _clanChat.SetActive(false);

        gameObject.GetComponent<FindAllChatOptions>().ChatOptionFound(FindAllChatOptions.ChatType.Language);

        Debug.Log("Kielivalinnan mukainen Chat aktivoitu");
    }

    public void OpenQuickMessages()
    {
        _quickMessages.SetActive(true);
        ///This is so that the reaction UI will stay visiable but not useable
        foreach(Transform child in _InputArea.transform)
        {
            if(child.gameObject == _sendButtonUI)
            {
            _lastSendButtonUsed.GetComponent<Button>().interactable = false;

                ///Incase if the user happens to have emotion selection on 
                foreach (var button in _sendButtons)
                {
                    button.SetActive(_lastSendButtonUsed == button);
                }
                _sendButtonsAreClosed = true;

                continue;
            }


            child.gameObject.SetActive(false);
        }
        CloseOnButtonClick(true);
    }

    /// <summary>
    /// Minimizes quick messages panel and send buttons. Last used send button is the one left visible.
    /// </summary>
    public void MinimizeOptions()
    {
        _quickMessages.SetActive(false);
        ///This is so that the reaction comes back being useable
        ///To give user a visual interpretation that they cant put a reaction till there's been text inserted
        foreach (Transform child in _InputArea.transform)
        {
            //Checks if there's text in textbox
            if (child.gameObject == _sendButtonUI)
            {
            _lastSendButtonUsed.GetComponent<Button>().interactable = true;
            continue;
            }

            child.gameObject.SetActive(true);
        }

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
        DeselectMessage();
        if (!onlyDeselectMessages)
        {
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
    }

    //public GameObject giveJoyPref()
    //{
    //    return _messagePrefabYellow;
    //}


}
