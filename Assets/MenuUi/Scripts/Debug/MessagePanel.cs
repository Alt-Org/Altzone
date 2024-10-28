using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class MessagePanel : MonoBehaviour/*, IPointerClickHandler*/
    {
        [SerializeField] private TextMeshProUGUI _infoTextField;
        [SerializeField] private TextMeshProUGUI _msgMainTextField;
        [SerializeField] private TextMeshProUGUI _traceTextField;
        [SerializeField] private Button _logButton;
        public GameObject Panel;
        private bool _diffMode = true;
        private IReadOnlyMsgObject _currentMessage;
        private List<Color> _colourList;

        void Start()
        {
            //_logButton.onClick.AddListener(ClosePanel);
        }

        public void OpenPanel()
        {
            if (Panel != null)
            {
                bool isActive = Panel.activeSelf;

                Panel.SetActive(true);
            }
        }

        public void ClosePanel()
        {
            if (Panel != null)
            {

                Panel.SetActive(false);
            }
        }

        internal void NewMessage(IReadOnlyMsgObject message, List<Color> colourList)
        {
            _currentMessage = message;
            if (_colourList == null || _colourList.Count == 0) _colourList = colourList;
            SetMessage(_currentMessage, _colourList);
        }

        internal void SetMessage(IReadOnlyMsgObject message, List<Color>colourList)
        {
            if (message == null || colourList == null|| colourList.Count == 0) return;
            string infoLogText = string.Format("[Client {0}] [{1:000000}]", message.Client, message.Time);
            _infoTextField.text = infoLogText;
            if (message.ColorGroup > 0 && _diffMode)
            {
                string fullMessage = "";
                foreach (IReadOnlyMsgObject matchMessage in message.MatchList)
                {
                    if (!string.IsNullOrWhiteSpace(fullMessage)) fullMessage += "\r\n";
                    fullMessage += string.Format("[C{0}] {1}", matchMessage.Client, matchMessage.GetHighlightedMsg(colourList));
                }
                _msgMainTextField.text = fullMessage;
            }
            else
                _msgMainTextField.text = message.Msg;

            switch (message.Type)
            {
                case MessageType.Info:
                    _msgMainTextField.color = Color.white;
                    break;
                case MessageType.Warning:
                    _msgMainTextField.color = Color.yellow;
                    break;
                case MessageType.Error:
                    _msgMainTextField.color = Color.red;
                    break;
                default:
                    _msgMainTextField.color = Color.gray; // Default color
                    break;
            }


            _traceTextField.text = message.Trace;

            OpenPanel();
        }

        public void SetDiffMode(bool value)
        {
            _diffMode = value;
            SetMessage(_currentMessage, _colourList);
        }

        public void OnPointerClick(BaseEventData eventData)
        {
            Debug.Log(((PointerEventData)eventData).pointerCurrentRaycast.gameObject.name);
            if (!((PointerEventData)eventData).pointerCurrentRaycast.gameObject.Equals(Panel))
            {
                ClosePanel();
            }
        }
        /*public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log(eventData.pointerCurrentRaycast.gameObject.name);
            if (!eventData.pointerCurrentRaycast.gameObject.Equals(Panel))
            {
                ClosePanel();
            }
        }*/
    }
}
