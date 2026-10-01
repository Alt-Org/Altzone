using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Chat;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    [CreateAssetMenu(menuName = "ALT-Zone/ChatResponse", fileName = "ChatResponse_name")]
    public class ChatResponse : ScriptableObject
    {
        [SerializeField]
        private CharacterClassType _classType;
        [SerializeField]
        private List<ChatResponseObject> _list;

        public CharacterClassType ClassType { get => _classType; }

        public List<ChatResponseObject> List { get => _list; }
    }


    [Serializable]
    public class ChatResponseObject
    {
        public ResponseType ResponseId;
        public string Response;
    }

}
