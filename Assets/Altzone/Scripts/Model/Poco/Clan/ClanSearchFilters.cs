namespace Altzone.Scripts.Model.Poco.Clan
{
    public struct ClanSearchFilters
    {
        public string clanName;
        public ClanActivity activity;
        public ClanAge age;
        public Language language;
        public Goals goal;
        public bool isOpen;
        public ClanRanking ranking;        // Muutettu RankingOption → ClanRanking
        public ClanMembers memberCount;    // Muutettu MemberCountOption → ClanMembers
        
        public override string ToString()
        {
            return $"{nameof(clanName)}: {clanName}, {nameof(activity)}: {activity}, {nameof(age)}: {age}, {nameof(language)}: {language}" +
                   $", {nameof(goal)}: {goal}, {nameof(isOpen)}: {isOpen}, {nameof(ranking)}: {ranking}, {nameof(memberCount)}: {memberCount}";
        }
    }
}


