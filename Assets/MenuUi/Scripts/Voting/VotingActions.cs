using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Voting;
using UnityEngine;
using Altzone.Scripts.AvatarPartsInfo;
public static class VotingActions
{
    public static Action<GameFurniture> PassShopItem;
    public static Action<AvatarPartInfo> PassShopItemAvatar;
    public static Action PollPopupReady;
    public static Action<string> PassPollId;
    public static Action ReloadPollList;
}
