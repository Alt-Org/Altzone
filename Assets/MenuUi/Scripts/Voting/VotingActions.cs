using System;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Voting;
using Altzone.Scripts.AvatarPartsInfo;
public static class VotingActions
{
    public static Action<GameFurniture> PassShopItem;
    public static Action<AvatarPartInfo> PassShopItemAvatar;
    public static Action<string> ShopItemBought;
    public static Action<string> ShopItemInVoting;
    public static Action<string> AvatarShopItemBought;
    public static Action PollPopupReady;
    public static Action<string> PassPollId;
    public static Action ReloadPollList;
}
