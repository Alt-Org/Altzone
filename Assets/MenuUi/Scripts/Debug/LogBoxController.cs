using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    internal class LogBoxController : MonoBehaviour
    {
        [SerializeField] private GameObject _logTextObject;
        [SerializeField] private GameObject[] _logTextBoxArray;
        [SerializeField] private MessageTypeOptions _defaultMsgFilter;
        [SerializeField] private RectTransform _contentBoxRectTransform; // Reference to the content box RectTransform
        [SerializeField] private Scrollbar _verticalScrollbar;           // Reference to the vertical scrollbar (if applicable)
        [SerializeField] private MessagePanel _messagePanel;             // Reference to the vertical scrollbar (if applicable)
        [SerializeField] private DebugTimelineController _debugTimelineController;
        [SerializeField] private bool _generateTestLogs;

        public void SetMsgStorage(IReadOnlyMsgStorage msgStorage) { _msgStorage = msgStorage; UpdateLogText(); UpdateTimeline(); }

        public void SetMsgFilter(int client, MessageTypeOptions msgFilter)
        {
            MsgBox msgBox = _msgBoxArray[client];
            IReadOnlyList<IReadOnlyMsgObject> messages = _msgStorage.AllMsgs(client);

            msgBox.MsgFilter = msgFilter;
            FilterLog(msgBox, messages);
        }
        public void SetTimelineFilter(MessageTypeOptions msgFilter, bool includeEmpty, int client = 0)
        {
            _debugTimelineController.FilterTimeline(msgFilter, includeEmpty);
        }

        internal void MessageDeliver(IReadOnlyMsgObject msgObject)
        {
            //string logText = string.Format("[Client {0}] [{1:000000}] {2}", msgObject.Client, msgObject.Time, msgObject.Msg);
            //_messagePanel.SetMessage(logText);
        }

        private IReadOnlyMsgStorage _msgStorage;

        private class MsgBox
        {
            public GameObject LogTextBox { get; }
            public IReadOnlyList<GameObject> MsgBoxObjectList => _msgBoxObjectList;
            public MessageTypeOptions MsgFilter { get; set; }

            public MsgBox(GameObject logTextBox, MessageTypeOptions msgFilter)
            {
                LogTextBox = logTextBox;
                _msgBoxObjectList = new();
                MsgFilter = msgFilter;
            }

            public void Clear()
            {
                foreach (GameObject msgBoxObject in _msgBoxObjectList)
                {
                    Destroy(msgBoxObject);
                }
                _msgBoxObjectList.Clear();
            }

            public void AddMsg(IReadOnlyMsgObject msg, GameObject logTextObject, MessagePanel _messagePanel)
            {
                GameObject logMsgBox = Instantiate(logTextObject, LogTextBox.transform.GetChild(0).GetChild(0));
                logMsgBox.GetComponent<LogBoxMessageHandler>().Initialize(_messagePanel ,msg);
                _msgBoxObjectList.Add(logMsgBox);
            }

            private readonly List<GameObject> _msgBoxObjectList;
        }
        private MsgBox[] _msgBoxArray;

        // Start is called before the first frame update
        private void Start()
        {
            _msgBoxArray = new MsgBox[_logTextBoxArray.Length];
            for (int i = 0; i < _logTextBoxArray.Length; i++)
            {
                _msgBoxArray[i] = new(_logTextBoxArray[i], _defaultMsgFilter);
            }

            if (!_generateTestLogs) return;

            MsgStorage msgStorage = new(4);

            // Iterate over each message and add it to a random log box
            for (int i = 0; i < 50; i++)
            {
                // Generate a random client index (log box index)
                int clientIndex = UnityEngine.Random.Range(0, 4);

                // Add the message to the randomly selected log box
                AddMessageToLog(msgStorage, "Info message", i, clientIndex, MessageType.Info);
                AddMessageToLog(msgStorage, "Warning message", i, clientIndex, MessageType.Warning);
                AddMessageToLog(msgStorage, "Error message", i, clientIndex, MessageType.Error);
            }

            SetMsgStorage(msgStorage);
        }

        // Add a message to the log box
        private void AddMessageToLog(MsgStorage msgStorage, string message, int time, int client, MessageType messageType)
        {
            msgStorage.Add(new MsgObject(client, time, message, "", messageType));
        }

        // Update the log text to display all messages
        private void UpdateLogText()
        {
            foreach (MsgBox msgBox in _msgBoxArray) msgBox.Clear();

            // Loop through each log box
            for (int i = 0; i < _msgStorage.ClientCount; i++)
            {
                // Get the corresponding log text box GameObject
                MsgBox msgBox = _msgBoxArray[i];
                // Get all messages for the current log box index
                IReadOnlyList<IReadOnlyMsgObject> messages = _msgStorage.AllMsgs(i);

                //IReadOnlyList<IReadOnlyMsgObject> filteredMessages = MsgStorage.GetSubList(messages, (MessageTypeOptions)(MessageType.Info | MessageType.Warning | MessageType.Error));
                foreach (IReadOnlyMsgObject msg in messages)
                {
                    // Instantiate a new log message GameObject for each message
                    msgBox.AddMsg(msg, _logTextObject, _messagePanel);
                }

                FilterLog(msgBox, messages);
            }
        }

        private void FilterLog(MsgBox msgBox, IReadOnlyList<IReadOnlyMsgObject> messages)
        {
            for (int i = 0; i < msgBox.MsgBoxObjectList.Count; i++)
            {
                msgBox.MsgBoxObjectList[i].SetActive(messages[i].IsType(msgBox.MsgFilter));
            }
        }

        private void UpdateTimeline()
        {
            IReadOnlyTimelineStorage messages = _msgStorage.GetTimelineStorage();
            _debugTimelineController.SetTimeline(messages);
        }
    }
}
