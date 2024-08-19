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

        public void OnButtonClick()
        {
            // Open file managing window
            _selectedFilePath = PanelOpener.OpenFileDialog();

            // Show the file's address and location in the file path text field
            _filePathTextField.text = _selectedFilePath;

            OnFilePathChange();
        }

        public void OnTextFieldEndEdit()
        {
            // Update selected file path
            _selectedFilePath = _filePathTextField.text;

            OnFilePathChange();
        }

        public void ConfirmLogs()
        {
            if (!string.IsNullOrEmpty(_selectedFilePath))
            {
                if (_logBoxController != null)
                {
                    // Load log file contents
                    string[] lines = File.ReadAllLines(_selectedFilePath);

                    //IReadOnlyMsgStorage msgStorage = //[sudo code] parser.parseLog(lines); //
                    //logBoxController.SetMsgStorage(msgStorage);
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
            _filePanel.SetActive(false);
        }

        public void DenyLogs()
        {
            // Reset the selected file path and clear the text
            _selectedFilePath = "";
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

                string path = UnityEditor.EditorUtility.OpenFilePanel("Select Log File", "", "txt");
                return path;
#else

            return null;

#endif
            }
        }

        private string _selectedFilePath;

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
            if (_selectedFilePath != "" && File.Exists(_selectedFilePath))
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
