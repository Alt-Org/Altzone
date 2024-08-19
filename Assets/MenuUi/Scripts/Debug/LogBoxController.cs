using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    internal class LogBoxController : MonoBehaviour
    {
        [SerializeField] private GameObject _logTextObject;
        [SerializeField] private GameObject _logTextBox1;
        [SerializeField] private GameObject _logTextBox2;
        [SerializeField] private GameObject _logTextBox3;
        [SerializeField] private GameObject _logTextBox4;
        [SerializeField] private RectTransform _contentBoxRectTransform; // Reference to the content box RectTransform
        [SerializeField] private Scrollbar _verticalScrollbar;           // Reference to the vertical scrollbar (if applicable)
        [SerializeField] private MessagePanel _messagePanel;             // Reference to the vertical scrollbar (if applicable)
        [SerializeField] private bool _generateTestLogs;

        public void SetMsgStorage(IReadOnlyMsgStorage msgStorage) { _msgStorage = msgStorage; UpdateLogText(); }

        internal void MessageDeliver(IReadOnlyMsgObject msgObject)
        {
            string logText = $"[{msgObject.Client}:{msgObject.Time}] {msgObject.Msg}\n";
            _messagePanel.SetMessage(logText);
        }

        private IReadOnlyMsgStorage _msgStorage;

        // Start is called before the first frame update
        private void Start()
        {
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
            msgStorage.Add(new MsgObject(client, time, message, messageType));
        }

        // Update the log text to display all messages
        private void UpdateLogText()
        {
            // Loop through each log box
            for (int i = 0; i < 4; i++)
            {
                // Get the corresponding log text box GameObject
                GameObject logTextBox = GetLogTextBox(i + 1);

                // Check if the logTextBox is not null
                if (logTextBox != null)
                {
                    // Get all messages for the current log box index
                    IReadOnlyList<IReadOnlyMsgObject> messages = _msgStorage.AllMsgs(i);
                    IReadOnlyList<IReadOnlyMsgObject> filteredMessages = MsgStorage.GetSubList(messages, (MessageTypeOptions)(MessageType.Info | MessageType.Warning | MessageType.Error));
                    foreach (IReadOnlyMsgObject msg in filteredMessages)
                    {
                        // Instantiate a new log message GameObject for each message
                        GameObject logMsgBox = Instantiate(_logTextObject, logTextBox.transform.GetChild(0).GetChild(0));
                        string logText = $"[{msg.Client}:{msg.Time}] {msg.Msg}\n"; // Message format
                        logMsgBox.GetComponent<LogBoxMessageHandler>().SetMessage(msg);
                    }
                }
                else
                {
                    Debug.LogError($"Log text box {_logTextBox1.name} not found.");
                }
            }
        }

        // Get the corresponding log text box GameObject based on the index
        private GameObject GetLogTextBox(int index)
        {
            return index switch
            {
                1 => _logTextBox1,
                2 => _logTextBox2,
                3 => _logTextBox3,
                4 => _logTextBox4,
                _ => null,
            };
        }
    }
}
