
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

        // Start is called before the first frame update
        void Start()
        {
            AddMessageToLog("This is a test message.", 0); //i
        }

        // Add a message to the log box
        public void AddMessageToLog(string message, int time)
        {
            msgStorage.Add(new MsgObject(time, time, message, MessageType.Info));
            UpdateLogText();
        }

        // Update the log text to display all messages
        private void UpdateLogText()
        {
            foreach (var msg in msgStorage.AllMsgs(0))
            {
                    GameObject logMsgBox = Instantiate(_logTextObject, _logTextBox1.transform.GetChild(0).GetChild(0));
                    string logText = $"[{msg.Client}:{msg.Time}] {msg.Msg}\n"; // Message format
                    logMsgBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = logText;
            }
        }
    }
}
