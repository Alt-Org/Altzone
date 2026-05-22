using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Altzone.Scripts.Common;
using Altzone.Scripts.Model.Poco.Clan;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco.Player
{
    public class DailyEmotion
    {
        private Emotion _emotion;
        private DateTime _dateTime;

        public Emotion Emotion { get => _emotion; }
        public DateTime DateTime { get => _dateTime; }

        public DailyEmotion() { }

        public DailyEmotion(ServerEmotions serverEmotion)
        {
            try
            {
                _emotion = (Emotion)Enum.Parse(typeof(Emotion), serverEmotion.emotion);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            _dateTime = DateTime.Parse(serverEmotion.date);
        }
    }
}
