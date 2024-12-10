using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Voting
{

    public class PollVoteData
    {
        public string PlayerId;
        public string PlayerName;
        public bool Answer;

        public PollVoteData(string playerId, string playerName, bool answer)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Answer = answer;
        }
    }

    public class PlayerVoteData
    {
        public string PollId;
        public bool Answer;

        public PlayerVoteData(string pollId, bool answer)
        {
            PollId = pollId;
            Answer = answer;
        }
    }
}
