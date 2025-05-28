using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Voting;
using UnityEngine;

namespace Altzone.Scripts.Voting
{
    public class ServerPoll
    {
        public string _id { get; set; }
        public PollOrganizer organizer { get; set; }
        public string endedAt { get; set; }
        public string startedAt { get; set; }
        public string endsOn { get; set; }
        public string type { get; set; }
        public string[] player_ids { get; set; }
        public int minPercentage { get; set; }
        public PollVote[] votes { get; set; }
        public string entity_id { get; set; }
        public string entity_name { get; set; }

    }

    public class PollOrganizer
    {
        public string player_id { get; set; }
        public string clan_id { get; set; }
    }

    public class PollVote
    {
        public string player_id { get; set; }
        public string choice { get; set; }
    }
}
