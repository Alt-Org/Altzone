using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugTimelineBlock : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _timeText;
        private int _time;
        private IReadOnlyTimestamp[] _timestamps;
        private MessageType _type = MessageType.None;

        // Start is called before the first frame update
        void Start()
        {
            _timestamps = new IReadOnlyTimestamp[4];
        }

        internal void SetTimeStamps(IReadOnlyTimestamp[] timestamps)
        {
            _timestamps = timestamps;
            _time = _timestamps[0].Time;
            _timeText.text = _time.ToString();
            foreach (IReadOnlyTimestamp timestamp in timestamps)
            {
                if (timestamp == null) continue;
                switch (timestamp.Type)
                {
                    case MessageType.None:
                        break;
                    case MessageType.Info:
                        if(_type <= MessageType.Info) _type = MessageType.Info;
                        break;
                    case MessageType.Warning:
                        if(_type <= MessageType.Warning) _type = MessageType.Warning;
                        break;
                    case MessageType.Error:
                        if(_type <= MessageType.Error) _type = MessageType.Error;
                        break;
                }
            }
            switch (_type)
            {
                case MessageType.None:
                    _image.color = Color.grey;
                    _button.interactable = false;
                    break;
                case MessageType.Info:
                    _image.color = Color.white;
                    break;
                case MessageType.Warning:
                    _image.color = Color.yellow;
                    break;
                case MessageType.Error:
                    _image.color = Color.red;
                    break;
            }
        }

        public void FilterBlock(MessageTypeOptions options, bool includeEmpty)
        {
            if(_type == MessageType.None)
                gameObject.SetActive(includeEmpty);
            else
                gameObject.SetActive(options.HasFlag((MessageTypeOptions)_type));
        }
    }
}
