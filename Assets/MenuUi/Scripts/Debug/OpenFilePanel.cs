using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DebugUi.Scripts.BattleAnalyzer
{
    internal class OpenFilePanel : MonoBehaviour
    {
        [SerializeField] private LogBoxController _logBoxController;
        [SerializeField] private GameObject _filePanel;
        [SerializeField] private Button _confirmLogsButton;
        [SerializeField] private Button _denyLogsButton;
        [SerializeField] private TMP_InputField _filePathTextField;
        [SerializeField] private TMP_Dropdown _clientSelector;

        public void OnButtonClick()
        {
            // Open file managing window
            _selectedFilePath[_clientSelector.value] = PanelOpener.OpenFileDialog();

            // Show the file's address and location in the file path text field
            _filePathTextField.text = _selectedFilePath[_clientSelector.value];

            OnFilePathChange();
        }

        public void OnTextFieldEndEdit()
        {
            // Update selected file path
            _selectedFilePath[_clientSelector.value] = _filePathTextField.text;

            OnFilePathChange();
        }

        public void ConfirmLogs()
        {
            if (_logBoxController != null)
            {
                bool foundlogs = false;
                string[][] logs = new string[_selectedFilePath.Length][];
                for (int i = 0; i < _selectedFilePath.Length; i++)
                {
                    if (!string.IsNullOrEmpty(_selectedFilePath[i]))
                    {
                        // Load log file contents
                        logs[i] = File.ReadAllLines(_selectedFilePath[i]);

                        foundlogs = true;
                    }
                    else
                    {
                        logs[i] = new string[0];
                    }
                }
                if(!foundlogs)
                {
                    Debug.Log("No log file selected.");
                    return;
                }

                IReadOnlyMsgStorage msgStorage = BattleLogParser.ParseLogs(logs);
                _logBoxController.SetMsgStorage(msgStorage);

                // Hide the panel after confirming
                _filePanel.SetActive(false);
            }
            else
            {
                Debug.LogError("LogBoxController reference is null.");
            }
        }

        public void DenyLogs()
        {
            // Reset the selected file path and clear the text
            for (int i = 0; i < _selectedFilePath.Length; i++)
            {
                _selectedFilePath[i] = "";
            }
            _filePathTextField.text = "";

            // Hide the buttons
            _confirmLogsButton.gameObject.SetActive(false);
            _denyLogsButton.gameObject.SetActive(false);
        }


        public class PanelOpener : MonoBehaviour
        {
            public static string OpenFileDialog()
            {
#if UNITY_EDITOR

                string path = UnityEditor.EditorUtility.OpenFilePanel("Select Log File", "", "log");
                return path;
#else

            return null;

#endif
            }
        }

        private string[] _selectedFilePath = new string[4];

        // Instantiate LogBoxController in the Start method
        private void Start()
        {
            _logBoxController = GetComponentInParent<LogBoxController>();
            if (_logBoxController == null)
            {
                Debug.LogError("LogBoxController not found in parent hierarchy.");
            }

            // Initially hide the buttons
            _confirmLogsButton.gameObject.SetActive(false);
            _denyLogsButton.gameObject.SetActive(false);
        }

        private void OnFilePathChange()
        {
            if (_selectedFilePath[_clientSelector.value] != "" && File.Exists(_selectedFilePath[_clientSelector.value]))
            {
                // Show "BT_CONFIRMLOGS" and "BT_DENYLOGS" buttons
                _confirmLogsButton.gameObject.SetActive(true);
                _denyLogsButton.gameObject.SetActive(true);
            }
            else
            {
                // Hide the buttons
                _confirmLogsButton.gameObject.SetActive(false);
                _denyLogsButton.gameObject.SetActive(false);
            }
        }
    }
}
