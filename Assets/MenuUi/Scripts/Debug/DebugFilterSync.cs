using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugFilterSync : MonoBehaviour
    {
        public delegate void FilterStatusChanged(DebugFilterSyncHandler originHandler, MessageType filter = MessageType.None, bool value = false, bool? empty = null);
        public static event FilterStatusChanged OnFilterStatusChanged;

        public delegate void SourceFilterStatusChanged(DebugFilterSyncHandler originHandler, int filter, bool value);
        public static event SourceFilterStatusChanged OnSourceFilterStatusChanged;

        private void OnEnable()
        {
            DebugFilterSyncHandler.OnFilterStatusChanged += SyncFilters;
            DebugFilterSyncHandler.OnSourceFilterStatusChanged += SyncSourceFilters;
        }

        private void OnDisable()
        {
            DebugFilterSyncHandler.OnFilterStatusChanged -= SyncFilters;
            DebugFilterSyncHandler.OnSourceFilterStatusChanged -= SyncSourceFilters;
        }


        private void SyncFilters(DebugFilterSyncHandler originHandler, MessageType filter = MessageType.None, bool value = false, bool? empty = null)
        {
            OnFilterStatusChanged(originHandler,filter, value, empty);
        }

        private void SyncSourceFilters(DebugFilterSyncHandler originHandler, int filter, bool value)
        {
            OnSourceFilterStatusChanged(originHandler, filter, value);
        }
    }
}
