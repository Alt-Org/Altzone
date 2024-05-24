using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using DebugUi.Scripts.BattleAnalyzer;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class OpenFilePanel : MonoBehaviour
    {
        public LogBoxController logBoxController;
        public GameObject filePanel;
        public Button confirmLogsButton;
        public Button denyLogsButton;
        public TextMeshProUGUI logText;

        private string selectedFilePath;

        // Instantiate LogBoxController in the Start method
        void Start()
        {
            logBoxController = GetComponentInParent<LogBoxController>();
            if (logBoxController == null)
            {
                Debug.LogError("LogBoxController not found in parent hierarchy.");
            }

            // Initially hide the buttons
            confirmLogsButton.gameObject.SetActive(false);
            denyLogsButton.gameObject.SetActive(false);
        }

        public void OnButtonClick()
        {
            // Open file managing window
            selectedFilePath = PanelOpener.OpenFileDialog();

            // Show the file's address and location in the LogViewer text box
            logText.text = selectedFilePath;

            // Show "BT_CONFIRMLOGS" and "BT_DENYLOGS" buttons
            confirmLogsButton.gameObject.SetActive(true);
            denyLogsButton.gameObject.SetActive(true);
        }

        public void ConfirmLogs()
        {
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                if (logBoxController != null)
                {
                    // Load log file contents
                    string[] lines = File.ReadAllLines(selectedFilePath);

                    // Pass log file contents to log box controller
                    foreach (string line in lines)
                    {
                        // For each line in the log file, add it to the log box controller
                        int clientIndex = UnityEngine.Random.Range(0, 4); // Generate random client index
                        MessageType messageType = MessageType.Info;
                        logBoxController.AddMessageToLog(line, (int)Time.time, clientIndex, messageType);
                    }
                }
                else
                {
                    Debug.LogError("LogBoxController reference is null.");
                }
            }
            else
            {
                Debug.LogError("No log file selected.");
            }

            // Hide the panel after confirming
            filePanel.SetActive(false);
        }

        public void DenyLogs()
        {
            // Reset the selected file path and clear the text
            selectedFilePath = "";
            logText.text = "";

            // Hide the buttons
            confirmLogsButton.gameObject.SetActive(false);
            denyLogsButton.gameObject.SetActive(false);
        }


        public class PanelOpener : MonoBehaviour
        {
            public static string OpenFileDialog()
            {
#if UNITY_EDITOR

                string path = UnityEditor.EditorUtility.OpenFilePanel("Select Log File", "", "txt");
                return path;
#else

            return null;

#endif
            }
        }
    }
}
