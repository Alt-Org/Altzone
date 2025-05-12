using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    [CreateAssetMenu(menuName = "ALT-Zone/MottoOptions", fileName = "MottoOptions_name")]
    public class MottoOptions : ScriptableObject
    {
        [SerializeField] private CharacterClassID _classID;
        [SerializeField] private List<string> _list;

        public CharacterClassID ClassID { get => _classID; }
        public List<string> List { get => _list; }
    }
}

