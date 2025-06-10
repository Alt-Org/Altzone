namespace Altzone.Scripts.Model.Poco.Game
{
    public enum CharacterID
    {
        None = -1,

        // Test character
        Test = 0,

        // Desensitizers class characters
        DesensitizerTest = CharacterClassID.Desensitizer,
        Racist = CharacterClassID.Desensitizer + 1, //101
        Bodybuilder = CharacterClassID.Desensitizer + 2, //102
        WarVeteran = CharacterClassID.Desensitizer + 3, //103
        Bully = CharacterClassID.Desensitizer + 4, //104

        // Tricksters class characters
        Comedian = CharacterClassID.Trickster + 1, //201
        Joker = CharacterClassID.Trickster + 2, //202
        Conman = CharacterClassID.Trickster + 3, //203
        Seducer = CharacterClassID.Trickster + 4, //204

        // Obedientes class characters
        Religious = CharacterClassID.Obedient + 1, //301
        Yesman = CharacterClassID.Obedient + 2, //302

        // Projectors class characters
        Artist = CharacterClassID.Projector + 1, //401
        Arguer = CharacterClassID.Projector + 2, //402
        Reflector = CharacterClassID.Projector + 3, //403

        // Retroflectors class characters
        Overeater = CharacterClassID.Retroflector + 1, //501
        Alcoholic = CharacterClassID.Retroflector + 2, //502
        Anorectic = CharacterClassID.Retroflector + 3, //503
        Stoner = CharacterClassID.Retroflector + 4, //504

        // Confluents class characters
        ConfluentTest = CharacterClassID.Confluent,
        Soulsisters = CharacterClassID.Confluent + 1, //601
        Lovers = CharacterClassID.Confluent + 2, //602
        SleepyHead = CharacterClassID.Confluent + 3, //603

        // Intellectualizers class characters
        Booksmart = CharacterClassID.Intellectualizer + 1, //701
        Capitalist = CharacterClassID.Intellectualizer + 2, //702
        ObsessiveCompulsive = CharacterClassID.Intellectualizer + 3 //703
    }
}
