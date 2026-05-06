using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
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

        // These are used to show the moods in profile and to check if the player has inserted a mood today.
        // These might not yet be sent by the server, but they are needed for the mood selector and profile, so they are included here.
        //public List<string> playerDataEmotionList { get; set; }
        //public string emotionSelectorDate { get; set; }
        //public int daysBetweenInput { get; set; }
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
        public ServerAvatarPiece head { get; set; }
        public ServerAvatarPiece hair { get; set; }
        public ServerAvatarPiece eyes { get; set; }
        public ServerAvatarPiece nose { get; set; }
        public ServerAvatarPiece mouth { get; set; }
        public ServerAvatarPiece eyebrows { get; set; }
        public ServerAvatarPiece clothes { get; set; }
        public ServerAvatarPiece feet { get; set; }
        public ServerAvatarPiece hands { get; set; }
        public string skinColor { get; set; }


        public ServerAvatar() //This is needed because json parser tries to use the other one otherwise
        {

        }
        public ServerAvatar(AvatarData data)
        {
            head = new(0, "#FFFFFFFF");
            hair = new(data.Hair, data.HairColor);
            eyes = new(data.Eyes, data.EyesColor);
            nose = new(data.Nose, data.NoseColor);
            mouth = new(data.Mouth, data.MouthColor);
            eyebrows = new(0, "#FFFFFFFF");
            clothes = new(data.Clothes, data.ClothesColor);
            feet = new(data.Feet, data.FeetColor);
            hands = new(data.Hands, data.HandsColor);
            skinColor = data.Color;
        }
    }

    public class ServerAvatarPiece
    {
        public int id { get; set; }
        public string color { get; set; }

        public ServerAvatarPiece(int id, string color)
        {
            this.id = id;
            this.color = color != null ? color : "#FFFFFF";
        }
    }

    public class ServerChatMessages
    {
        public int date { get; set; }
        public int count { get; set; }
    }
}
