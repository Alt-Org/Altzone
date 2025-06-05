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

        public override string ToString()
        {
            return $"{nameof(clanName)}: {clanName}, {nameof(activity)}: {activity}, {nameof(age)}: {age}, {nameof(language)}: {language}" +
                   $", {nameof(goal)}: {goal}, {nameof(isOpen)}: {isOpen}";
        }
    }
}
