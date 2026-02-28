/// @file BattleUiDebugConsoleHandler.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiDebugConsoleHandler} class which displays custom on-screen debug console.
/// </summary>

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
    /// <summary>
    /// <span class="brief-h">DebugConsole @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles displaying the custom on-screen debug console.
    /// </summary>
    ///
    /// @anchor BattleUiDebugConsoleHandler-DetailedDescription
    ///
    /// Controlled by @cref{Battle.QSimulation,BattleDebugLogger}
    ///
    /// Shows only certain amount of log entries. First line shows the type of log and it's source and second line shows the message of the log.<br/>
    /// Logs appear on console when @cref{Battle.QSimulation,BattleDebugLogger} requests log to be shown.<br/>
    /// Each log entry has a lifetime after which they disappear.<br/>
    /// See @ref BattleUiDebugConsoleHandler-SerializeFields "SerializeFields" for all settings.
    ///
    /// When visible, automatically shows entry logs if there are any, otherwise debug console will be hidden.<br/>
    /// When not visible, console is never shown.<br/>
    /// See @cref{SetShow}
    public class BattleUiDebugConsoleHandler : MonoBehaviour
    {
        /// @anchor BattleUiDebugConsoleHandler-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] <see cref="UnityReferences"/> struct.</summary>
        /// @ref BattleUiDebugConsoleHandler-SerializeFields "SerializeFields"
        [Tooltip("UnityReferences struct")]
        [SerializeField] private UnityReferences _unity;

        [Header("Settings")]

        /// <summary>[SerializeField] The max amount of entries.</summary>
        /// @ref BattleUiDebugConsoleHandler-SerializeFields "SerializeFields"
        [Tooltip("The max amount of entries")]
        [SerializeField] private int _entriesMax;

        /// <summary>[SerializeField] The font size.</summary>
        /// @ref BattleUiDebugConsoleHandler-SerializeFields "SerializeFields"
        [Tooltip("The font size")]
        [SerializeField] private int _entryFontSize;

        /// <summary>[SerializeField] The size of one character including padding.</summary>
        /// @ref BattleUiDebugConsoleHandler-SerializeFields "SerializeFields"
        [Tooltip("The size of one character including padding")]
        [SerializeField] private Vector2 _entryCharacterSize;

        /// <summary>[SerializeField] The max message length.</summary>
        /// @ref BattleUiDebugConsoleHandler-SerializeFields "SerializeFields"
        [Tooltip("The max message length")]
        [SerializeField] private int _entryMessageLengthMax;

        /// <summary>[SerializeField] The lifetime of individual log entries in seconds.</summary>
        /// @ref BattleUiDebugConsoleHandler-SerializeFields "SerializeFields"
        [Tooltip("The lifetime of individual log entries in seconds")]
        [SerializeField] private float _entryLifetimeSec;

        /// @}

        /// <summary>
        /// Sets the %UI element visibility.
        /// </summary>
        ///
        /// <param name="show">If the %UI element should be visible or not.</param>
        public void SetShow(bool show)
        {
            _isVisible = show;

            UpdateUI();
        }

        /// <summary>Private struct used internally to hold references to Unity components.</summary>
        [Serializable]
        private struct UnityReferences
        {
            /// <summary>Root RectTransform component of the debug console.</summary>
            [Tooltip("Root RectTransform component of the debug console")]
            public RectTransform RectTransform;

            /// <summary>TMP_Text component of the debug console textfield.</summary>
            [Tooltip("TMP_Text component of the debug console textfield")]
            public TMP_Text TextField;
        }

        /// <summary>Private struct used internally to hold the data of an entry.</summary>
        private struct Entry
        {
            /// <summary>Type of the log entry.</summary>
            public LogType Type;

            /// <summary>Source of the log entry.</summary>
            public string Source;

            /// <summary>Message of the log entry.</summary>
            public string Message;

            /// <summary>Lifetime of the log entry.</summary>
            public float Timestamp;
        }

        //{ Prefixes

        /// <summary>Prefix for log entry of type Log.</summary>
        private const string PrefixTypeLog = "[ <color=#FFFFFF>Log</color> ]";

        /// <summary>Prefix for log entry of type Warning.</summary>
        private const string PrefixTypeWarning = "[<color=#FFBF00>Warn</color> ]";

        /// <summary>Prefix for log entry of type Error.</summary>
        private const string PrefixTypeError = "[<color=#E34234>Error</color>]";

        /// <summary>Prefix used for formatting and fallback.</summary>
        private const string PrefixEmpty = "       ";

        /// <summary>Length of the prefix.</summary>
        private const int PrefixLength = 8;

        //} Prefixes

        //{ Entries array
        // Entries array is allocated with maximum amount of entries.
        // Entries will be added at the end of the array (_entriesStartingIndex + _entriesCount).
        // Entries will be removed from the start of the array (_entriesStartingIndex).
        // To achieve this without unnecessarily moving the entries around in memory, we keep track of _entriesStartingIndex and _entriesCount.
        // Expanding the array can be achieved by incrementing the _entriesCount.
        // Shrinking the array can be achieved by incrementing _entriesStartingIndex and decrementing the _entriesCount.
        // To make this work the array wraps around from the end to the beginning.
        // Example: (Array size = 8, _entriesStartingIndex = 5, _entriesCount = 6)
        // _entries[0] entry 4
        // _entries[1] entry 5
        // _entries[2] entry 6 <- _entriesStartingIndex + _entriesCount -1
        // _entries[3] unused
        // _entries[4] unused
        // _entries[5] entry 1 <- _entriesStartingIndex
        // _entries[6] entry 2
        // _entries[7] entry 3

        /// <summary>An array of log entries.</summary>
        private Entry[] _entries;

        /// <summary>Starting index for the log entries.</summary>
        private int _entriesStartingIndex = 0;

        /// <summary>Amount of log entries.</summary>
        private int _entriesCount = 0;

        //} Entries array

        /// <summary>Is the %UI element visible or not.</summary>
        private bool _isVisible = false;

        /// <summary>Should %UI be updated or not.</summary>
        private bool _updateUI = false;

        /// <summary>
        /// Initializes the link between <see cref="BattleDebugLogger"/>, the size of the debug console, font size and an entry array.
        /// </summary>
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

        /// <summary>
        /// Used by <see cref="BattleDebugLogger"/> to add logs to debug console.
        /// </summary>
        ///
        /// Formats log entry and adds it to @cref{_entries} array after which @cref{_updateUI} will be set to true to signal that %UI needs to be updated.<br/>
        /// See @cref{UpdateUI}
        ///
        /// <param name="type">Type of the log entry.</param>
        /// <param name="source">Source of the log entry.</param>
        /// <param name="message">Message of the log entry.</param>
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

            int endOfFirstLineIndex = message.IndexOf('\n', 0, Mathf.Min(_entryMessageLengthMax, message.Length));

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

            _updateUI = true;
        }

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method.
        /// Checks if a log has ran out of lifetime and if %UI should be updated.
        /// </summary>
        ///
        /// See @cref{UpdateUI}
        private void Update()
        {
            while (_entriesCount > 0 && Time.time - _entries[_entriesStartingIndex].Timestamp >= _entryLifetimeSec)
            {
                _entriesStartingIndex = (_entriesStartingIndex + 1) % _entriesMax;
                _entriesCount--;

                _updateUI = true;
            }

            if (_updateUI)
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// Handles visibility and updating the debug console text.
        /// </summary>
        ///
        /// Shows and hides the debug console automatically.<br/>
        /// Builds and sets debug console text based on the current state of the @cref{_entries} array.
        ///
        /// See @ref BattleUiDebugConsoleHandler-DetailedDescription "Detailed Description" for more info.
        private void UpdateUI()
        {
            if (!_isVisible || _entriesCount < 1)
            {
                _unity.RectTransform.gameObject.SetActive(false);
                _updateUI = false;
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

            _updateUI = false;
        }
    }
}
