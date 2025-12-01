namespace Altzone.Scripts.Model.Poco.Game
{
    public enum CharacterID
    {
        None = -1,

        // Test character
        Test = 0,

        // Desensitizer class characters
        DesensitizerTest = CharacterClassType.Desensitizer,
        Racist = CharacterClassType.Desensitizer + 1, //101
        Bodybuilder = CharacterClassType.Desensitizer + 2, //102
        WarVeteran = CharacterClassType.Desensitizer + 3, //103
        Bully = CharacterClassType.Desensitizer + 4, //104
        Egoist = CharacterClassType.Desensitizer + 5, //105
        Depressed = CharacterClassType.Desensitizer + 6, //106

        // Trickster class characters
        TricksterTest = CharacterClassType.Trickster,
        Comedian = CharacterClassType.Trickster + 1, //201
        Joker = CharacterClassType.Trickster + 2, //202
        Conman = CharacterClassType.Trickster + 3, //203
        Seducer = CharacterClassType.Trickster + 4, //204

        // Obedient class characters
        ObedientTest = CharacterClassType.Obedient,
        Religious = CharacterClassType.Obedient + 1, //301
        Yesman = CharacterClassType.Obedient + 2, //302
        SlaveOfTheLaw = CharacterClassType.Obedient + 3, //303
        FashionSlave = CharacterClassType.Obedient + 4, //304
        MammasBoy = CharacterClassType.Obedient + 5, //305
        Superstitious = CharacterClassType.Obedient + 6, //306

        // Projector class characters
        ProjectorTest = CharacterClassType.Projector,
        Artist = CharacterClassType.Projector + 1, //401
        Arguer = CharacterClassType.Projector + 2, //402
        Reflector = CharacterClassType.Projector + 3, //403
        Delusional = CharacterClassType.Projector + 4, //404

        // Retroflector class characters
        RetroflectorTest = CharacterClassType.Retroflector,
        Overeater = CharacterClassType.Retroflector + 1, //501
        Alcoholic = CharacterClassType.Retroflector + 2, //502
        Anorectic = CharacterClassType.Retroflector + 3, //503
        Stoner = CharacterClassType.Retroflector + 4, //504
        Martyr = CharacterClassType.Retroflector + 5, //505
        Suicidal = CharacterClassType.Retroflector + 6, //506

        // Confluent class characters
        ConfluentTest = CharacterClassType.Confluent,
        Soulsisters = CharacterClassType.Confluent + 1, //601
        Lovers = CharacterClassType.Confluent + 2, //602
        SleepyHead = CharacterClassType.Confluent + 3, //603
        Tribalist = CharacterClassType.Confluent + 4, //604
        GangBanger = CharacterClassType.Confluent + 5, //605

        // Intellectualizer class characters
        IntellectualizerTest = CharacterClassType.Intellectualizer,
        Booksmart = CharacterClassType.Intellectualizer + 1, //701
        Capitalist = CharacterClassType.Intellectualizer + 2, //702
        ObsessiveCompulsive = CharacterClassType.Intellectualizer + 3, //703
        Overcompilator = CharacterClassType.Intellectualizer + 4, //704
        Nitpicker = CharacterClassType.Intellectualizer + 5 //705
    }
}
