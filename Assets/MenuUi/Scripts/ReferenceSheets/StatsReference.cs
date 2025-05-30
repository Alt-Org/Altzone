using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    //[CreateAssetMenu(menuName = "ALT-Zone/StatsReference", fileName = "StatsReference")]
    public class StatsReference : ScriptableObject
    {
        [SerializeField] private List<StatInfo> _info;
        public StatInfo GetStatInfo(StatType statType)
        {
            foreach (StatInfo info in _info)
            {
                if (info.StatType == statType)
                {
                    return info;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class StatInfo
    {
        public StatType StatType;
        public string Name;
        public Sprite Image;
        public Color StatBoxColor;
        public string Description;
    }
}
