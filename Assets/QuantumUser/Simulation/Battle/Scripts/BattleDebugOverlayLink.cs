/// @file BattleDebugOverlayLink.cs
/// <summary>
/// Contains @cref{Battle.QSimulation,BattleDebugOverlayLink} which is a wrapper class for handling debug info in Simulation.
/// </summary>


// System usings
using System;
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;

namespace Battle.QSimulation
{
    /// <summary>
    /// Wrapper class for handling debug info in Simulation.<br/>
    /// Implements wrapper methods to control the debug overlay.
    /// </summary>
    public static class BattleDebugOverlayLink
    {
        /// <summary>
        /// Establishes link between View and Simulation.
        /// </summary>
        ///
        /// <param name="addEntryFnRef"><see cref="Battle.View.BattleDebugOverlay.AddEntry">BattleDebugOverlay.AddEntry</see> function reference.</param>
        /// <param name="addEntriesFnRef"><see cref="Battle.View.BattleDebugOverlay.AddEntries">BattleDebugOverlay.AddEntries</see> function reference.</param>
        /// <param name="setEntryFnRef"><see cref="Battle.View.BattleDebugOverlay.SetEntry">BattleDebugOverlay.SetEntry</see> function reference.</param>
        /// <param name="setEntriesFnRef"><see cref="Battle.View.BattleDebugOverlay.SetEntries">BattleDebugOverlay.SetEntries</see> function reference.</param>
        public static void InitLink(
            Func<string, int>  addEntryFnRef,
            Func<string[], int>  addEntriesFnRef,
            Action<int,object> setEntryFnRef,
            Action<int,object[]> setEntriesFnRef
        )
        {
            s_localPlayerSlot = BattlePlayerSlot.Spectator;

            s_addEntryFnRef   = addEntryFnRef;
            s_addEntriesFnRef = addEntriesFnRef;
            s_setEntryFnRef   = setEntryFnRef;
            s_setEntriesFnRef = setEntriesFnRef;
        }

        /// <summary>
        /// Sets local player slot.
        /// </summary>
        ///
        /// <param name="slot">Slot of the player.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPlayerSlot(BattlePlayerSlot slot) => s_localPlayerSlot = slot;

        /// <summary>
        /// Adds new entry to debug overlay.<br/>
        /// Simulation side wrapper method for <see cref="Battle.View.BattleDebugOverlay.AddEntry">BattleDebugOverlay.AddEntry</see> method.
        /// </summary>
        ///
        /// Adds new entry and sets its @a name.<br/>
        /// To set entries' values call @cref{SetEntry} or @cref{SetEntries} methods with the @em entryNumber you want to set.
        ///
        /// <param name="name">Name of the entry.</param>
        ///
        /// <returns>The <em>entryNumber</em> of the added entry.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddEntry(string name) => s_addEntryFnRef(name);

        /// <summary>
        /// Adds new entry to debug overlay.<br/>
        /// Simulation side wrapper method for <see cref="Battle.View.BattleDebugOverlay.AddEntry">BattleDebugOverlay.AddEntry</see> method.<br/>
        /// Filtered based on <paramref name="playerSlot"/>.
        /// </summary>
        ///
        /// Adds new entry and sets its @a name.<br/>
        /// To set entries' values call @cref{SetEntry} or @cref{SetEntries} methods with the @em entryNumber you want to set.
        ///
        /// If @a playerSlot is not the same as local player slot then method call will be ignored and minus one is returned.
        ///
        /// <param name="playerSlot">Slot of the player.</param>
        /// <param name="name">Name of the entry.</param>
        ///
        /// <returns>The <em>entryNumber</em> of the added entry or -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddEntry(BattlePlayerSlot playerSlot, string name)
        {
            if (playerSlot != s_localPlayerSlot) return -1;

            return s_addEntryFnRef(name);
        }

        /// <summary>
        /// Adds multiple new entries to debug overlay.<br/>
        /// Simulation side wrapper method for <see cref="Battle.View.BattleDebugOverlay.AddEntries">BattleDebugOverlay.AddEntries</see> method.
        /// </summary>
        ///
        /// Adds new entries and sets their @a names.<br/>
        /// To set entries' values call @cref{SetEntry} or @cref{SetEntries} methods with the @em entryNumber you want to set.
        ///
        /// <param name="names">An array of strings used to set entry names.</param>
        ///
        /// <returns>The <em>entryNumber</em> of the first added entry.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddEntries(string[] names) => s_addEntriesFnRef(names);

        /// <summary>
        /// Adds multiple new entries to debug overlay.<br/>
        /// Simulation side wrapper method for <see cref="Battle.View.BattleDebugOverlay.AddEntries">BattleDebugOverlay.AddEntries</see> method.<br/>
        /// Filtered based on <paramref name="playerSlot"/>.
        /// </summary>
        ///
        /// Adds new entries and sets their @a names.<br/>
        /// To set entries' values call @cref{SetEntry} or @cref{SetEntries} methods with the @em entryNumber you want to set.
        ///
        /// If @a playerSlot is not the same as local player slot then method call will be ignored and minus one is returned.
        ///
        /// <param name="playerSlot">Slot of the player.</param>
        /// <param name="names">An array of strings used to set entry names.</param>
        ///
        /// <returns>The <em>entryNumber</em> of the first added entry or -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddEntries(BattlePlayerSlot playerSlot, string[] names)
        {
            if (playerSlot != s_localPlayerSlot) return -1;

            return s_addEntriesFnRef(names);
        }

        /// <summary>
        /// Sets value of a existing entry in debug overlay.<br/>
        /// Simulation side wrapper method for <see cref="Battle.View.BattleDebugOverlay.SetEntry">BattleDebugOverlay.SetEntry</see> method.
        /// </summary>
        ///
        /// Sets value of an existing entry at @a entryNumber.<br/>
        /// To add new entry first call @cref{AddEntry} or @cref{AddEntries} methods.
        ///
        /// <param name="entryNumber">Entry number used to identify the entry.</param>
        /// <param name="value">The entry's new value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEntry(int entryNumber, object value) => s_setEntryFnRef(entryNumber, value);

        /// <summary>
        /// Sets value of a existing entry in debug overlay.<br/>
        /// Simulation side wrapper method for <see cref="Battle.View.BattleDebugOverlay.SetEntry">BattleDebugOverlay.SetEntry</see> method.<br/>
        /// Filtered based on <paramref name="playerSlot"/>.
        /// </summary>
        ///
        /// Sets value of an existing entry at @a entryNumber.<br/>
        /// To add new entry first call @cref{AddEntry} or @cref{AddEntries} methods.
        ///
        /// If @a playerSlot is not the same as local player slot then method call will be ignored and minus one is returned.
        ///
        /// <param name="playerSlot">Slot of the player.</param>
        /// <param name="entryNumber">Entry number used to identify the entry.</param>
        /// <param name="value">The entry's new value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEntry(BattlePlayerSlot playerSlot, int entryNumber, object value)
        {
            if(playerSlot != s_localPlayerSlot) return;

            s_setEntryFnRef(entryNumber, value);
        }

        /// <summary>
        /// Sets values of multiple existing entries in debug overlay.<br/>
        /// Simulation side wrapper method for <see cref="Battle.View.BattleDebugOverlay.SetEntries">BattleDebugOverlay.SetEntries</see> method.
        /// </summary>
        ///
        /// Sets values of existing entries starting at @a entryNumber.<br/>
        /// To add new entries first call @cref{AddEntry} or @cref{AddEntries} methods.
        ///
        /// <param name="entryNumber">Entry number used to identify the first entry.</param>
        /// <param name="values">An array of objects to set entries' new values.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEntries(int entryNumber, object[] values) => s_setEntriesFnRef(entryNumber, values);

        /// <summary>
        /// Sets values of multiple existing entries in debug overlay.<br/>
        /// Simulation side wrapper method for <see cref="Battle.View.BattleDebugOverlay.SetEntries">BattleDebugOverlay.SetEntries</see> method.<br/>
        /// Filtered based on <paramref name="playerSlot"/>.
        /// </summary>
        ///
        /// Sets values of existing entries starting at @a entryNumber.<br/>
        /// To add new entries first call @cref{AddEntry} or @cref{AddEntries} methods.
        ///
        /// If @a playerSlot is not the same as local player slot then method call will be ignored and minus one is returned.
        ///
        /// <param name="playerSlot">Slot of the player.</param>
        /// <param name="entryNumber">Entry number used to identify the first entry.</param>
        /// <param name="values">An array of objects to set entries' new values.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetEntries(BattlePlayerSlot playerSlot, int entryNumber, object[] values)
        {
            if (playerSlot != s_localPlayerSlot) return;

            s_setEntriesFnRef(entryNumber, values);
        }

        /// <summary>Reference to local player slot</summary>
        private static BattlePlayerSlot s_localPlayerSlot;

        /// <summary>Function reference to <see cref="Battle.View.BattleDebugOverlay.AddEntry">BattleDebugOverlay.AddEntry</see></summary>
        private static Func<string, int> s_addEntryFnRef;

        /// <summary>Function reference to <see cref="Battle.View.BattleDebugOverlay.AddEntries">BattleDebugOverlay.AddEntries</see></summary>
        private static Func<string[], int> s_addEntriesFnRef;

        /// <summary>Function reference to <see cref="Battle.View.BattleDebugOverlay.SetEntry">BattleDebugOverlay.SetEntry</see></summary>
        private static Action<int, object> s_setEntryFnRef;

        /// <summary>Function reference to <see cref="Battle.View.BattleDebugOverlay.SetEntries">BattleDebugOverlay.SetEntries</see></summary>
        private static Action<int, object[]> s_setEntriesFnRef;
    }
}
