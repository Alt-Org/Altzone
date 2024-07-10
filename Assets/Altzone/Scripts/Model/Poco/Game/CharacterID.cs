namespace Altzone.Scripts.Model.Poco.Game
{
    public enum CharacterID
    {
        None = 0,

        // Desensitizers class characters
        DesensitizerBodybuilder = CharacterClassID.Desensitizer | 1,

        // Tricksters class characters
        TricksterComedian = CharacterClassID.Trickster | 1,

        // Obedientes class characters
        ObedientPreacher = CharacterClassID.Obedient | 1,

        // Projectors class characters
        ProjectorGrafitiartist = CharacterClassID.Projector | 1,

        // Retroflectors class characters
        RetroflectorOvereater = CharacterClassID.Retroflector | 1,
        RetroflectorAlcoholic = CharacterClassID.Retroflector | 2,

        // Confluents class characters
        ConfluentBesties = CharacterClassID.Confluent | 1,

        // Intellectualizers class characters
        IntellectualizerResearcher = CharacterClassID.Intellectualizer | 1
    }
}
