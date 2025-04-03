using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(menuName = "ALT-Zone/ChatResponseList", fileName = "ChatResponseList")]
    public class ChatResponseList : ScriptableObject
    {
        [SerializeField] private List<ChatResponse> _info;

        [SerializeField] private ChatResponse _defaultList;

        public List<ChatResponse> Info => _info; // Public accessor for _info

        public List<string> GetResponses(CharacterClassID classId)
        {

            List<string> list = _info.FirstOrDefault(x => x.ClassId == classId)?.List;
            if (list == null) list = _defaultList.List;
            return list;
        }
    }

    [Serializable]
    public class ChatResponseObject
    {
        public string ResponseId;
        public string Response;
    }

    [Serializable]
    public class CharacterChatResponseObject
    {

        public CharacterClassID ClassId;

        public ChatResponse ResponseObject;
    }
}
