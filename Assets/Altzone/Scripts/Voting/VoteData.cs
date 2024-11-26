using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.Voting
{
    public class VoteData
    {
        public string PlayerId;
        public string PollId;
        public bool Answer;

        public VoteData(string playerId, string pollId, bool answer)
        {
            PlayerId = playerId;
            PollId = pollId;
            Answer = answer;
        }
    }
}
