using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Voting;
using UnityEngine;

public static class VotingActions
{
    public static Action<EsineDisplay> PassKauppaItem;
    public static Action PollPopupReady;
    public static Action<string> PassPollId;
}
