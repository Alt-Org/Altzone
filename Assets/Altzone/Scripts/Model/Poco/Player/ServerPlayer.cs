using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Assets.Altzone.Scripts.Model.Poco.Player;
/// <summary>
/// Player object received from the server
/// </summary>
///
namespace Altzone.Scripts.Model.Poco.Player
{
    public class ServerPlayer
    {
        public string _id { get; set; }
        public string name { get; set; }
        public int backpackCapacity { get; set; }
        public string uniqueIdentifier { get; set; }
        public string profile_id { get; set; }
        public string clan_id { get; set; }
        public int? currentAvatarId { get; set; }
        public string[] battleCharacter_ids { get; set; }
        public bool? above13 { get; set; }
        public bool? parentalAuth { get; set; }
        public int points { get; set; }
        public ServerAvatar avatar { get; set; }
        public ServerGameStatistics gameStatistics { get; set; }
        public ServerPlayerTask DailyTask { get; set; }
        public string clanRole_id { get; set; }
        public ClanLogo clanLogo { get; set; }
    }

    public class ServerGameStatistics
    {
        //public ServerChatMessages messages { get; set; }
        public int playedBattles { get; set; }
        public int wonBattles { get; set; }
        public int startedVotings { get; set; }
        public int participatedVotings { get; set; }
    }

    public class ServerAvatar
    {
        public int head { get; set; }
        public int hair { get; set; }
        public int eyes { get; set; }
        public int nose { get; set; }
        public int mouth { get; set; }
        public int eyebrows { get; set; }
        public int clothes { get; set; }
        public int feet { get; set; }
        public int hands { get; set; }
        public string skinColor { get; set; }


        public ServerAvatar() //This is needed because json parser tries to use the other one otherwise
        {

        }
        public ServerAvatar(AvatarData data)
        {
            head = 0;
            hair = data.Hair;
            eyes = data.Eyes;
            nose = data.Nose;
            mouth = data.Mouth;
            eyebrows = 0;
            clothes = data.Clothes;
            feet = data.Feet;
            hands = data.Hands;
            skinColor = data.Color;
        }
    }

    public class ServerChatMessages
    {
        public int date { get; set; }
        public int count { get; set; }
    }
}
