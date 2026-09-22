using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Chat;
using Altzone.Scripts.Common;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(menuName = "ALT-Zone/ChatResponseList", fileName = "ChatResponseList")]
    public class CharacterResponseList : ScriptableObject
    {
        [SerializeField] private List<ChatResponse> _chatResponseInfo;

        [SerializeField] private List<ChatResponse> _chatResponseReactionInfo;

        [SerializeField] private ChatResponse _defaultList;

        [SerializeField] private List<MottoOptions> _mottoOptionsInfo;

        public List<ChatResponse> ChatResponseInfo => _chatResponseInfo; // Public accessor for _chatResponseInfo
        public List<MottoOptions> MottoOptionsInfo => _mottoOptionsInfo; // Public accessor for _mottoOptionsInfo

        public List<ChatResponseObject> GetChatResponses(Emotion mood)
        {

            List<ChatResponseObject> list = null;
            switch (mood)
            {
                case Emotion.Blank:
                    list = _defaultList.List;
                    break;

                case Emotion.Sorrow:
                    list = _chatResponseInfo[0].List;
                    break;

                case Emotion.Anger:
                    list = _chatResponseInfo[1].List;
                    break;

                case Emotion.Joy:
                    list = _chatResponseInfo[2].List;
                    break;

                case Emotion.Playful:
                    list = _chatResponseInfo[3].List;
                    break;

                case Emotion.Love:
                    list = _chatResponseInfo[4].List;
                    break;
            }

            if (list == null)
                return _defaultList.List; //_defaultList.List;

            return list;




        }

        public List<string> GetMottoOptions(CharacterClassType classType)
        {
            List<string> list = _mottoOptionsInfo.FirstOrDefault(x => x.ClassType == classType)?.List;
            return list;
        }
    }

    [Serializable]
    public class CharacterChatResponseObject
    {

        public CharacterClassType ClassType;

        public ChatResponse ResponseObject;
    }
}
