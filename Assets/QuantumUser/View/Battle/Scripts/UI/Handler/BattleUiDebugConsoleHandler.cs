// System usings
using System;
using System.Text;

// Unity usings
using UnityEngine;
using TMPro;

// Battle QSimulation usings
using Battle.QSimulation;

using LogType = Battle.QSimulation.BattleDebugLogger.LogType;

namespace Battle.View.UI
{
    public class BattleUiDebugConsoleHandler : MonoBehaviour
    {
        [SerializeField] private UnityReferences _unity;

        [Header("Settings")]
        [SerializeField] private int _entriesMax;
        [SerializeField] private int _entryFontSize;
        [SerializeField] private Vector2 _entryCharacterSize;
        [SerializeField] private int _entryMessageLengthMax;
        [SerializeField] private float _entryLifetimeSec;

        public void SetShow(bool show)
        {
            _isVisible = show;

            UpdateTextField();
        }

        [Serializable]
        private struct UnityReferences
        {
            public RectTransform RectTransform;
            public TMP_Text TextField;
        }

        private struct Entry
        {
            public LogType Type;
            public string  Source;
            public string  Message;
            public float   Timestamp;
        }

        private const string PrefixTypeLog     = "[ <color=#FFFFFF>Log</color> ]";
        private const string PrefixTypeWarning = "[<color=#FFBF00>Warn</color> ]";
        private const string PrefixTypeError   = "[<color=#E34234>Error</color>]";
        private const string PrefixEmpty       = "       ";

        private const int PrefixLength = 8;

        private Entry[] _entries;
        private int     _entriesStartingIndex = 0;
        private int     _entriesCount = 0;

        private bool _isVisible       = false;
        private bool _updateTextField = false;

        private void Awake()
        {
            BattleDebugLogger.InitOnScreenConsoleLink(AddLog);

            _unity.RectTransform.sizeDelta = new Vector2(
                _entryCharacterSize.x * (PrefixLength + _entryMessageLengthMax),
                _entryCharacterSize.y * _entriesMax * 2
            );
            _unity.TextField.fontSize = _entryFontSize;

            _entries = new Entry[_entriesMax];

            for (int i = 0; i < _entriesMax; i++)
            {
                _entries[i] = new Entry
                {
                    Type = LogType.Log,
                    Message = "",
                    Timestamp = 0f
                };
            }
        }

        private void AddLog(LogType type, string source, string message)
        {
            if (_entriesCount == _entriesMax)
            {
                _entriesStartingIndex = (_entriesStartingIndex + 1) % _entriesMax;
            }
            else
            {
                _entriesCount++;
            }

            int newIndex = (_entriesStartingIndex + _entriesCount - 1) % _entriesMax;

            int endOfFirstLineIndex = message.IndexOf('\n', 0, _entryMessageLengthMax);

            int messageShortLength = endOfFirstLineIndex > -1 ? endOfFirstLineIndex : _entryMessageLengthMax;

            if (source  .Length > _entryMessageLengthMax) source  = source  .Substring(0, _entryMessageLengthMax);
            if (message .Length > messageShortLength)     message = message .Substring(0, messageShortLength);

            string typePrefix = type switch
            {
                LogType.Log => PrefixTypeLog,
                LogType.Warning => PrefixTypeWarning,
                LogType.Error => PrefixTypeError,

                _ => PrefixEmpty
            };

            source  = string.Format("{0} {1}", typePrefix,  source);
            message = string.Format("{0} {1}", PrefixEmpty, message);

            _entries[newIndex].Type      = type;
            _entries[newIndex].Source    = source;
            _entries[newIndex].Message   = message;
            _entries[newIndex].Timestamp = Time.time;

            _updateTextField = true;
        }

        private void Update()
        {
            while (_entriesCount > 0 && Time.time - _entries[_entriesStartingIndex].Timestamp >= _entryLifetimeSec)
            {
                _entriesStartingIndex = (_entriesStartingIndex + 1) % _entriesMax;
                _entriesCount--;

                _updateTextField = true;
            }

            if (_updateTextField)
            {
                UpdateTextField();
            }
        }

        private void UpdateTextField()
        {
            if (!_isVisible || _entriesCount < 1)
            {
                _unity.RectTransform.gameObject.SetActive(false);
                _updateTextField = false;
                return;
            }

            _unity.RectTransform.gameObject.SetActive(true);

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < _entriesCount; i++)
            {
                Entry entry = _entries[(_entriesStartingIndex + i) % _entriesMax];

                stringBuilder.Append(entry.Source);
                stringBuilder.Append("\n");
                stringBuilder.Append(entry.Message);
                stringBuilder.Append("\n");
            }

            _unity.TextField.text = stringBuilder.ToString();

            _updateTextField = false;
        }
    }
}
