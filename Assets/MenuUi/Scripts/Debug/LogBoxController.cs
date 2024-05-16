using UnityEngine;
using UnityEngine.UI;
using DebugUi.Scripts.BattleAnalyzer;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;
using System.Collections.Generic;
using System.IO; // Lisätty käyttääksemme System.IO -kirjastoa tiedostojen käsittelyyn

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class LogBoxController : MonoBehaviour
    {
        [SerializeField] private GameObject _logTextObject;
        [SerializeField] private GameObject _logTextBox1;
        [SerializeField] private GameObject _logTextBox2;
        [SerializeField] private GameObject _logTextBox3;
        [SerializeField] private GameObject _logTextBox4;

        private MsgStorage msgStorage = new MsgStorage(4);

        // Reference to the content box RectTransform
        [SerializeField] private RectTransform _contentBoxRectTransform;

        // Reference to the vertical scrollbar (if applicable)
        [SerializeField] private Scrollbar _verticalScrollbar;

        // Reference to the panel opener
        [SerializeField] private MessagePanel _messagePanel;

        // Start is called before the first frame update
        void Start()
        {
            // Iterate over each message and add it to a random log box
            for (int i = 0; i < 50; i++)
            {
                // Generate a random client index (log box index)
                int clientIndex = UnityEngine.Random.Range(0, 4);

                // Add the message to the randomly selected log box
                AddMessageToLog("Info message", i, clientIndex, MessageType.Info);
                AddMessageToLog("Warning message", i, clientIndex, MessageType.Warning);
                AddMessageToLog("Error message", i, clientIndex, MessageType.Error);
            }

            // Update the log text to display all messages
            UpdateLogText();
        }

        // Add a message to the log box
        public void AddMessageToLog(string message, int time, int client, MessageType messageType)
        {
            msgStorage.Add(new MsgObject(client, time, message, messageType));
        }

        // Load log file and display its content
        public void LoadLogFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(filePath);

                // Add each line to the message storage
                foreach (string line in lines)
                {
                    // Add the line as a message to the log storage
                    AddMessageToLog(line, 0, 0, MessageType.Info);
                }

                // Update the log text to display all messages
                UpdateLogText();
            }
            else
            {
                Debug.LogError("File not found: " + filePath);
            }
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
                    var messages = msgStorage.AllMsgs(i);
                    var filteredMessages = MsgStorage.GetSubList(messages, (MessageTypeOptions)(MessageType.Info | MessageType.Warning | MessageType.Error));
                    foreach (var msg in filteredMessages)
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

        internal void MessageDeliver(IReadOnlyMsgObject msgObject)
        {
            string logText = $"[{msgObject.Client}:{msgObject.Time}] {msgObject.Msg}\n";
            _messagePanel.SetMessage(logText);
        }

        // Get the corresponding log text box GameObject based on the index
        private GameObject GetLogTextBox(int index)
        {
            switch (index)
            {
                case 1:
                    return _logTextBox1;
                case 2:
                    return _logTextBox2;
                case 3:
                    return _logTextBox3;
                case 4:
                    return _logTextBox4;
                default:
                    return null;
            }
        }
    }
}