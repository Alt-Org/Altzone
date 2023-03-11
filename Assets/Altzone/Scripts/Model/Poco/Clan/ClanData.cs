using System;
using System.Diagnostics.CodeAnalysis;

namespace Altzone.Scripts.Model.Poco.Clan
{
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ClanData
    {
        public int Id;
        public string Name;
        public string Tag;
        public int GameCoins;
    }
}