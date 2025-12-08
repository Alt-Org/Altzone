/// @file BattleUiDebugOverlayHandler.cs
/// <summary>
/// Contains @cref{Battle.View.UI,BattleUiDebugOverlayHandler} class which displays the debug info.
/// </summary>

// System usings
using System.Collections.Generic;

// Unity usings
using UnityEngine;
using TMPro;

namespace Battle.View.UI
{
    /// <summary>
    /// <span class="brief-h">DebugOverlay @uihandlerlink (<a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>).</span><br/>
    /// Handles displaying the debug info.<br/>
    /// Controlled by <see cref="BattleDebugOverlay"/>.
    /// </summary>
    public class BattleUiDebugOverlayHandler : MonoBehaviour
    {
        /// @anchor BattleUiDebugOverlayHandler-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to the GameObject which can be used to hide or show the overlay.</summary>
        /// @ref BattleUiDebugOverlayHandler-SerializeFields "SerializeFields"
        [Tooltip("Reference to the GameObject which can be used to hide or show the overlay")]
        [SerializeField] private GameObject _view;

        /// <summary>[SerializeField] Reference to the GameObject which is used as template when instantiating entries.</summary>
        /// @ref BattleUiDebugOverlayHandler-SerializeFields "SerializeFields"
        [Tooltip("Reference to the GameObject which is used as template when instantiating entries")]
        [SerializeField] private GameObject _entryTemplate;

        /// <summary>[SerializeField] Reference to the Transform which the entries are parented to.</summary>
        /// @ref BattleUiDebugOverlayHandler-SerializeFields "SerializeFields"
        [Tooltip("Reference to the Transform which the entries are parented to")]
        [SerializeField] private Transform _entryParent;

        /// <summary>[SerializeField] The entry height.</summary>
        /// @ref BattleUiDebugOverlayHandler-SerializeFields "SerializeFields"
        [Tooltip("The entry height")]
        [SerializeField] private float _entryHeight;

        /// <summary>[SerializeField] The entry margin.</summary>
        /// @ref BattleUiDebugOverlayHandler-SerializeFields "SerializeFields"
        [Tooltip("The entry margin")]
        [SerializeField] private float _entryMargin;

        /// <summary>[SerializeField] The entry font size.</summary>
        /// @ref BattleUiDebugOverlayHandler-SerializeFields "SerializeFields"
        [Tooltip("The entry font size")]
        [SerializeField] private float _entryFontSize;

        /// @}

        /// <value>Is the %UI element visible or not.</value>
        public bool IsVisible => _view.activeSelf;

        /// <summary>
        /// Sets the %UI element visibility.
        /// </summary>
        ///
        /// <param name="show">If the %UI element should be visible or not.</param>
        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

        /// <summary>
        /// Method for adding multiple new entries.
        /// </summary>
        ///
        /// Creates as many entries as there are elements in the @a names array.<br/>
        /// After adding entries calls @cref{CalculateNameColumnLayout} and @cref{UpdateLayout}.
        ///
        /// <param name="names">An array of strings used to give name to entries.</param>
        public void AddEntries(string[] names)
        {
            foreach(string name in names)
            {
                Entry entry = new Entry()
                {
                    Name = Instantiate(_entryTemplate, _entryParent).GetComponent<TMP_Text>(),
                    Value = Instantiate(_entryTemplate, _entryParent).GetComponent<TMP_Text>()
                };

                entry.Name.gameObject.name = name + " (Name)";
                entry.Value.gameObject.name = name + " (Value)";

                entry.Name.fontSize = _entryFontSize;
                entry.Value.fontSize = _entryFontSize;

                entry.Name.gameObject.SetActive(true);
                entry.Value.gameObject.SetActive(true);

                entry.Name.text = name;

                _entries.Add(entry);
            }

            CalculateNameColumnLayout();
            UpdateLayout();
        }

        /// <summary>
        /// Method for setting multiple existing entries.
        /// </summary>
        ///
        /// Sets multiple entries from @a entryNumber to @a entrynumber + length of the @a textArray.<br/>
        /// After setting entries calls @cref{CalculateValueColumnLayout} and @cref{UpdateLayout}.
        ///
        /// <param name="entryNumber">Entry number used to identify the first entry.</param>
        /// <param name="textArray">An array of strings used to give text to entries.</param>
        public void SetEntries(int entryNumber, string[] textArray)
        {
            for(int i = 0; i < textArray.Length; i++)
            {
                _entries[entryNumber + i].Value.text = textArray[i];
            }
            CalculateValueColumnLayout();
            UpdateLayout();
        }

        /// <summary>Width of the name column.</summary>
        ///
        /// Used by @cref{UpdateLayout}.
        private float _nameColumnWidth;

        /// <summary>Width of the value column.</summary>
        ///
        /// Used by @cref{UpdateLayout}.
        private float _valueColumnWidth;

        /// <summary>List of the entries</summary>
        private readonly List<Entry> _entries = new List<Entry>();

        /// <summary>
        /// Private struct used internally to hold references to the <em>Name</em> and <em>Value</em> <b>TMP_Text</b> components for entries.
        /// </summary>
        private struct Entry
        {
            public TMP_Text Name;
            public TMP_Text Value;
        }

        /// <summary>
        /// Method for updating entries' layout.
        /// </summary>
        ///
        /// Positions and scales entries.<br/>
        /// Uses @cref{_nameColumnWidth} and @cref{_valueColumnWidth} which are set by
        /// @cref{CalculateNameColumnLayout} and @cref{CalculateValueColumnLayout}.
        private void UpdateLayout()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                Entry entry = _entries[i];

                entry.Name.rectTransform.anchoredPosition = new Vector2(-_valueColumnWidth, -_entryHeight * i);
                entry.Name.rectTransform.sizeDelta = new Vector2(_nameColumnWidth, _entryHeight);

                entry.Value.rectTransform.anchoredPosition = new Vector2(0, -_entryHeight * i);
                entry.Value.rectTransform.sizeDelta = new Vector2 (_valueColumnWidth, _entryHeight);
            }
        }

        /// <summary>
        /// Private helper method for calculating how wide the name column should be.<br/>
        /// Stored in <see cref="_nameColumnWidth"/>.
        /// </summary>
        private void CalculateNameColumnLayout()
        {
            float preferredWidth = 0;

            foreach (Entry entry in _entries)
            {
                preferredWidth = Mathf.Max(entry.Name.preferredWidth, preferredWidth);
            }
            _nameColumnWidth = Mathf.Max(preferredWidth + _entryMargin, _nameColumnWidth);
        }

        /// <summary>
        /// Private helper method for calculating how wide the value column should be.<br/>
        /// Stored in <see cref="_valueColumnWidth"/>.
        /// </summary>
        private void CalculateValueColumnLayout()
        {
            float preferredWidth = 0;

            foreach (Entry entry in _entries)
            {
                preferredWidth = Mathf.Max(entry.Value.preferredWidth, preferredWidth);
            }
            _valueColumnWidth = Mathf.Max(preferredWidth + _entryMargin, _valueColumnWidth);
        }
    }
}
