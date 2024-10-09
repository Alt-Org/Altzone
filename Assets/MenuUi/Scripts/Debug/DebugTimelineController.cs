using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugTimelineController : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private GameObject _timelineBlock;

        private IReadOnlyTimelineStorage _storage;
        private IReadOnlyList<IReadOnlyTimestamp>[] _timelines;
        private int _lastTime = 0;

        // Start is called before the first frame update
        void Start()
        {

        }

        internal void SetTimeline(IReadOnlyTimelineStorage storage)
        {
            _storage = storage;
            NewTimeline();
        }

        private void NewTimeline()
        {
            for(int i = _content.childCount-1; i >= 0;i--)
            {
                Destroy(_content.GetChild(i).gameObject);
            }

            if(_timelines == null) _timelines = new IReadOnlyList<IReadOnlyTimestamp>[_storage.ClientCount];

            for(int i = 0; i < _storage.ClientCount; i++)
            {
                IReadOnlyList<IReadOnlyTimestamp> timeline = _storage.GetTimeline(i);
                Debug.LogWarning(timeline.Count);
                if (timeline.Count != 0) 
                if(timeline[timeline.Count-1].Time > _lastTime) _lastTime = timeline[timeline.Count-1].Time;
                _timelines[i] = timeline;
            }

            for(int i = 0; i<= _lastTime; i++)
            {
                GameObject timelineBlock = Instantiate(_timelineBlock, _content);
                DebugTimelineBlock debugTimelineBlock = timelineBlock.GetComponent<DebugTimelineBlock>();

                IReadOnlyTimestamp[] timestamp = new IReadOnlyTimestamp[4];
                for (int j=0; j< _timelines.Length; j++)
                {
                    if (_timelines[j].Count <= i) continue;
                    timestamp[j]= _timelines[j][i];
                }

                debugTimelineBlock.SetTimeStamps(timestamp);
            }
        }
        public void FilterTimeline(MessageTypeOptions msgFilter, bool includeEmpty)
        {
            foreach (Transform block in _content)
            {
                block.GetComponent<DebugTimelineBlock>().FilterBlock(msgFilter, includeEmpty);
            }
        }
    }
}
