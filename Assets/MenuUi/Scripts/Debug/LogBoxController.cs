using UnityEngine;
using UnityEngine.UI;
using DebugUi.Scripts.BattleAnalyzer;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class LogBoxController : MonoBehaviour
    {
        public Text logText;
        private MsgStorage msgStorage = new MsgStorage();

        // Start is called before the first frame update
        void Start()
        {
            AddMessageToLog("This is a test message.");
        }

        // Add a message to the log box
        public void AddMessageToLog(string message)
        {
            msgStorage.Add(new MsgObject(0, 0, message, MessageType.Info));
            UpdateLogText();
        }

        // Update the log text to display all messages
        private void UpdateLogText()
        {
            string log = "";
            foreach (var timestamp in msgStorage.GetTimelineStorage().GetTimeline(0))
            {
                foreach (var msg in timestamp.List)
                {
                    log += $"[{msg.Client}:{msg.Time}] {msg.Msg}\n"; // Message format
                }
            }
            logText.text = log;
        }
    }
}
