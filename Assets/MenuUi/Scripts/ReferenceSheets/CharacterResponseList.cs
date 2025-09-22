using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(menuName = "ALT-Zone/ChatResponseList", fileName = "ChatResponseList")]
    public class CharacterResponseList : ScriptableObject
    {
        [SerializeField] private List<ChatResponse> _chatResponseInfo;

        [SerializeField] private ChatResponse _defaultList;

        [SerializeField] private List<MottoOptions> _mottoOptionsInfo;

        public List<ChatResponse> ChatResponseInfo => _chatResponseInfo; // Public accessor for _chatResponseInfo
        public List<MottoOptions> MottoOptionsInfo => _mottoOptionsInfo; // Public accessor for _mottoOptionsInfo

        public List<string> GetChatResponses(CharacterClassID classId)
        {

            List<string> list = _chatResponseInfo.FirstOrDefault(x => x.ClassId == classId)?.List;
            if (list == null) list = _defaultList.List;
            return list;
        }

        public List<string> GetMottoOptions(CharacterClassID classId)
        {
            List<string> list = _mottoOptionsInfo.FirstOrDefault(x => x.ClassID == classId)?.List;
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
