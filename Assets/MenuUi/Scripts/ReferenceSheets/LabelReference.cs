using System;
using System.Collections;
using System.Collections.Generic;
using MenuUI.Scripts.SoulHome;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;

namespace MenuUi.Scripts.Clan
{
    [CreateAssetMenu(menuName = "ALT-Zone/LabelReference", fileName = "LabelReference")]
    public class LabelReference : ScriptableObject
    {
        [SerializeField] private List<LabelInfoObject> _info;


        public LabelInfoObject GetLabelInfo(ClanValues value)
        {
            LabelInfoObject data = GetLabelData(value);
            return (data);
        }

        public Sprite GetLabelImage(ClanValues value)
        {
            LabelInfoObject data = GetLabelData(value);
            return data.Image;
        }

        private LabelInfoObject GetLabelData(ClanValues value)
        {
            foreach (LabelInfoObject info in _info)
            {
                if (info.values == value)
                {
                    return info;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class LabelInfoObject
    {
        public string Name;
        public ClanValues values;
        public Sprite Image;
    }
}

