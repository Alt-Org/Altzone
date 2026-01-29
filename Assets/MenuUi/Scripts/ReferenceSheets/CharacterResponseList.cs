using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Chat;
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

        public List<string> GetChatResponses(CharacterClassType classType)
        {

            List<string> list = _chatResponseInfo.FirstOrDefault(x => x.ClassType == classType)?.List;




            /*switch(mood)
            {
                //Checks which Mood been selected and imports it
                case Mood.Neutral:
                    list = _defaultList.List;
                    break;

                case Mood.Happy:
                    list = _ChatResponseReactionInfo[0].List;
                    break;

                case Mood.Sad:
                    list = _ChatResponseReactionInfo[1].List;
                    break;

                case Mood.Angry:
                    list = _ChatResponseReactionInfo[2].List;
                    break;

                case Mood.Love:
                    list = _ChatResponseReactionInfo[3].List;
                    break;

                case Mood.Wink:
                    list = _ChatResponseReactionInfo[4].List;
                    break;
            }*/

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
    public class ChatResponseObject
    {
        public string ResponseId;
        public string Response;
    }

    [Serializable]
    public class CharacterChatResponseObject
    {

        public CharacterClassType ClassType;

        public ChatResponse ResponseObject;
    }
}
