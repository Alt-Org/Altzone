using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Chat;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeField, FormerlySerializedAs("ResponseId")] private ResponseType _responseId;
        [SerializeField, FormerlySerializedAs("Response")] private string _responseFI;
        [SerializeField] private string _responseEN;

        public ResponseType ResponseId => _responseId;
        public string Response
        {
            get
            {
                if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
                    return _responseFI;
                else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
                    return _responseEN;
                else
                    return _responseFI;
            }
        }
    }

}
