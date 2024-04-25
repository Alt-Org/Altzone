using UnityEngine;
using UnityEngine.UI;
using DebugUi.Scripts.BattleAnalyzer;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;

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

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < 30; i++)
            {
                AddMessageToLog("This is a test message.", i, 0);
                AddMessageToLog("This is a test message aswell", i, 1);
                AddMessageToLog("This may be a test message aswell", i, 2);
                AddMessageToLog("This might not be a test message", i, 3);
            }
            UpdateLogText();
        }

        // Add a message to the log box
        public void AddMessageToLog(string message, int time, int client)
        {
            msgStorage.Add(new MsgObject(client, time, message, MessageType.Info));
            UpdateContentBoxHeight();
        }

        // Update the height of the content box to match the total height of messages
        private void UpdateContentBoxHeight()
        {
            float totalHeight = CalculateTotalHeightOfMessages();
            _contentBoxRectTransform.sizeDelta = new Vector2(_contentBoxRectTransform.sizeDelta.x, totalHeight);
        }

        // Calculate the total height required to display all messages
        private float CalculateTotalHeightOfMessages()
        {
            // Assuming each message has a fixed height (adjust as needed)
            float messageHeight = 50f; // Adjust this value based on your UI design
            int totalMessages = msgStorage.TotalMessages(); // Implement TotalMessages() method in MsgStorage class

            return totalMessages * messageHeight;
        }

        // Update the log text to display all messages
        private void UpdateLogText()
        {
            // Loop through each log box
            for (int i = 0; i < 4; i++)
            {
                // Get the corresponding log text box GameObject
                GameObject logTextBox = GetLogTextBox(i + 1); // Implement GetLogTextBox method

                // Check if the logTextBox is not null
                if (logTextBox != null)
                {
                    // Get all messages for the current log box index
                    foreach (var msg in msgStorage.AllMsgs(i))
                    {
                        // Instantiate a new log message GameObject for each message
                        GameObject logMsgBox = Instantiate(_logTextObject, logTextBox.transform.GetChild(0).GetChild(0));
                        string logText = $"[{msg.Client}:{msg.Time}] {msg.Msg}\n"; // Message format
                        logMsgBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = logText;
                    }
                }
                else
                {
                    Debug.LogError($"Log text box {_logTextBox1.name} not found.");
                }
            }

            UpdateContentBoxHeight(); // Ensure the content box height is updated after adding messages
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