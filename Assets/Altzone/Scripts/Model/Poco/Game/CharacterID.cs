namespace Altzone.Scripts.Model.Poco.Game
{
    public enum CharacterID
    {
        None = 0,

        // Desensitizers class characters
        Racist = CharacterClassID.Desensitizer + 1,
        Bodybuilder = CharacterClassID.Desensitizer + 2,
        Jingoist = CharacterClassID.Desensitizer + 3,

        // Tricksters class characters
        Comedian = CharacterClassID.Trickster + 1,
        Joker = CharacterClassID.Trickster + 2,
        Conman = CharacterClassID.Trickster + 3,
        Seducer = CharacterClassID.Trickster + 4, //Alternatively Womanizer

        // Obedientes class characters
        Religious = CharacterClassID.Obedient + 1,
        Yesman = CharacterClassID.Obedient + 2,

        // Projectors class characters
        Artist = CharacterClassID.Projector + 1,
        Arguer = CharacterClassID.Projector + 2,

        // Retroflectors class characters
        Overeater = CharacterClassID.Retroflector + 1,
        Alcoholic = CharacterClassID.Retroflector + 2,

        // Confluents class characters
        Soulsisters = CharacterClassID.Confluent + 1,
        Lovers = CharacterClassID.Confluent + 2,
        SleepyHead = CharacterClassID.Confluent + 3,

        // Intellectualizers class characters
        Booksmart = CharacterClassID.Intellectualizer + 1,
        Capitalist = CharacterClassID.Intellectualizer + 2
    }
}
