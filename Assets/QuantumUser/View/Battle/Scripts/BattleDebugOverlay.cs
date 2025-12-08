/// @file BattleDebugOverlay.cs
/// <summary>
/// Contains @cref{Battle.View,BattleDebugOverlay} class which handles the debug info.
/// </summary>

// Battle QSimulation usings
using Battle.QSimulation;
using Battle.View.Game;

namespace Battle.View
{
    /// <summary>
    /// Handles the debug info.<br/>
    /// Implements methods to control the debug overlay.
    /// </summary>
    ///
    /// Controls the @cref{Battle.View.UI,BattleUiDebugOverlayHandler} which handles displaying the debug information.<br/>
    /// Establishes a link with @cref{Battle.QSimulation,BattleDebugOverlayLink} to expose debug overlay control methods to Simulation.
    public static class BattleDebugOverlay
    {
        /// <summary>
        /// Initializes debug overlay and calls <see cref="BattleDebugOverlayLink.InitLink"/> to establish a link with Simulation.
        /// </summary>
        public static void Init()
        {
            s_entryCount = 0;
            BattleDebugOverlayLink.InitLink(
                AddEntry,
                AddEntries,
                SetEntry,
                SetEntries
            );
        }

        /// <summary>
        /// Adds new entry to debug overlay.
        /// </summary>
        ///
        /// Adds new entry and sets its @a name.<br/>
        /// To set entries' values call @cref{SetEntry} or @cref{SetEntries} methods with the @em entryNumber you want to set.
        ///
        /// <param name="name">Name of the entry.</param>
        ///
        /// <returns>The <em>entryNumber</em> of the added entry.</returns>
        public static int AddEntry(string name)
        {
            int entryNumber = s_entryCount;

            BattleGameViewController.UiController.DebugOverlayHandler.AddEntries(new string[] { name });

            s_entryCount++;
            return entryNumber;
        }

        /// <summary>
        /// Adds multiple new entries to debug overlay.
        /// </summary>
        ///
        /// Adds new entries and sets their @a names.<br/>
        /// To set entries' values call @cref{SetEntry} or @cref{SetEntries} methods with the @em entryNumber you want to set.
        ///
        /// <param name="names">An array of strings used to set entry names.</param>
        ///
        /// <returns>The <em>entryNumber</em> of the first added entry.</returns>
        public static int AddEntries(string[] names)
        {
            int entriesNumber = s_entryCount;

            BattleGameViewController.UiController.DebugOverlayHandler.AddEntries(names);

            s_entryCount += names.Length;
            return entriesNumber;
        }

        /// <summary>
        /// Sets value of an existing entry in debug overlay.
        /// </summary>
        ///
        /// Sets value of an existing entry at @a entryNumber.<br/>
        /// To add new entry first call @cref{AddEntry} or @cref{AddEntries} methods.
        ///
        /// <param name="entryNumber">Entry number used to identify the entry.</param>
        /// <param name="value">The entry's new value.</param>
        public static void SetEntry(int entryNumber, object value)
        {
            if (entryNumber < 0 || entryNumber >= s_entryCount)
            {
                s_debugLogger.WarningFormat("Entry number ({0}) out of range!", entryNumber);
                return;
            }

            string[] valueTextArray = new string[] { value != null ? value.ToString() : "null" };
            BattleGameViewController.UiController.DebugOverlayHandler.SetEntries(entryNumber, valueTextArray);
        }

        /// <summary>
        /// Sets values of multiple existing entries in debug overlay.
        /// </summary>
        ///
        /// Sets values of existing entries starting at @a entryNumber.<br/>
        /// To add new entries first call @cref{AddEntry} or @cref{AddEntries} methods.
        ///
        /// <param name="entryNumber">Entry number used to identify the first entry.</param>
        /// <param name="values">An array of objects to set entries' new values.</param>
        public static void SetEntries(int entryNumber, object[] values)
        {
            if (entryNumber < 0 || entryNumber + values.Length > s_entryCount)
            {
                s_debugLogger.WarningFormat("Entry number ({0}-{1}) out of range!", entryNumber, entryNumber + values.Length -1);
                return;
            }

            string[] valueTextArray = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                valueTextArray[i] = values[i] != null ? values[i].ToString() : "null";
            }

            BattleGameViewController.UiController.DebugOverlayHandler.SetEntries(entryNumber, valueTextArray);
        }

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger = BattleDebugLogger.Create(typeof(BattleDebugOverlay));

        /// <summary>
        /// The amount of entries.<br/>
        /// Used to assign entry numbers.
        /// </summary>
        private static int s_entryCount;
    }
}
