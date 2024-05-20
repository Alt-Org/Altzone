using UnityEngine;
using TMPro;
using DebugUi.Scripts.BattleAnalyzer;

public class LogBoxMessageHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textField;

    private IReadOnlyMsgObject _msgObject;

    private LogBoxController _logBoxController;

    // Start is called before the first frame update
    void Start()
    {
        _logBoxController = GetComponentInParent<LogBoxController>();
    }

    public void Message()
    {
        _logBoxController.MessageDeliver(_msgObject);
    }

    internal void SetMessage(IReadOnlyMsgObject msgObject)
    {
        _msgObject = msgObject;
        string logText = $"[{msgObject.Client}:{msgObject.Time}] {msgObject.Msg}\n";
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