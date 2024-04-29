using System.Collections;
using System.Collections.Generic;
using DebugUi.Scripts.BattleAnalyzer;
using UnityEngine;
using TMPro;

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
    }
}
