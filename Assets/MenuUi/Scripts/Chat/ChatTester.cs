using System;
using UnityEngine;
using Random = UnityEngine.Random;


/// <summary>
/// Sends random messages to chat for testing purposes.
/// </summary>
public class ChatTester : MonoBehaviour
{
    //
    // THIS SCRIPT HAS NOT BEEN UPDATED IN A WHILE AND MAY NOT WORK!
    //
    public static ChatTester Instance;

    [Tooltip("ChatTester can be activated/stopped by pressing z and x keys simultaneously on keyboard")] public bool isActive;
    public bool isRunning;

    private float _currentTime;
    private float _messageSendTime;

    [Header("Timer settings")]
    [SerializeField] private float _minTimeBetweenMessages;
    [SerializeField] private float _maxTimeBetweenMessages;

    [Header("Message settings")]
    [SerializeField] private int _minWordsInMessage = 0;
    [SerializeField] private int _maxWordsInMessage = 0;

    private static string[] _randomWords ={"shiver","aboard","intelligent","snotty","free","cherry","hug","needle","synonymous",
        "alive","loss","sea","mark","plantation","tacky","shy","daily","absurd","delight","poised","erratic","suggest",
        "juvenile","angle","welcome","to","and","or", "by", "the", "for", "car", "eat", "sleep", "exercise", "comedy", "tall", "eye", "end", "sheep", "police", "sad", "misplaced",
        "bacon", "jump", "cold", "moist", "yawn", "over"};


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    void Start()
    {
        _messageSendTime = Random.Range(_minTimeBetweenMessages, _maxTimeBetweenMessages);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Z) && Input.GetKeyDown(KeyCode.X))
            isActive = !isActive;

        if (isActive && !isRunning)
        {
            Debug.Log("Running ChatTester!");
            isRunning = true;
        }
        else if (!isActive && isRunning)
        {
            Debug.Log("Stopping ChatTester!");
            isRunning = false;
        }

        if (isActive)
        {
            if (_currentTime >= _messageSendTime)
            {
                string message = GenerateRandomMessage();
                SendRandomMessage(message);
                _currentTime = 0;
                _messageSendTime = Random.Range(_minTimeBetweenMessages, _maxTimeBetweenMessages);
            }

            _currentTime += Time.deltaTime;
        }
    }

    internal void Toggle(bool value)
    {
        isActive = value;
    }

    private string GenerateRandomMessage()
    {
        string message = "";
        int wordsInMessage = Random.Range(_minWordsInMessage, _maxWordsInMessage + 1);

        for (int i = 0; i <= wordsInMessage; i++)
            message += _randomWords[Random.Range(0, _randomWords.Length)] + " ";

        return message.TrimEnd(' ');
    }

    private void SendRandomMessage(string message)
    {
        if (ChatListener.Instance.ChatClient.CanChat)
        {
            ChatChannel chatChannel = ChatListener.Instance._chatChannels[Random.Range(0, ChatListener.Instance._chatChannels.Length)];
            object[] dataToSend;
            dataToSend = new object[] {ChatListener.Instance._username, message, Random.Range(0, Enum.GetValues(typeof(ChatListener.Mood)).Length - 1), chatChannel._chatChannelType };
            ChatListener.Instance.ChatClient.PublishMessage(chatChannel._channelName, dataToSend, true);
        }
    }
}
