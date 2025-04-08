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
        public ServerGameStatistics gameStatistics { get; set; }
        public ServerPlayerTask DailyTask { get; set; } 
    }

    public class ServerGameStatistics
    {
        //public ServerChatMessages messages { get; set; }
        public int playedBattles { get; set; }
        public int wonBattles { get; set; }
        public int startedVotings { get; set; }
        public int participatedVotings { get; set; }
    }

    public class ServerChatMessages
    {
        public int date { get; set; }
        public int count { get; set; }
    }
}
