using System;
using System.Collections;
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

        public void SetMsgStorage(IReadOnlyMsgStorage msgStorage) { _msgStorage = msgStorage; StartCoroutine(UpdateLogText()); UpdateTimeline(); }

        public void SetMsgTypeFilter(int client, MessageTypeOptions msgTypeFilter)
        {
            MsgBox msgBox = _msgBoxArray[client];
            IReadOnlyList<IReadOnlyMsgObject> messages = _msgStorage.AllMsgs(client);

            msgBox.MsgTypeFilter = msgTypeFilter;
            FilterLog(msgBox, messages);
        }

        public void SetMsgSourceFilter(int client, int msgSourceFilter)
        {
            MsgBox msgBox = _msgBoxArray[client];
            IReadOnlyList<IReadOnlyMsgObject> messages = _msgStorage.AllMsgs(client);

            msgBox.MsgSourceFilter = msgSourceFilter;
            FilterLog(msgBox, messages);
        }

        public void SetTimelineFilter(MessageTypeOptions msgFilter, bool includeEmpty, int client = 0)
        {
            _debugTimelineController.FilterTimeline(msgFilter, includeEmpty);
        }

        internal void SetTimelinePosition(int time)
        {
            //string logText = string.Format("[Client {0}] [{1:000000}] {2}", msgObject.Client, msgObject.Time, msgObject.Msg);
            //_messagePanel.SetMessage(logText);
            _debugTimelineController.SetPosition(time);
        }

        internal void SetDiffMode(bool value)
        {
            _diffMode = value;
            for (int i = 0; i<_msgBoxArray.Length;i++)
            {
                MsgBox msgBox = _msgBoxArray[i];
                IReadOnlyList<IReadOnlyMsgObject> messages = _msgStorage.AllMsgs(i);
                FilterLog(msgBox, messages);
            }
            _messagePanel.SetDiffMode(_diffMode);
        }

        private IReadOnlyMsgStorage _msgStorage;
        private bool _diffMode = true;

        private class MsgBox
        {
            public GameObject LogTextBox { get; }
            public IReadOnlyList<GameObject> MsgBoxObjectList => _msgBoxObjectList;
            public MessageTypeOptions MsgTypeFilter { get; set; }
            public int MsgSourceFilter { get; set; }

            private DebugSourceFilterHandler _sourceFilterHandler;

            public MsgBox(GameObject logTextBox, MessageTypeOptions msgFilter)
            {
                LogTextBox = logTextBox;
                MsgTypeFilter = msgFilter;
                MsgSourceFilter = 0;
                _msgBoxObjectList = new();
                _sourceFilterHandler = LogTextBox.transform.Find("SourceFilter").GetComponent<DebugSourceFilterHandler>();
            }

            public void Clear()
            {
                foreach (GameObject msgBoxObject in _msgBoxObjectList)
                {
                    Destroy(msgBoxObject);
                }
                _msgBoxObjectList.Clear();
            }

            public void AddMsg(IReadOnlyMsgObject msg, GameObject logTextObject, MessagePanel _messagePanel, IReadOnlyMsgStorage msgStorage)
            {
                GameObject logMsgBox = Instantiate(logTextObject, LogTextBox.transform.GetChild(0).GetChild(0));
                logMsgBox.GetComponent<LogBoxMessageHandler>().Initialize(_messagePanel ,msg, msgStorage);
                _msgBoxObjectList.Add(logMsgBox);
            }

            public void SetSourceFilter(int client, IReadOnlyMsgStorage msgStorage, Action<int, int> setMsgSourceFilter)
            {
                _sourceFilterHandler.SetInitialLogBoxFilters(client, msgStorage, MsgSourceFilter, setMsgSourceFilter);
            }

            private readonly List<GameObject> _msgBoxObjectList;
        }
        private MsgBox[] _msgBoxArray;

        public bool DiffMode { get => _diffMode;}

        // Start is called before the first frame update
        private void Start()
        {
            _msgBoxArray = new MsgBox[_logTextBoxArray.Length];
            for (int i = 0; i < _logTextBoxArray.Length; i++)
            {
                _msgBoxArray[i] = new(_logTextBoxArray[i], _defaultMsgFilter);
            }
            InitializeTimeline();

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
            msgStorage.Add(new MsgObject(client, time, message, 1, "", messageType, false));
        }

        // Update the log text to display all messages
        private IEnumerator UpdateLogText()
        {
            long prevYield = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (MsgBox msgBox in _msgBoxArray)
            {
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - prevYield > 100)
                {
                    prevYield = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    Debug.LogWarning("Yield1");
                    yield return null;
                }
                msgBox.Clear();
            }

            // Loop through each log box
            for (int i = 0; i < _msgStorage.ClientCount; i++)
            {
                // Get the corresponding log text box GameObject
                MsgBox msgBox = _msgBoxArray[i];
                // Get all messages for the current log box index
                IReadOnlyList<IReadOnlyMsgObject> messages = _msgStorage.AllMsgs(i);

                msgBox.MsgSourceFilter = _msgStorage.GetSourceAllFlags();

                //IReadOnlyList<IReadOnlyMsgObject> filteredMessages = MsgStorage.GetSubList(messages, (MessageTypeOptions)(MessageType.Info | MessageType.Warning | MessageType.Error));
                foreach (IReadOnlyMsgObject msg in messages)
                {
                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - prevYield > 100)
                    {
                        prevYield = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        Debug.LogWarning("Yield2");
                        yield return null;
                    }
                    // Instantiate a new log message GameObject for each message
                    msgBox.AddMsg(msg, _logTextObject, _messagePanel, _msgStorage);
                }
                msgBox.SetSourceFilter(i, _msgStorage, SetMsgSourceFilter);
                FilterLog(msgBox, messages);
            }
        }

        private void FilterLog(MsgBox msgBox, IReadOnlyList<IReadOnlyMsgObject> messages)
        {
            for (int i = 0; i < msgBox.MsgBoxObjectList.Count; i++)
            {
                bool typeBool = messages[i].IsType(msgBox.MsgTypeFilter);
                bool sourceBool = messages[i].IsFromSource(msgBox.MsgSourceFilter);
                msgBox.MsgBoxObjectList[i].SetActive(typeBool && sourceBool);
                msgBox.MsgBoxObjectList[i].GetComponent<LogBoxMessageHandler>().SetDiffMode(_diffMode);
            }
        }

        private void InitializeTimeline()
        {
            _debugTimelineController.FilterTimeline(_defaultMsgFilter, false);
            _debugTimelineController.Initialize(SetLogPosition);
        }

        private void UpdateTimeline()
        {
            _debugTimelineController.SetTimeline(_msgStorage);
        }

        private void SetLogPosition(int[] values)
        {
            for(int i = 0; i < _msgBoxArray.Length ; i++)
            {
                if (values[i] < 0) continue;
                float value = (float)values[i]/(float)_msgBoxArray[i].MsgBoxObjectList.Count;
                if(_msgBoxArray[i].MsgBoxObjectList[values[i]].GetComponent<LogBoxMessageHandler>().MsgObject.Id == values[i])
                {
                    float boxPosition = 0;
                    if (!_msgBoxArray[i].MsgBoxObjectList[values[i]].activeSelf)
                    {
                        int j = 1;
                        do
                        {
                            Debug.LogWarning($"{j}. Pos: {_msgBoxArray[i].MsgBoxObjectList[values[i]].transform.localPosition.y}");
                            if (_msgBoxArray[i].MsgBoxObjectList[values[i] - j].activeSelf)
                            {
                                boxPosition = Mathf.Abs(_msgBoxArray[i].MsgBoxObjectList[values[i]-j].transform.localPosition.y);
                                break;
                            }
                            j++;
                        } while (values[i] - j >= 0);
                    }
                    else
                    {
                        boxPosition = Mathf.Abs(_msgBoxArray[i].MsgBoxObjectList[values[i]].transform.localPosition.y);
                    }
                    float contentHeight = _msgBoxArray[i].MsgBoxObjectList[0].transform.parent.GetComponent<RectTransform>().rect.height;
                    float viewHeight = _msgBoxArray[i].MsgBoxObjectList[0].transform.parent.parent.GetComponent<RectTransform>().rect.height;
                    value = boxPosition/(contentHeight-viewHeight);
                }
                _msgBoxArray[i].MsgBoxObjectList[0].transform.parent.parent.parent.GetComponent<ScrollRect>().verticalScrollbar.value = 1-value;
            }
        }
    }
}
