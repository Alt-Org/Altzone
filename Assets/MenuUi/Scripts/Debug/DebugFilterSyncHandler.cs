using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugFilterSyncHandler : MonoBehaviour
    {
        [SerializeField] private DebugSourceFilterHandler _sourceFilterHandler;
        [SerializeField] private DebugFilterHandler _filterHandler;

        [SerializeField] private Toggle _filterSyncToggle;

        [SerializeField] private Image _filterImage;
        [SerializeField] private Sprite _lockOn;
        [SerializeField] private Sprite _lockOff;

        private Action<bool,bool> _emptyToggle;
        private Action<bool, bool> _infoToggle;
        private Action<bool, bool> _warningToggle;
        private Action<bool, bool> _errorToggle;
        private Action<int,bool, bool> _setSourceFilter;

        private bool _sync = false;

        public bool Sync { get => _sync;}

        public delegate void FilterStatusChanged(DebugFilterSyncHandler thisHandler,MessageType filter = MessageType.None, bool value = false, bool? empty = null);
        public static event FilterStatusChanged OnFilterStatusChanged;

        public delegate void SourceFilterStatusChanged(DebugFilterSyncHandler thisHandler, int filter, bool value);
        public static event SourceFilterStatusChanged OnSourceFilterStatusChanged;

        // Start is called before the first frame update
        void Start()
        {
            _filterSyncToggle.isOn = _sync;
            _filterSyncToggle.onValueChanged.AddListener(OnSyncToggle);
        }

        private void OnEnable()
        {
            DebugFilterSync.OnFilterStatusChanged += UpdateFilters;
            DebugFilterSync.OnSourceFilterStatusChanged += UpdateSourceFilters;
        }

        private void OnDisable()
        {
            DebugFilterSync.OnFilterStatusChanged -= UpdateFilters;
            DebugFilterSync.OnSourceFilterStatusChanged -= UpdateSourceFilters;
        }

        public void InitializeFilterCalls(Action<bool,bool> EmptyToggle, Action<bool,bool> InfoToggle, Action<bool,bool> WarningToggle, Action<bool,bool> ErrorToggle)
        {
            _emptyToggle = EmptyToggle;
            _infoToggle = InfoToggle;
            _warningToggle = WarningToggle;
            _errorToggle = ErrorToggle;
        }

        public void InitializeSourceFilterCalls(Action<int,bool, bool> SetFilter)
        {
            _setSourceFilter = SetFilter;
        }

        public void SendFilters(MessageType typeOptions = MessageType.None, bool value = false, bool? empty = null)
        {
            if (_sync)
            {
                OnFilterStatusChanged(this, typeOptions,value,empty);
            }
        }
        public void SendSourceFilters(int flag, bool value)
        {
            if (_sync)
            {
                OnSourceFilterStatusChanged(this, flag, value);
            }
        }

        public void UpdateFilters(DebugFilterSyncHandler originHandler, MessageType typeOptions = MessageType.None, bool value = false, bool? empty = null)
        {
            if (!_sync) return;
            if(originHandler.Equals(this)) return;
            if (empty != null) _emptyToggle.Invoke((bool)empty, true);
            else if(typeOptions == MessageType.Info) _infoToggle.Invoke(value, true);
            else if (typeOptions == MessageType.Warning) _warningToggle.Invoke(value, true);
            else if (typeOptions == MessageType.Error) _errorToggle.Invoke(value, true);
        }
        public void UpdateSourceFilters(DebugFilterSyncHandler originHandler, int flag, bool value)
        {
            if (!_sync) return;
            if (originHandler.Equals(this)) return;
            _setSourceFilter.Invoke(flag, value, true);
        }

        private void OnSyncToggle(bool value)
        {
            _sync = value;
            if (value) _filterImage.sprite = _lockOn;
            else _filterImage.sprite = _lockOff;
        }
    }
}
