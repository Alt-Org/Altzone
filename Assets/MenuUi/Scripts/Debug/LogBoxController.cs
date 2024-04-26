using UnityEngine;
using UnityEngine.UI;
using DebugUi.Scripts.BattleAnalyzer;
using TMPro;
using ExitGames.Client.Photon.StructWrapping;
using System.Collections.Generic;

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
    // Create a list of messages
    List<string> messages = new List<string>
    {
        "[PLAYER DRIVER PHOTON] (team: 1, pos: 1) Movement requested",
        "[PLAYER DRIVER STATE] (team: 1, pos: 0) State (movement enabled: True, is waiting to move: False, player actor is busy: False)",
        "[BALL HANDLER] Ball launched (position: (-2.58, 2.20), velocity: (-1.91, -4.62))"
    };

    // Shuffle the list of messages
    Shuffle(messages);

    // Iterate over each message and add it to a random log box
    for (int i = 0; i < 50; i++)
    {
        // Generate a random client index (log box index)
        int clientIndex = UnityEngine.Random.Range(0, 4);

        // Add the message to the randomly selected log box
        AddMessageToLog(messages[i % messages.Count], i, clientIndex);
    }

    // Update the log text to display all messages
    UpdateLogText();
}

// Add a message to the log box
public void AddMessageToLog(string message, int time, int client)
{
    msgStorage.Add(new MsgObject(client, time, message, MessageType.Info));
}

// Shuffle a list
private void Shuffle<T>(List<T> list)
{
    for (int i = 0; i < list.Count; i++)
    {
        int randomIndex = UnityEngine.Random.Range(i, list.Count);
        T temp = list[randomIndex];
        list[randomIndex] = list[i];
        list[i] = temp;
    }
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
