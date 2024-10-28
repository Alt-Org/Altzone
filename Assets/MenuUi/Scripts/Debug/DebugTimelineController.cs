using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugTimelineController : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private GameObject _timelineBlock;
        [SerializeField] private SliderController _sliderController;
        [SerializeField] private DebugSourceFilterHandler _debugSourceFilter;

        private IReadOnlyMsgStorage _msgStorage;
        private IReadOnlyTimelineStorage _timelineStorage;
        private IReadOnlyList<IReadOnlyTimestamp>[] _timelines;
        private int _lastTime = 0;

        private MessageTypeOptions _msgFilter = MessageTypeOptions.Info | MessageTypeOptions.Warning | MessageTypeOptions.Error;
        private int _sourceFilters;
        private bool _includeEmpty = true;
        private Action<int[]> _setLogBoxPosition;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void Initialize(Action<int[]> setLogBoxPosition)
        {
            _setLogBoxPosition = setLogBoxPosition;
        }

        internal void SetTimeline(IReadOnlyMsgStorage storage)
        {
            _msgStorage = storage;
            _timelineStorage = _msgStorage.GetTimelineStorage();
            _sourceFilters = _msgStorage.GetSourceAllFlags();
            StartCoroutine(NewTimeline());
        }

        private IEnumerator NewTimeline()
        {
            float prevYield = Time.time;
            for (int i = _content.childCount-1; i >= 0;i--)
            {
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - prevYield > 100)
                {
                    prevYield = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    yield return null;
                }
                Destroy(_content.GetChild(i).gameObject);
            }

            if(_timelines == null) _timelines = new IReadOnlyList<IReadOnlyTimestamp>[_timelineStorage.ClientCount];

            for(int i = 0; i < _timelineStorage.ClientCount; i++)
            {
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - prevYield > 100)
                {
                    prevYield = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    yield return null;
                }
                IReadOnlyList<IReadOnlyTimestamp> timeline = _timelineStorage.GetTimeline(i);
                Debug.LogWarning(timeline.Count);
                if (timeline.Count != 0) 
                if(timeline[timeline.Count-1].Time > _lastTime) _lastTime = timeline[timeline.Count-1].Time;
                _timelines[i] = timeline;
            }

            _sliderController.MaxSliderAmount = _lastTime;

            for(int i = 0; i<= _lastTime; i++)
            {
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - prevYield > 100)
                {
                    prevYield = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    yield return null;
                }
                GameObject timelineBlock = Instantiate(_timelineBlock, _content);
                DebugTimelineBlock debugTimelineBlock = timelineBlock.GetComponent<DebugTimelineBlock>();
                debugTimelineBlock.Initialize(SetTimelineValue, FindTimestampValues);
                IReadOnlyTimestamp[] timestamp = new IReadOnlyTimestamp[4];
                for (int j=0; j< _timelines.Length; j++)
                {
                    if (_timelines[j].Count <= i) continue;
                    timestamp[j]= _timelines[j][i];
                }

                debugTimelineBlock.SetTimeStamps(timestamp);
                debugTimelineBlock.FilterBlock(_msgFilter, _includeEmpty, _sourceFilters);
            }
            _debugSourceFilter.SetInitialTimelineFilters(_msgStorage, FilterTimelineWithSource);
        }
        public void FilterTimeline(MessageTypeOptions msgFilter, bool includeEmpty)
        {
            _msgFilter = msgFilter;
            _includeEmpty = includeEmpty;
            foreach (Transform block in _content)
            {
                block.GetComponent<DebugTimelineBlock>().FilterBlock(msgFilter, includeEmpty, _sourceFilters);
            }
        }

        public void FilterTimelineWithSource(int dummy, int sourceFilters)
        {
            _sourceFilters = sourceFilters;
            foreach (Transform block in _content)
            {
                block.GetComponent<DebugTimelineBlock>().FilterBlock(_msgFilter, _includeEmpty, _sourceFilters);
            }
        }

        public void SetTimelineValue(int value)
        {
            _sliderController.SetText(value);
        }

        public void SetPosition(int time)
        {
            float contentWidth = _content.GetComponent<RectTransform>().rect.width;
            float viewWidth = _content.transform.parent.GetComponent<RectTransform>().rect.width;
            float blockPosition = _content.GetChild(time).transform.localPosition.x;
            float value = (blockPosition-(viewWidth/2))/(contentWidth-viewWidth);
            _sliderController.SetSlider(value);
        }

        private void FindTimestampValues(int timestamp, int[] oldvalues)
        {
            int[] values = new int[_timelines.Length];

            for(int i=0; i<values.Length; i++)
            {
                if (oldvalues[i] >= 0) values[i] = oldvalues[i];
                else
                {
                    if (_timelines[i].Count == 0 || _timelines[i].Count-1<timestamp) {values[i] = -1; continue; }
                    int id = -1;
                    int j = 0;
                    do
                    {
                        IReadOnlyList<IReadOnlyMsgObject> list = _timelines[i][timestamp-j].List;
                        if (list.Count > 0)
                        {
                            if(j == 0) id = list[0].Id;
                            else id = list[list.Count-1].Id;
                            values[i] = id;
                            break;
                        }
                        j++;
                    } while (j >= 0);
                    if (id >= 0) continue;
                    values[i] = -1;
                }
            }

            _setLogBoxPosition.Invoke(values);
        }
    }
}
