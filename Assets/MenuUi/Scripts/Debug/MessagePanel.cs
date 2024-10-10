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

        internal void SetMessage(IReadOnlyMsgObject message)
        {
            string infoLogText = string.Format("[Client {0}] [{1:000000}]", message.Client, message.Time);
            _infoTextField.text = infoLogText;

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
