using UnityEngine;
using TMPro;
using DebugUi.Scripts.BattleAnalyzer;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class LogBoxMessageHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textField;
        [SerializeField] private Image _backgroundImage;

        private IReadOnlyMsgObject _msgObject;

        private LogBoxController _logBoxController;
        private IReadOnlyMsgStorage _msgStorage;

        private MessagePanel _messagePanel;

        private static List<Color> _colourList;

        internal IReadOnlyMsgObject MsgObject { get => _msgObject;}

        // Start is called before the first frame update
        void Start()
        {
            _logBoxController = GetComponentInParent<LogBoxController>();
        }

        internal void Initialize(MessagePanel msgPanel, IReadOnlyMsgObject msgObject, IReadOnlyMsgStorage msgStorage)
        {
            _messagePanel = msgPanel;
            _msgStorage = msgStorage;
            SetMessage(msgObject);
        }

        public void Message()
        {
            _messagePanel.SetMessage(_msgObject, _colourList);
            _logBoxController.SetTimelinePosition(_msgObject.Time);
        }

        internal void SetMessage(IReadOnlyMsgObject msgObject)
        {
            _msgObject = msgObject;
            string logText = string.Format("[{0:000000}] {1} {2}", msgObject.Time, msgObject.ColorGroup, msgObject.Msg);
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
            if (_colourList == null) CreateColourSet();
            _backgroundImage.color = _colourList[msgObject.ColorGroup];
            
        }

        private void CreateColourSet()
        {
            IReadOnlyList<int> superColourList = _msgStorage.GetColorSuperGroupList();

            int lastGroup = superColourList.Count;
            int totalGroupCount = 0;
            foreach (int superGroupSize in superColourList)
            {
                totalGroupCount += superGroupSize;
            }
            _colourList = new();
            float groupColourSampleSize = ((120f - (10f * (superColourList.Count - 1))) / (superColourList.Count - 2));
            Debug.LogWarning("SuperGroupSize: "+groupColourSampleSize);
            int i = 0;
            foreach (int superGroupSize in superColourList)
            {
                if (i == 0)
                {
                    Color colour = Color.HSVToRGB(360f / 360f, 1, 0.7f);
                    _colourList.Add(colour);
                }
                else if (i == lastGroup - 1)
                {
                    Color colour = Color.HSVToRGB(240f / 360f, 1, 1f);
                    _colourList.Add(colour);
                }
                else
                {
                    float colourSampleSize = groupColourSampleSize / superColourList[i] + 1;
                    for (int j = 1; j < superColourList[i] + 1; j++)
                    {
                        Debug.LogWarning((360f - 10f * i - groupColourSampleSize * (i-1) - colourSampleSize * j )/ 360f);
                        Color colour = Color.HSVToRGB((360f - 10f * i - groupColourSampleSize * (i - 1) - colourSampleSize * j )/ 360f, 1, 0.9f);
                        _colourList.Add(colour);
                    }
                }
                i++;
            }
        }
    }
}
