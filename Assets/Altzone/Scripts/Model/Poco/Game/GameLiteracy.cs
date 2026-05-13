public static class GameLiteracy
{

    public static string Get(GameLiteracyType literacy, SettingsCarrier.LanguageType language)
    {
        if (language == SettingsCarrier.LanguageType.English)
        {
            switch (literacy)
            {
                case GameLiteracyType.None:
                    return string.Empty;

                case GameLiteracyType.InterfaceNavigation:
                    return "Interface navigation";

                case GameLiteracyType.GameMechanics:
                    return "Game mechanics";

                case GameLiteracyType.GameConstraints:
                    return "Game constraints";

                case GameLiteracyType.Creativity:
                    return "Creativity";

                case GameLiteracyType.Communication:
                    return "Communication";

                case GameLiteracyType.DecisionMaking:
                    return "Decision-making";

                case GameLiteracyType.ResourceSharing:
                    return "Resource sharing";

                case GameLiteracyType.Negotiation:
                    return "Negotiation";

                case GameLiteracyType.GameStory:
                    return "Game Story";

                case GameLiteracyType.Instructions:
                    return "Instructions";

                case GameLiteracyType.VisualCoherence:
                    return "Visual coherence";

                case GameLiteracyType.Symbolism:
                    return "Symbolism";

                case GameLiteracyType.CulturalReferences:
                    return "Cultural references";

                case GameLiteracyType.GameGenresAndTypes:
                    return "Game genres and types";

                case GameLiteracyType.GamePracticeCulture:
                    return "Game practice culture";

                case GameLiteracyType.SocialTheme:
                    return "Social theme";

                case GameLiteracyType.IntertextualConnections:
                    return "Intertextual connections";

                case GameLiteracyType.Consumption:
                    return "Consumption";

                case GameLiteracyType.RewardingAndTemptation:
                    return "Rewarding and temptation";

                case GameLiteracyType.MoralAction:
                    return "Moral action";

                case GameLiteracyType.SustainableDevelopment:
                    return "Sustainable development";

                case GameLiteracyType.Values:
                    return "Values";

                default:
                    return literacy.ToString();
            }
        }
        else
        {
            switch (literacy)
            {
                case GameLiteracyType.None:
                    return string.Empty;

                case GameLiteracyType.InterfaceNavigation:
                    return "Käyttöliittymän hallinta";

                case GameLiteracyType.GameMechanics:
                    return "Pelimekaniikka";

                case GameLiteracyType.GameConstraints:
                    return "Pelin rajallisuus";

                case GameLiteracyType.Creativity:
                    return "Luovuus";

                case GameLiteracyType.Communication:
                    return "Viestintä";

                case GameLiteracyType.DecisionMaking:
                    return "Päätöksenteko";

                case GameLiteracyType.ResourceSharing:
                    return "Resurssien jakaminen";

                case GameLiteracyType.Negotiation:
                    return "Yhteisön ja yksilön identiteetti";

                case GameLiteracyType.GameStory:
                    return "Pelin tarina";

                case GameLiteracyType.Instructions:
                    return "Ohjeistus";

                case GameLiteracyType.VisualCoherence:
                    return "Visuaalinen yhtenäisyys";

                case GameLiteracyType.Symbolism:
                    return "Symboliikka";

                case GameLiteracyType.CulturalReferences:
                    return "Kulttuuriset viittaukset";

                case GameLiteracyType.GameGenresAndTypes:
                    return "Peligenret ja lajityypit";

                case GameLiteracyType.GamePracticeCulture:
                    return "Pelin toimintakulttuuri";

                case GameLiteracyType.SocialTheme:
                    return "Yhteiskunnallinen teema";

                case GameLiteracyType.IntertextualConnections:
                    return "Intertekstuaaliset yhteydet";

                case GameLiteracyType.Consumption:
                    return "Kuluttaminen";

                case GameLiteracyType.RewardingAndTemptation:
                    return "Palkitseminen ja houkuttelu";

                case GameLiteracyType.MoralAction:
                    return "Moraalinen toiminta";

                case GameLiteracyType.SustainableDevelopment:
                    return "Kestävä kehitys";

                case GameLiteracyType.Values:
                    return "Arvot";

                default:
                    return literacy.ToString();
            }
        }
    }


}
public enum GameLiteracyType
{
    None = 0,
    InterfaceNavigation = 1,
    GameMechanics = 2,
    GameConstraints = 3,
    Creativity = 4,
    Communication = 5,
    DecisionMaking = 6,
    ResourceSharing = 7,
    Negotiation = 8,
    GameStory = 9,
    Instructions = 10,
    VisualCoherence = 11,
    Symbolism = 12,
    CulturalReferences = 13,
    GameGenresAndTypes = 14,
    GamePracticeCulture = 15,
    SocialTheme = 16,
    IntertextualConnections = 17,
    Consumption = 18,
    RewardingAndTemptation = 19,
    MoralAction = 20,
    SustainableDevelopment = 21,
    Values = 22
}
