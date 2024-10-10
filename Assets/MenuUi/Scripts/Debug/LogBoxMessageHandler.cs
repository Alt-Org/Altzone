using UnityEngine;
using TMPro;
using DebugUi.Scripts.BattleAnalyzer;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class LogBoxMessageHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textField;

        private IReadOnlyMsgObject _msgObject;

        private LogBoxController _logBoxController;

        private MessagePanel _messagePanel;

        internal IReadOnlyMsgObject MsgObject { get => _msgObject;}

        // Start is called before the first frame update
        void Start()
        {
            _logBoxController = GetComponentInParent<LogBoxController>();
        }

        internal void Initialize(MessagePanel msgPanel, IReadOnlyMsgObject msgObject)
        {
            _messagePanel = msgPanel;
            SetMessage(msgObject);
        }

        public void Message()
        {
            _messagePanel.SetMessage(_msgObject);
            //_logBoxController.MessageDeliver(_msgObject);
        }

        internal void SetMessage(IReadOnlyMsgObject msgObject)
        {
            _msgObject = msgObject;
            string logText = string.Format("[{0:000000}] {1}", msgObject.Time, msgObject.Msg);
            _textField.text = logText;

            // Set text color based on message type
            switch (msgObject.Type)
            {
                case MessageType.Info:
                    _textField.color = Color.white;
                    break;
                case MessageType.Warning:
                    _textField.color = Color.yellow;
                    break;
                case MessageType.Error:
                    _textField.color = Color.red;
                    break;
                default:
                    _textField.color = Color.gray; // Default color
                    break;
            }
        }
    }
}
