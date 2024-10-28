using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace DebugUi.Scripts.BattleAnalyzer
{
    internal class OpenFilePanel : MonoBehaviour
    {
        [SerializeField] private LogBoxController _logBoxController;
        [SerializeField] private GameObject _filePanel;
        [SerializeField] private Button _addLogButton;
        [SerializeField] private Button _confirmLogsButton;
        [SerializeField] private Button _denyLogsButton;
        [SerializeField] private TMP_InputField _filePathTextField;
        [SerializeField] private TMP_Dropdown _clientSelector;
        [SerializeField] private DebugLogFileStoreHandler _fileStoreHandler;

        private void OnEnable()
        {
            DebugLogStoreField.OnAddressDeleted += OnFilePathChange;
        }
        private void OnDisable()
        {
            DebugLogStoreField.OnAddressDeleted -= OnFilePathChange;
        }

        public void OnButtonClick()
        {
            // Open file managing window
            if (File.Exists(_filePathTextField.text))
            {
                if(_clientSelector.value == 0)
                    _fileStoreHandler.SetLogAddress(_filePathTextField.text);
                else
                    _fileStoreHandler.SetLogAddress(_filePathTextField.text, _clientSelector.value-1);
                _filePathTextField.text = "";
            }
            else
            {
                if (_clientSelector.value == 0)
                    _fileStoreHandler.SetLogAddress(PanelOpener.OpenFileDialog());
                else
                    _fileStoreHandler.SetLogAddress(PanelOpener.OpenFileDialog(), _clientSelector.value-1);

                // Show the file's address and location in the file path text field
                //_filePathTextField.text = _fileStoreHandler.GetAddress(_clientSelector.value);
            }

            OnFilePathChange();
        }

        public void OnTextFieldEndEdit()
        {
            // Update selected file path
            //_selectedFilePath[_clientSelector.value] = _filePathTextField.text;

            OnFilePathChange();
        }

        public void ConfirmLogs()
        {
            if (_logBoxController != null)
            {
                bool foundlogs = false;
                string[][] logs = new string[_fileStoreHandler.FieldListCount][];
                for (int i = 0; i < _fileStoreHandler.FieldListCount; i++)
                {
                    if (!string.IsNullOrEmpty(_fileStoreHandler.GetAddress(i)))
                    {
                        // Load log file contents
                        logs[i] = File.ReadAllLines(_fileStoreHandler.GetAddress(i));

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

                IReadOnlyBattleLogParserStatus battleLogParserStatus = BattleLogParser.ParseLogs(logs);
                StartCoroutine(WaitForParser(battleLogParserStatus));

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
            for (int i = 0; i < _fileStoreHandler.FieldListCount; i++)
            {
                _fileStoreHandler.SetLogAddress("", i);
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
            _addLogButton.onClick.AddListener(OnButtonClick);
            _confirmLogsButton.onClick.AddListener(ConfirmLogs);
            _denyLogsButton.onClick.AddListener(DenyLogs);

            // Initially hide the buttons
            _confirmLogsButton.gameObject.SetActive(false);
            _denyLogsButton.gameObject.SetActive(false);
        }

        private IEnumerator WaitForParser(IReadOnlyBattleLogParserStatus battleLogParserStatus)
        {
            IReadOnlyMsgStorage msgStorage;

            for (;;)
            {
                yield return null;
                msgStorage = battleLogParserStatus.GetResult();
                if (msgStorage != null) break;
            }

            _logBoxController.SetMsgStorage(msgStorage);
        }

        private void OnFilePathChange(int dummy=0)
        {
            bool addressFound = false;
            for (int i = 0; i < _fileStoreHandler.FieldListCount; i++)
            {
                string address = _fileStoreHandler.GetAddress(i);
                if (string.IsNullOrEmpty(address)) continue;
                addressFound = true;
            }

            if (addressFound)
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
