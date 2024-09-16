/// <summary>
/// Player object received from the server
/// </summary>
public class ServerPlayer
{
        public string _id { get; set; }
        public string name { get; set; }
        public int backpackCapacity { get; set; }
        public string uniqueIdentifier { get; set; }
        public string profile_id { get; set; }
        public string clan_id { get; set; }
        public bool? above13 { get; set; }
        public bool? parentalAuth { get; set; }
}
