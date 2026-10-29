using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Model.Poco
{
    public class ServerStall
    {
        public ServerAdPoster adPoster { get; set; }
        public int maxSlots { get; set; }
    }

    public class ServerAdPoster
    {
        public string border { get; set; }
        public string colour { get; set; }
        public string mainFurniture { get; set; }
        public ServerAdText textField { get; set; }

    }

    public class ServerAdText
    {
        public string textString { get; set; }
        public string textFont { get; set; }
        public string textColor { get; set; }
        public float[] textPosition { get; set; }
    }
}
