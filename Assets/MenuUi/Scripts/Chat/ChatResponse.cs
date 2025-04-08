using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    [CreateAssetMenu(menuName = "ALT-Zone/ChatResponse", fileName = "ChatResponse_name")]
    public class ChatResponse : ScriptableObject
    {
        [SerializeField]
        private CharacterClassID _classId;
        [SerializeField]
        private List<string> _list;

        public CharacterClassID ClassId { get => _classId; }

        public List<string> List { get => _list; }
    }
}
