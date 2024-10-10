using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugTimelineBlock : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image[] _image;
        [SerializeField] private TextMeshProUGUI _timeText;
        private int _time;
        private IReadOnlyTimestamp[] _timestamps;
        private MessageTypeOptions _type = MessageTypeOptions.None;
        private int _sourceTypes = 0;
        private Action<int> _setTimeline;
        private Action<int, int[]> _setLogBoxPosition;

        public void Initialize(Action<int> setTimeline, Action<int, int[]> setLogBoxPosition)
        {
            _setTimeline = setTimeline;
            _setLogBoxPosition = setLogBoxPosition;
            _button.onClick.AddListener(OnBlockClick);
        }

        internal void SetTimeStamps(IReadOnlyTimestamp[] timestamps)
        {
            _timestamps = timestamps;
            _time = _timestamps[0].Time;
            _timeText.text = _time.ToString();
            int i = 0;
            foreach (IReadOnlyTimestamp timestamp in timestamps)
            {
                if (timestamp == null) { ChangeColour(MessageType.None, _image[i]); i++; continue; }
                switch (timestamp.Type)
                {
                    case MessageType.None:
                        break;
                    case MessageType.Info:
                        _type |= (MessageTypeOptions)MessageType.Info;
                        break;
                    case MessageType.Warning:
                        _type |= (MessageTypeOptions)MessageType.Warning;
                        break;
                    case MessageType.Error:
                        _type |= (MessageTypeOptions)MessageType.Error;
                        break;
                }
                ChangeColour(timestamp.Type, _image[i]);
                _sourceTypes |= timestamp.SourceTypes;
                i++;
            }

            if( _type == MessageTypeOptions.None) _button.interactable = false;
        }

        private void ChangeColour(MessageType type, Image image)
        {
            switch (type)
            {
                case MessageType.None:
                    image.color = Color.grey;
                    break;
                case MessageType.Info:
                    image.color = Color.white;
                    break;
                case MessageType.Warning:
                    image.color = Color.yellow;
                    break;
                case MessageType.Error:
                    image.color = Color.red;
                    break;
            }
        }

        public void FilterBlock(MessageTypeOptions options, bool includeEmpty, int sourceFilter)
        {
            if(_type == MessageTypeOptions.None)
                gameObject.SetActive(includeEmpty);
            else
                gameObject.SetActive(options.HasFlag(_type) && (_sourceTypes & sourceFilter) !=0);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _setTimeline.Invoke(_time);
        }
        public void OnBlockClick()
        {
            int[] values = new int[_timestamps.Length];

            for (int i = 0; i < _timestamps.Length; i++)
            {
                IReadOnlyList<IReadOnlyMsgObject> list = _timestamps[i]?.List;
                if (list != null)
                {
                    if (list.Count > 0) values[i] = list[0].Id;
                    else values[i] = -1;
                }
                else values[i] = -2;
            }

            _setLogBoxPosition.Invoke(_time, values);
        }
    }
}
