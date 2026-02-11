using Quantum;

namespace Assets.Altzone.Scripts.Reference_Sheets
{
    public enum BattleSFXNameTypes
    {
        None                  = -1,
        SoulWallHit           = BattleSoundFX.SoulWallHit,
        GoalHit               = BattleSoundFX.GoalHit,
        SideWallHit           = BattleSoundFX.SideWallHit,
        SoulWallBroken        = BattleSoundFX.SoulWallBroken,
        DiamondPickUp         = BattleSoundFX.DiamondPickUp,
        SoulWallHitAggression = BattleSoundFX.SoulWallHitAggression,
        SoulWallHitJoy        = BattleSoundFX.SoulWallHitJoy,
        SoulWallHitLove       = BattleSoundFX.SoulWallHitLove,
        SoulWallHitPlayful    = BattleSoundFX.SoulWallHitPlayful,
        SoulWallHitSadness    = BattleSoundFX.SoulWallHitSadness,

        #region

        // Racist
        PlayerCharacterRacistCatchphrase            = BattleSoundFX.PlayerCharacterRacistCatchphrase,
        PlayerCharacterRacistHitCharacterAggression = BattleSoundFX.PlayerCharacterRacistHitCharacterAggression,
        PlayerCharacterRacistHitCharacterJoy        = BattleSoundFX.PlayerCharacterRacistHitCharacterJoy,
        PlayerCharacterRacistHitCharacterLove       = BattleSoundFX.PlayerCharacterRacistHitCharacterLove,
        PlayerCharacterRacistHitCharacterPlayful    = BattleSoundFX.PlayerCharacterRacistHitCharacterPlayful,
        PlayerCharacterRacistHitCharacterSadness    = BattleSoundFX.PlayerCharacterRacistHitCharacterSadness,
        PlayerCharacterRacistHitShield              = BattleSoundFX.PlayerCharacterRacistHitShield,
        PlayerCharacterRacistDeath                  = BattleSoundFX.PlayerCharacterRacistDeath,

        // BodyBuilder
        PlayerCharacterBodybuilderCatchphrase            = BattleSoundFX.PlayerCharacterBodybuilderCatchphrase,
        PlayerCharacterBodybuilderHitCharacterAggression = BattleSoundFX.PlayerCharacterBodybuilderHitCharacterAggression,
        PlayerCharacterBodybuilderHitCharacterJoy        = BattleSoundFX.PlayerCharacterBodybuilderHitCharacterJoy,
        PlayerCharacterBodybuilderHitCharacterLove       = BattleSoundFX.PlayerCharacterBodybuilderHitCharacterLove,
        PlayerCharacterBodybuilderHitCharacterPlayful    = BattleSoundFX.PlayerCharacterBodybuilderHitCharacterPlayful,
        PlayerCharacterBodybuilderHitCharacterSadness    = BattleSoundFX.PlayerCharacterBodybuilderHitCharacterSadness,
        PlayerCharacterBodybuilderHitShield              = BattleSoundFX.PlayerCharacterBodybuilderHitShield,
        PlayerCharacterBodybuilderDeath                  = BattleSoundFX.PlayerCharacterBodybuilderDeath,

        // WarVeteran
        PlayerCharacterWarVeteranCatchphrase            = BattleSoundFX.PlayerCharacterWarVeteranCatchphrase,
        PlayerCharacterWarVeteranHitCharacterAggression = BattleSoundFX.PlayerCharacterWarVeteranHitCharacterAggression,
        PlayerCharacterWarVeteranHitCharacterJoy        = BattleSoundFX.PlayerCharacterWarVeteranHitCharacterJoy,
        PlayerCharacterWarVeteranHitCharacterLove       = BattleSoundFX.PlayerCharacterWarVeteranHitCharacterLove,
        PlayerCharacterWarVeteranHitCharacterPlayful    = BattleSoundFX.PlayerCharacterWarVeteranHitCharacterPlayful,
        PlayerCharacterWarVeteranHitCharacterSadness    = BattleSoundFX.PlayerCharacterWarVeteranHitCharacterSadness,
        PlayerCharacterWarVeteranHitShield              = BattleSoundFX.PlayerCharacterWarVeteranHitShield,
        PlayerCharacterWarVeteranDeath                  = BattleSoundFX.PlayerCharacterWarVeteranDeath,

        // Bully
        PlayerCharacterBullyCatchphrase            = BattleSoundFX.PlayerCharacterBullyCatchphrase,
        PlayerCharacterBullyHitCharacterAggression = BattleSoundFX.PlayerCharacterBullyHitCharacterAggression,
        PlayerCharacterBullyHitCharacterJoy        = BattleSoundFX.PlayerCharacterBullyHitCharacterJoy,
        PlayerCharacterBullyHitCharacterLove       = BattleSoundFX.PlayerCharacterBullyHitCharacterLove,
        PlayerCharacterBullyHitCharacterPlayful    = BattleSoundFX.PlayerCharacterBullyHitCharacterPlayful,
        PlayerCharacterBullyHitCharacterSadness    = BattleSoundFX.PlayerCharacterBullyHitCharacterSadness,
        PlayerCharacterBullyHitShield              = BattleSoundFX.PlayerCharacterBullyHitShield,
        PlayerCharacterBullyDeath                  = BattleSoundFX.PlayerCharacterBullyDeath,

        // Egoist
        PlayerCharacterEgoistCatchphrase  = BattleSoundFX.PlayerCharacterEgoistCatchphrase,
        PlayerCharacterEgoistHitCharacterAggression = BattleSoundFX.PlayerCharacterEgoistHitCharacterAggression,
        PlayerCharacterEgoistHitCharacterJoy = BattleSoundFX.PlayerCharacterEgoistHitCharacterJoy,
        PlayerCharacterEgoistHitCharacterLove = BattleSoundFX.PlayerCharacterEgoistHitCharacterLove,
        PlayerCharacterEgoistHitCharacterPlayful = BattleSoundFX.PlayerCharacterEgoistHitCharacterPlayful,
        PlayerCharacterEgoistHitCharacterSadness = BattleSoundFX.PlayerCharacterEgoistHitCharacterSadness,
        PlayerCharacterEgoistHitShield    = BattleSoundFX.PlayerCharacterEgoistHitShield,
        PlayerCharacterEgoistDeath        = BattleSoundFX.PlayerCharacterEgoistDeath,

        // Depressed
        PlayerCharacterDepressedCatchphrase            = BattleSoundFX.PlayerCharacterDepressedCatchphrase,
        PlayerCharacterDepressedHitCharacterAggression = BattleSoundFX.PlayerCharacterDepressedHitCharacterAggression,
        PlayerCharacterDepressedHitCharacterJoy        = BattleSoundFX.PlayerCharacterDepressedHitCharacterJoy,
        PlayerCharacterDepressedHitCharacterLove       = BattleSoundFX.PlayerCharacterDepressedHitCharacterLove,
        PlayerCharacterDepressedHitCharacterPlayful    = BattleSoundFX.PlayerCharacterDepressedHitCharacterPlayful,
        PlayerCharacterDepressedHitCharacterSadness    = BattleSoundFX.PlayerCharacterDepressedHitCharacterSadness,
        PlayerCharacterDepressedHitShield              = BattleSoundFX.PlayerCharacterDepressedHitShield,
        PlayerCharacterDepressedDeath                  = BattleSoundFX.PlayerCharacterDepressedDeath,

        // Comedian
        PlayerCharacterComedianCatchphrase           = BattleSoundFX.PlayerCharacterComedianCatchphrase,
        PlayerCharacterComedianHitCharacterAggresion = BattleSoundFX.PlayerCharacterComedianHitCharacterAggression,
        PlayerCharacterComedianHitCharacterJoy       = BattleSoundFX.PlayerCharacterComedianHitCharacterJoy,
        PlayerCharacterComedianHitCharacterLove      = BattleSoundFX.PlayerCharacterComedianHitCharacterLove,
        PlayerCharacterComedianHitCharacterPlayful = BattleSoundFX.PlayerCharacterComedianHitCharacterPlayful,
        PlayerCharacterComedianHitCharacterSadness   = BattleSoundFX.PlayerCharacterComedianHitCharacterSadness,
        PlayerCharacterComedianHitShield             = BattleSoundFX.PlayerCharacterComedianHitShield,
        PlayerCharacterComedianDeath                 = BattleSoundFX.PlayerCharacterComedianDeath,

        // Joker
        PlayerCharacterJokerCatchphrase            = BattleSoundFX.PlayerCharacterJokerCatchphrase,
        PlayerCharacterJokerHitCharacterAggression = BattleSoundFX.PlayerCharacterJokerHitCharacterAggression,
        PlayerCharacterJokerHitCharacterJoy        = BattleSoundFX.PlayerCharacterJokerHitCharacterJoy,
        PlayerCharacterJokerHitCharacterLove       = BattleSoundFX.PlayerCharacterJokerHitCharacterLove,
        PlayerCharacterJokerHitCharacterPlayful    = BattleSoundFX.PlayerCharacterJokerHitCharacterPlayful,
        PlayerCharacterJokerHitCharacterSadness    = BattleSoundFX.PlayerCharacterJokerHitCharacterSadness,
        PlayerCharacterJokerHitShield              = BattleSoundFX.PlayerCharacterJokerHitShield,
        PlayerCharacterJokerDeath                  = BattleSoundFX.PlayerCharacterJokerDeath,

        // Conman
        PlayerCharacterConmanCatchphrase            = BattleSoundFX.PlayerCharacterConmanCatchphrase,
        PlayerCharacterConmanHitCharacterAggression = BattleSoundFX.PlayerCharacterConmanHitCharacterAggression,
        PlayerCharacterConmanHitCharacterJoy        = BattleSoundFX.PlayerCharacterConmanHitCharacterJoy,
        PlayerCharacterConmanHitCharacterLove       = BattleSoundFX.PlayerCharacterConmanHitCharacterLove,
        PlayerCharacterConmanHitCharacterPlayful    = BattleSoundFX.PlayerCharacterConmanHitCharacterPlayful,
        PlayerCharacterConmanHitCharacterSadness    = BattleSoundFX.PlayerCharacterConmanHitCharacterSadness,
        PlayerCharacterConmanHitShield              = BattleSoundFX.PlayerCharacterConmanHitShield,
        PlayerCharacterConmanDeath                  = BattleSoundFX.PlayerCharacterConmanDeath,

        // Seducer
        PlayerCharacterSeducerCatchphrase            = BattleSoundFX.PlayerCharacterSeducerCatchphrase,
        PlayerCharacterSeducerHitCharacterAggression = BattleSoundFX.PlayerCharacterSeducerHitCharacterAggression,
        PlayerCharacterSeducerHitCharacterJoy        = BattleSoundFX.PlayerCharacterSeducerHitCharacterJoy,
        PlayerCharacterSeducerHitCharacterLove       = BattleSoundFX.PlayerCharacterSeducerHitCharacterLove,
        PlayerCharacterSeducerHitCharacterPlayful    = BattleSoundFX.PlayerCharacterSeducerHitCharacterPlayful,
        PlayerCharacterSeducerHitCharacterSadness    = BattleSoundFX.PlayerCharacterSeducerHitCharacterSadness,
        PlayerCharacterSeducerHitShield              = BattleSoundFX.PlayerCharacterSeducerHitShield,
        PlayerCharacterSeducerDeath                  = BattleSoundFX.PlayerCharacterSeducerDeath,

        // Religious
        PlayerCharacterReligiousCatchphrase            = BattleSoundFX.PlayerCharacterReligiousCatchphrase,
        PlayerCharacterReligiousHitCharacterAggression = BattleSoundFX.PlayerCharacterReligiousHitCharacterAggression,
        PlayerCharacterReligiousHitCharacterJoy        = BattleSoundFX.PlayerCharacterReligiousHitCharacterJoy,
        PlayerCharacterReligiousHitCharacterPlayful    = BattleSoundFX.PlayerCharacterReligiousHitCharacterPlayful,
        PlayerCharacterReligiousHitCharacterLove       = BattleSoundFX.PlayerCharacterReligiousHitCharacterLove,
        PlayerCharacterReligiousHitCharacterSadness    = BattleSoundFX.PlayerCharacterReligiousHitCharacterSadness,
        PlayerCharacterReligiousHitShield              = BattleSoundFX.PlayerCharacterReligiousHitShield,
        PlayerCharacterReligiousDeath                  = BattleSoundFX.PlayerCharacterSeducerDeath,

        // Yesman
        PlayerCharacterYesmanCatchphrase            = BattleSoundFX.PlayerCharacterYesmanCatchphrase,
        PlayerCharacterYesmanHitCharacterAggression = BattleSoundFX.PlayerCharacterYesmanHitCharacterAggression,
        PlayerCharacterYesmanHitCharacterJoy        = BattleSoundFX.PlayerCharacterYesmanHitCharacterJoy,
        PlayerCharacterYesmanHitCharacterLove       = BattleSoundFX.PlayerCharacterYesmanHitCharacterLove,
        PlayerCharacterYesmanHitCharacterPlayful    = BattleSoundFX.PlayerCharacterYesmanHitCharacterPlayful,
        PlayerCharacterYesmanHitCharacterSadness    = BattleSoundFX.PlayerCharacterYesmanHitCharacterSadness,
        PlayerCharacterYesmanHitShield              = BattleSoundFX.PlayerCharacterYesmanHitShield,
        PlayerCharacterYesmanDeath                  = BattleSoundFX.PlayerCharacterYesmanDeath,

        // SlaveOfTheLaw
        PlayerCharacterSlaveOfTheLawCatchphrase            = BattleSoundFX.PlayerCharacterSlaveOfTheLawCatchphrase,
        PlayerCharacterSlaveOfTheLawHitCharacterAggression = BattleSoundFX.PlayerCharacterSlaveOfTheLawHitCharacterAggression,
        PlayerCharacterSlaveOfTheLawHitCharacterJoy        = BattleSoundFX.PlayerCharacterSlaveOfTheLawHitCharacterJoy,
        PlayerCharacterSlaveOfTheLawHitCharacterLove       = BattleSoundFX.PlayerCharacterSlaveOfTheLawHitCharacterLove,
        PlayerCharacterSlaveOfTheLawHitCharacterPlayful    = BattleSoundFX.PlayerCharacterSlaveOfTheLawHitCharacterPlayful,
        PlayerCharacterSlaveOfTheLawHitCharacterSadness    = BattleSoundFX.PlayerCharacterSlaveOfTheLawHitCharacterSadness,
        PlayerCharacterSlaveOfTheLawHitShield              = BattleSoundFX.PlayerCharacterSlaveOfTheLawHitShield,
        PlayerCharacterSlaveOfTheLawDeath                  = BattleSoundFX.PlayerCharacterSlaveOfTheLawDeath,

        // FashionSlave
        PlayerCharacterFashionSlaveCatchphrase            = BattleSoundFX.PlayerCharacterFashionSlaveCatchphrase,
        PlayerCharacterFashionSlaveHitCharacterAggression = BattleSoundFX.PlayerCharacterFashionSlaveHitCharacterAggression,
        PlayerCharacterFashionSlaveHitCharacterJoy        = BattleSoundFX.PlayerCharacterFashionSlaveHitCharacterJoy,
        PlayerCharacterFashionSlaveHitCharacterLove       = BattleSoundFX.PlayerCharacterFashionSlaveHitCharacterLove,
        PlayerCharacterFashionSlaveHitCharacterPlayful    = BattleSoundFX.PlayerCharacterFashionSlaveHitCharacterPlayful,
        PlayerCharacterFashionSlaveHitCharacterSadness    = BattleSoundFX.PlayerCharacterFashionSlaveHitCharacterSadness,
        PlayerCharacterFashionSlaveHitShield              = BattleSoundFX.PlayerCharacterFashionSlaveHitShield,
        PlayerCharacterFashionSlaveDeath                  = BattleSoundFX.PlayerCharacterFashionSlaveDeath,

        // MammasBoy
        PlayerCharacterMammasBoyCatchphrase            = BattleSoundFX.PlayerCharacterMammasBoyCatchphrase,
        PlayerCharacterMammasBoyHitCharacterAggression = BattleSoundFX.PlayerCharacterMammasBoyHitCharacterAggression,
        PlayerCharacterMammasBoyHitCharacterJoy        = BattleSoundFX.PlayerCharacterMammasBoyHitCharacterJoy,
        PlayerCharacterMammasBoyHitCharacterLove       = BattleSoundFX.PlayerCharacterMammasBoyHitCharacterLove,
        PlayerCharacterMammasBoyHitCharacterPlayful    = BattleSoundFX.PlayerCharacterMammasBoyHitCharacterPlayful,
        PlayerCharacterMammasBoyHitCharacterSadness    = BattleSoundFX.PlayerCharacterMammasBoyHitCharacterSadness,
        PlayerCharacterMammasBoyHitShield              = BattleSoundFX.PlayerCharacterMammasBoyHitShield,
        PlayerCharacterMammasBoyDeath                  = BattleSoundFX.PlayerCharacterMammasBoyDeath,

        // Superstitious
        PlayerCharacterSuperstitiousCatchphrase            = BattleSoundFX.PlayerCharacterSuperstitiousCatchphrase,
        PlayerCharacterSuperstitiousHitCharacterAggression = BattleSoundFX.PlayerCharacterSuperstitiousHitCharacterAggression,
        PlayerCharacterSuperstitiousHitCharacterJoy        = BattleSoundFX.PlayerCharacterSuperstitiousHitCharacterJoy,
        PlayerCharacterSuperstitiousHitCharacterLove       = BattleSoundFX.PlayerCharacterSuperstitiousHitCharacterLove,
        PlayerCharacterSuperstitiousHitCharacterPlayful    = BattleSoundFX.PlayerCharacterSuperstitiousHitCharacterPlayful,
        PlayerCharacterSuperstitiousHitCharacterSadness    = BattleSoundFX.PlayerCharacterSuperstitiousHitCharacterSadness,
        PlayerCharacterSuperstitiousHitShield              = BattleSoundFX.PlayerCharacterSuperstitiousHitShield,
        PlayerCharacterSuperstitiousDeath                  = BattleSoundFX.PlayerCharacterSuperstitiousDeath,

        // Artist
        PlayerCharacterArtistCatchphrase            = BattleSoundFX.PlayerCharacterArtistCatchphrase,
        PlayerCharacterArtistHitCharacterAggression = BattleSoundFX.PlayerCharacterArtistHitCharacterAggression,
        PlayerCharacterArtistHitCharacterJoy        = BattleSoundFX.PlayerCharacterArtistHitCharacterJoy,
        PlayerCharacterArtistHitCharacterLove       = BattleSoundFX.PlayerCharacterArtistHitCharacterLove,
        PlayerCharacterArtistHitCharacterPlayful    = BattleSoundFX.PlayerCharacterArtistHitCharacterPlayful,
        PlayerCharacterArtistHitCharacterSadness    = BattleSoundFX.PlayerCharacterArtistHitCharacterSadness,
        PlayerCharacterArtistHitShield              = BattleSoundFX.PlayerCharacterArtistHitShield,
        PlayerCharacterArtistDeath                  = BattleSoundFX.PlayerCharacterArtistDeath,

        // Arguer
        PlayerCharacterArguerCatchphrase            = BattleSoundFX.PlayerCharacterArguerCatchphrase,
        PlayerCharacterArguerHitCharacterAggression = BattleSoundFX.PlayerCharacterArguerHitCharacterAggression,
        PlayerCharacterArguerHitCharacterJoy        = BattleSoundFX.PlayerCharacterArguerHitCharacterJoy,
        PlayerCharacterArguerHitCharacterLove       = BattleSoundFX.PlayerCharacterArguerHitCharacterLove,
        PlayerCharacterArguerHitCharacterPlayful    = BattleSoundFX.PlayerCharacterArguerHitCharacterPlayful,
        PlayerCharacterArguerHitCharacterSadness    = BattleSoundFX.PlayerCharacterArguerHitCharacterSadness,
        PlayerCharacterArguerHitShield              = BattleSoundFX.PlayerCharacterArguerHitShield,
        PlayerCharacterArguerDeath                  = BattleSoundFX.PlayerCharacterArguerDeath,

        // Reflector
        PlayerCharacterReflectorCatchphrase            = BattleSoundFX.PlayerCharacterReflectorCatchphrase,
        PlayerCharacterReflectorHitCharacterAggression = BattleSoundFX.PlayerCharacterReflectorHitCharacterAggression,
        PlayerCharacterReflectorHitCharacterJoy        = BattleSoundFX.PlayerCharacterReflectorHitCharacterJoy,
        PlayerCharacterReflectorHitCharacterLove       = BattleSoundFX.PlayerCharacterReflectorHitCharacterLove,
        PlayerCharacterReflectorHitCharacterPlayful    = BattleSoundFX.PlayerCharacterReflectorHitCharacterPlayful,
        PlayerCharacterReflectorHitCharacterSadness    = BattleSoundFX.PlayerCharacterReflectorHitCharacterSadness,
        PlayerCharacterReflectorHitShield              = BattleSoundFX.PlayerCharacterReflectorHitShield,
        PlayerCharacterReflectorDeath                  = BattleSoundFX.PlayerCharacterReflectorDeath,

        // Delusional
        PlayerCharacterDelusionalCatchphrase            = BattleSoundFX.PlayerCharacterDelusionalCatchphrase,
        PlayerCharacterDelusionalHitCharacterAggression = BattleSoundFX.PlayerCharacterDelusionalHitCharacterAggression,
        PlayerCharacterDelusionalHitCharacterJoy        = BattleSoundFX.PlayerCharacterDelusionalHitCharacterJoy,
        PlayerCharacterDelusionalHitCharacterLove       = BattleSoundFX.PlayerCharacterDelusionalHitCharacterLove,
        PlayerCharacterDelusionalHitCharacterPlayful    = BattleSoundFX.PlayerCharacterDelusionalHitCharacterPlayful,
        PlayerCharacterDelusionalHitCharacterSadness    = BattleSoundFX.PlayerCharacterDelusionalHitCharacterSadness,
        PlayerCharacterDelusionalHitShield              = BattleSoundFX.PlayerCharacterDelusionalHitShield,
        PlayerCharacterDelusionalDeath                  = BattleSoundFX.PlayerCharacterDelusionalDeath,

        // Overeater
        PlayerCharacterOvereaterCatchphrase            = BattleSoundFX.PlayerCharacterOvereaterCatchphrase,
        PlayerCharacterOvereaterHitCharacterAggression = BattleSoundFX.PlayerCharacterOvereaterHitCharacterAggression,
        PlayerCharacterOvereaterHitCharacterJoy        = BattleSoundFX.PlayerCharacterOvereaterHitCharacterJoy,
        PlayerCharacterOvereaterHitCharacterLove       = BattleSoundFX.PlayerCharacterOvereaterHitCharacterLove,
        PlayerCharacterOvereaterHitCharacterPlayful    = BattleSoundFX.PlayerCharacterOvereaterHitCharacterPlayful,
        PlayerCharacterOvereaterHitCharacterSadness    = BattleSoundFX.PlayerCharacterOvereaterHitCharacterSadness,
        PlayerCharacterOvereaterHitShield              = BattleSoundFX.PlayerCharacterOvereaterHitShield,
        PlayerCharacterOvereaterDeath                  = BattleSoundFX.PlayerCharacterOvereaterDeath,

        // Alcoholic
        PlayerCharacterAlcoholicCatchphrase            = BattleSoundFX.PlayerCharacterAlcoholicCatchphrase,
        PlayerCharacterAlcoholicHitCharacterAggression = BattleSoundFX.PlayerCharacterAlcoholicHitCharacterAggression,
        PlayerCharacterAlcoholicHitCharacterJoy        = BattleSoundFX.PlayerCharacterAlcoholicHitCharacterJoy,
        PlayerCharacterAlcoholicHitCharacterLove       = BattleSoundFX.PlayerCharacterAlcoholicHitCharacterLove,
        PlayerCharacterAlcoholicHitCharacterPlayful    = BattleSoundFX.PlayerCharacterAlcoholicHitCharacterPlayful,
        PlayerCharacterAlcoholicHitCharacterSadness    = BattleSoundFX.PlayerCharacterAlcoholicHitCharacterSadness,
        PlayerCharacterAlcoholicHitShield              = BattleSoundFX.PlayerCharacterAlcoholicHitShield,
        PlayerCharacterAlcoholicDeath                  = BattleSoundFX.PlayerCharacterAlcoholicDeath,

        // Anorectic
        PlayerCharacterAnorecticCatchphrase            = BattleSoundFX.PlayerCharacterAnorecticCatchphrase,
        PlayerCharacterAnorecticHitCharacterAggression = BattleSoundFX.PlayerCharacterAnorecticHitCharacterAggression,
        PlayerCharacterAnorecticHitCharacterJoy        = BattleSoundFX.PlayerCharacterAnorecticHitCharacterJoy,
        PlayerCharacterAnorecticHitCharacterLove       = BattleSoundFX.PlayerCharacterAnorecticHitCharacterLove,
        PlayerCharacterAnorecticHitCharacterPlayful    = BattleSoundFX.PlayerCharacterAnorecticHitCharacterPlayful,
        PlayerCharacterAnorecticHitCharacterSadness    = BattleSoundFX.PlayerCharacterAnorecticHitCharacterSadness,
        PlayerCharacterAnorecticHitShield              = BattleSoundFX.PlayerCharacterAnorecticHitShield,
        PlayerCharacterAnorecticDeath                  = BattleSoundFX.PlayerCharacterAnorecticDeath,

        // Stoner
        PlayerCharacterStonerCatchphrase            = BattleSoundFX.PlayerCharacterStonerCatchphrase,
        PlayerCharacterStonerHitCharacterAggression = BattleSoundFX.PlayerCharacterStonerHitCharacterAggression,
        PlayerCharacterStonerHitCharacterJoy        = BattleSoundFX.PlayerCharacterStonerHitCharacterJoy,
        PlayerCharacterStonerHitCharacterLove       = BattleSoundFX.PlayerCharacterStonerHitCharacterLove,
        PlayerCharacterStonerHitCharacterPlayful    = BattleSoundFX.PlayerCharacterStonerHitCharacterPlayful,
        PlayerCharacterStonerHitCharacterSadness    = BattleSoundFX.PlayerCharacterStonerHitCharacterSadness,
        PlayerCharacterStonerHitShield              = BattleSoundFX.PlayerCharacterStonerHitShield,
        PlayerCharacterStonerDeath                  = BattleSoundFX.PlayerCharacterStonerDeath,

        // Martyr
        PlayerCharacterMartyrCatchphrase            = BattleSoundFX.PlayerCharacterMartyrCatchphrase,
        PlayerCharacterMartyrHitCharacterAggression = BattleSoundFX.PlayerCharacterMartyrHitCharacterAggression,
        PlayerCharacterMartyrHitCharacterJoy        = BattleSoundFX.PlayerCharacterMartyrHitCharacterJoy,
        PlayerCharacterMartyrHitCharacterLove       = BattleSoundFX.PlayerCharacterMartyrHitCharacterLove,
        PlayerCharacterMartyrHitCharacterPlayful    = BattleSoundFX.PlayerCharacterMartyrHitCharacterPlayful,
        PlayerCharacterMartyrHitCharacterSadness    = BattleSoundFX.PlayerCharacterMartyrHitCharacterSadness,
        PlayerCharacterMartyrHitShield              = BattleSoundFX.PlayerCharacterMartyrHitShield,
        PlayerCharacterMartyrDeath                  = BattleSoundFX.PlayerCharacterMartyrDeath,

        // Suicidal
        PlayerCharacterSuicidalCatchphrase            = BattleSoundFX.PlayerCharacterSuicidalCatchphrase,
        PlayerCharacterSuicidalHitCharacterAggression = BattleSoundFX.PlayerCharacterSuicidalHitCharacterAggression,
        PlayerCharacterSuicidalHitCharacterJoy        = BattleSoundFX.PlayerCharacterSuicidalHitCharacterJoy,
        PlayerCharacterSuicidalHitCharacterLove       = BattleSoundFX.PlayerCharacterSuicidalHitCharacterLove,
        PlayerCharacterSuicidalHitCharacterPlayful    = BattleSoundFX.PlayerCharacterSuicidalHitCharacterPlayful,
        PlayerCharacterSuicidalHitCharacterSadness    = BattleSoundFX.PlayerCharacterSuicidalHitCharacterSadness,
        PlayerCharacterSuicidalHitShield              = BattleSoundFX.PlayerCharacterSuicidalHitShield,
        PlayerCharacterSuicidalDeath                  = BattleSoundFX.PlayerCharacterSuicidalDeath,

        // Soulsisters
        PlayerCharacterSoulsistersCatchphrase            = BattleSoundFX.PlayerCharacterSoulsistersCatchphrase,
        PlayerCharacterSoulsistersHitCharacterAggression = BattleSoundFX.PlayerCharacterSoulsistersHitCharacterAggression,
        PlayerCharacterSoulsistersHitCharacterJoy        = BattleSoundFX.PlayerCharacterSoulsistersHitCharacterJoy,
        PlayerCharacterSoulsistersHitCharacterLove       = BattleSoundFX.PlayerCharacterSoulsistersHitCharacterLove,
        PlayerCharacterSoulsistersHitCharacterPlayful    = BattleSoundFX.PlayerCharacterSoulsistersHitCharacterPlayful,
        PlayerCharacterSoulsistersHitCharacterSadness    = BattleSoundFX.PlayerCharacterSoulsistersHitCharacterSadness,
        PlayerCharacterSoulsistersHitShield              = BattleSoundFX.PlayerCharacterSoulsistersHitShield,
        PlayerCharacterSoulsistersDeath                  = BattleSoundFX.PlayerCharacterSoulsistersDeath,

        // Lovers
        PlayerCharacterLoversCatchphrase            = BattleSoundFX.PlayerCharacterLoversCatchphrase,
        PlayerCharacterLoversHitCharacterAggression = BattleSoundFX.PlayerCharacterLoversHitCharacterAggression,
        PlayerCharacterLoversHitCharacterJoy        = BattleSoundFX.PlayerCharacterLoversHitCharacterJoy,
        PlayerCharacterLoversHitCharacterPlayful    = BattleSoundFX.PlayerCharacterLoversHitCharacterPlayful,
        PlayerCharacterLoversHitCharacterLove       = BattleSoundFX.PlayerCharacterLoversHitCharacterLove,
        PlayerCharacterLoversHitCharacterSadness    = BattleSoundFX.PlayerCharacterLoversHitCharacterSadness,
        PlayerCharacterLoversHitShield              = BattleSoundFX.PlayerCharacterLoversHitShield,
        PlayerCharacterLoversDeath                  = BattleSoundFX.PlayerCharacterLoversDeath,

        // SleepyHead
        PlayerCharacterSleepyHeadCatchphrase            = BattleSoundFX.PlayerCharacterSleepyHeadCatchphrase,
        PlayerCharacterSleepyHeadHitCharacterAggression = BattleSoundFX.PlayerCharacterSleepyHeadHitCharacterAggression,
        PlayerCharacterSleepyHeadHitCharacterJoy        = BattleSoundFX.PlayerCharacterSleepyHeadHitCharacterJoy,
        PlayerCharacterSleepyHeadHitCharacterLove       = BattleSoundFX.PlayerCharacterSleepyHeadHitCharacterLove,
        PlayerCharacterSleepyHeadHitCharacterPlayful    = BattleSoundFX.PlayerCharacterSleepyHeadHitCharacterPlayful,
        PlayerCharacterSleepyHeadHitCharacterSadness    = BattleSoundFX.PlayerCharacterSleepyHeadHitCharacterSadness,
        PlayerCharacterSleepyHeadHitShield              = BattleSoundFX.PlayerCharacterSleepyHeadHitShield,
        PlayerCharacterSleepyHeadDeath                  = BattleSoundFX.PlayerCharacterSleepyHeadDeath,

        // Tribalist
        PlayerCharacterTribalistCatchphrase            = BattleSoundFX.PlayerCharacterTribalistCatchphrase,
        PlayerCharacterTribalistHitCharacterAggression = BattleSoundFX.PlayerCharacterTribalistHitCharacterAggression,
        PlayerCharacterTribalistHitCharacterJoy        = BattleSoundFX.PlayerCharacterTribalistHitCharacterJoy,
        PlayerCharacterTribalistHitCharacterLove       = BattleSoundFX.PlayerCharacterTribalistHitCharacterLove,
        PlayerCharacterTribalistHitCharacterPlayful    = BattleSoundFX.PlayerCharacterTribalistHitCharacterPlayful,
        PlayerCharacterTribalistHitCharacterSadness    = BattleSoundFX.PlayerCharacterTribalistHitCharacterSadness,
        PlayerCharacterTribalistHitShield              = BattleSoundFX.PlayerCharacterTribalistHitShield,
        PlayerCharacterTribalistDeath                  = BattleSoundFX.PlayerCharacterTribalistDeath,

        // GangBanger
        PlayerCharacterGangBangerCatchphrase            = BattleSoundFX.PlayerCharacterGangBangerCatchphrase,
        PlayerCharacterGangBangerHitCharacterAggression = BattleSoundFX.PlayerCharacterGangBangerHitCharacterAggression,
        PlayerCharacterGangBangerHitCharacterJoy        = BattleSoundFX.PlayerCharacterGangBangerHitCharacterJoy,
        PlayerCharacterGangBangerHitCharacterLove       = BattleSoundFX.PlayerCharacterGangBangerHitCharacterLove,
        PlayerCharacterGangBangerHitCharacterPlayful    = BattleSoundFX.PlayerCharacterGangBangerHitCharacterPlayful,
        PlayerCharacterGangBangerHitCharacterSadness    = BattleSoundFX.PlayerCharacterGangBangerHitCharacterSadness,
        PlayerCharacterGangBangerHitShield              = BattleSoundFX.PlayerCharacterGangBangerHitShield,
        PlayerCharacterGangBangerDeath                  = BattleSoundFX.PlayerCharacterGangBangerDeath,

        // Booksmart
        PlayerCharacterBooksmartCatchphrase            = BattleSoundFX.PlayerCharacterBooksmartCatchphrase,
        PlayerCharacterBooksmartHitCharacterAggression = BattleSoundFX.PlayerCharacterBooksmartHitCharacterAggression,
        PlayerCharacterBooksmartHitCharacterJoy        = BattleSoundFX.PlayerCharacterBooksmartHitCharacterJoy,
        PlayerCharacterBooksmartHitCharacterLove       = BattleSoundFX.PlayerCharacterBooksmartHitCharacterLove,
        PlayerCharacterBooksmartHitCharacterPlayful    = BattleSoundFX.PlayerCharacterBooksmartHitCharacterPlayful,
        PlayerCharacterBooksmartHitCharacterSadness    = BattleSoundFX.PlayerCharacterBooksmartHitCharacterSadness,
        PlayerCharacterBooksmartHitShield              = BattleSoundFX.PlayerCharacterBooksmartHitShield,
        PlayerCharacterBooksmartDeath                  = BattleSoundFX.PlayerCharacterBooksmartDeath,

        // Capitalist
        PlayerCharacterCapitalistCatchphrase            = BattleSoundFX.PlayerCharacterCapitalistCatchphrase,
        PlayerCharacterCapitalistHitCharacterAggression = BattleSoundFX.PlayerCharacterCapitalistHitCharacterAggression,
        PlayerCharacterCapitalistHitCharacterJoy        = BattleSoundFX.PlayerCharacterCapitalistHitCharacterJoy,
        PlayerCharacterCapitalistHitCharacterLove       = BattleSoundFX.PlayerCharacterCapitalistHitCharacterLove,
        PlayerCharacterCapitalistHitCharacterPlayful    = BattleSoundFX.PlayerCharacterCapitalistHitCharacterPlayful,
        PlayerCharacterCapitalistHitCharacterSadness    = BattleSoundFX.PlayerCharacterCapitalistHitCharacterSadness,
        PlayerCharacterCapitalistHitShield              = BattleSoundFX.PlayerCharacterCapitalistHitShield,
        PlayerCharacterCapitalistDeath                  = BattleSoundFX.PlayerCharacterCapitalistDeath,

        // ObsessiveCompulsive
        PlayerCharacterObsessiveCompulsiveCatchphrase            = BattleSoundFX.PlayerCharacterObsessiveCompulsiveCatchphrase,
        PlayerCharacterObsessiveCompulsiveHitCharacterAggression = BattleSoundFX.PlayerCharacterObsessiveCompulsiveHitCharacterAggression,
        PlayerCharacterObsessiveCompulsiveHitCharacterJoy        = BattleSoundFX.PlayerCharacterObsessiveCompulsiveHitCharacterJoy,
        PlayerCharacterObsessiveCompulsiveHitCharacterLove       = BattleSoundFX.PlayerCharacterObsessiveCompulsiveHitCharacterLove,
        PlayerCharacterObsessiveCompulsiveHitCharacterPlayful    = BattleSoundFX.PlayerCharacterObsessiveCompulsiveHitCharacterPlayful,
        PlayerCharacterObsessiveCompulsiveHitCharacterSadness    = BattleSoundFX.PlayerCharacterObsessiveCompulsiveHitCharacterSadness,
        PlayerCharacterObsessiveCompulsiveHitShield              = BattleSoundFX.PlayerCharacterObsessiveCompulsiveHitShield,
        PlayerCharacterObsessiveCompulsiveDeath                  = BattleSoundFX.PlayerCharacterObsessiveCompulsiveDeath,

        // Overcompilator
        PlayerCharacterOvercompilatorCatchphrase            = BattleSoundFX.PlayerCharacterOvercompilatorCatchphrase,
        PlayerCharacterOvercompilatorHitCharacterAggression = BattleSoundFX.PlayerCharacterOvercompilatorHitCharacterAggression,
        PlayerCharacterOvercompilatorHitCharacterJoy        = BattleSoundFX.PlayerCharacterOvercompilatorHitCharacterJoy,
        PlayerCharacterOvercompilatorHitCharacterLove       = BattleSoundFX.PlayerCharacterOvercompilatorHitCharacterLove,
        PlayerCharacterOvercompilatorHitCharacterPlayful    = BattleSoundFX.PlayerCharacterOvercompilatorHitCharacterPlayful,
        PlayerCharacterOvercompilatorHitCharacterSadness    = BattleSoundFX.PlayerCharacterOvercompilatorHitCharacterSadness,
        PlayerCharacterOvercompilatorHitShield              = BattleSoundFX.PlayerCharacterOvercompilatorHitShield,
        PlayerCharacterOvercompilatorDeath                  = BattleSoundFX.PlayerCharacterOvercompilatorDeath,

        // NitPicker
        PlayerCharacterNitPickerCatchphrase            = BattleSoundFX.PlayerCharacterNitPickerCatchphrase,
        PlayerCharacterNitPickerHitCharacterAggression = BattleSoundFX.PlayerCharacterNitPickerHitCharacterAggression,
        PlayerCharacterNitPickerHitCharacterJoy        = BattleSoundFX.PlayerCharacterNitPickerHitCharacterJoy,
        PlayerCharacterNitPickerHitCharacterLove       = BattleSoundFX.PlayerCharacterNitPickerHitCharacterLove,
        PlayerCharacterNitPickerHitCharacterPlayful    = BattleSoundFX.PlayerCharacterNitPickerHitCharacterPlayful,
        PlayerCharacterNitPickerHitCharacterSadness    = BattleSoundFX.PlayerCharacterNitPickerHitCharacterSadness,
        PlayerCharacterNitPickerHitShield              = BattleSoundFX.PlayerCharacterNitPickerHitShield,
        PlayerCharacterNitPickerDeath                  = BattleSoundFX.PlayerCharacterNitPickerDeath

        #endregion  Character Sound Effects
    }
}
