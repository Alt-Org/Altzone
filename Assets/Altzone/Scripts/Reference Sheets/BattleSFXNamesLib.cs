using Quantum;

namespace Assets.Altzone.Scripts.Reference_Sheets
{
    public enum BattleSFXNameTypes
    {
        None = -1,
        SoulWallHit = BattleSoundFX.SoulWallHit,
        GoalHit = BattleSoundFX.GoalHit,
        SideWallHit = BattleSoundFX.SideWallHit,
        SoulWallBroken = BattleSoundFX.SoulWallBroken,
        DiamondPickUp = BattleSoundFX.DiamondPickUp,
        SoulWallHitAggression = BattleSoundFX.SoulWallHitAggression,
        SoulWallHitJoy = BattleSoundFX.SoulWallHitJoy,
        SoulWallHitLove = BattleSoundFX.SoulWallHitLove,
        SoulWallHitPlayful = BattleSoundFX.SoulWallHitPlayful,
        SoulWallHitSadness = BattleSoundFX.SoulWallHitSadness,

        #region Character Sound Effects

        // Racist
        PlayerCharacterRacistCatchphrase = BattleSoundFX.PlayerCharacterRacistCatchphrase,
        PlayerCharacterRacistHitCharacter = BattleSoundFX.PlayerCharacterRacistHitCharacter,
        PlayerCharacterRacistHitShield    = BattleSoundFX.PlayerCharacterRacistHitShield,
        PlayerCharacterRacistDeath        = BattleSoundFX.PlayerCharacterRacistDeath,

        // BodyBuilder
        PlayerCharacterBodybuilderCatchphrase  = BattleSoundFX.PlayerCharacterBodybuilderCatchphrase,
        PlayerCharacterBodybuilderHitCharacter = BattleSoundFX.PlayerCharacterBodybuilderHitCharacter,
        PlayerCharacterBodybuilderHitShield    = BattleSoundFX.PlayerCharacterBodybuilderHitShield,
        PlayerCharacterBodybuilderDeath        = BattleSoundFX.PlayerCharacterBodybuilderDeath,

        // WarVeteran
        PlayerCharacterWarVeteranCatchphrase  = BattleSoundFX.PlayerCharacterWarVeteranCatchphrase,
        PlayerCharacterWarVeteranHitCharacter = BattleSoundFX.PlayerCharacterWarVeteranHitCharacter,
        PlayerCharacterWarVeteranHitShield    = BattleSoundFX.PlayerCharacterWarVeteranHitShield,
        PlayerCharacterWarVeteranDeath        = BattleSoundFX.PlayerCharacterWarVeteranDeath,

        // Bully
        PlayerCharacterBullyCatchphrase  = BattleSoundFX.PlayerCharacterBullyCatchphrase,
        PlayerCharacterBullyHitCharacter = BattleSoundFX.PlayerCharacterBullyHitCharacter,
        PlayerCharacterBullyHitShield    = BattleSoundFX.PlayerCharacterBullyHitShield,
        PlayerCharacterBullyDeath        = BattleSoundFX.PlayerCharacterBullyDeath,

        // Egoist
        PlayerCharacterEgoistCatchphrase  = BattleSoundFX.PlayerCharacterEgoistCatchphrase,
        PlayerCharacterEgoistHitCharacter = BattleSoundFX.PlayerCharacterEgoistHitCharacter,
        PlayerCharacterEgoistHitShield    = BattleSoundFX.PlayerCharacterEgoistHitShield,
        PlayerCharacterEgoistDeath        = BattleSoundFX.PlayerCharacterEgoistDeath,

        // Depressed
        PlayerCharacterDepressedCatchphrase  = BattleSoundFX.PlayerCharacterDepressedCatchphrase,
        PlayerCharacterDepressedHitCharacter = BattleSoundFX.PlayerCharacterDepressedHitCharacter,
        PlayerCharacterDepressedHitShield    = BattleSoundFX.PlayerCharacterDepressedHitShield,
        PlayerCharacterDepressedDeath        = BattleSoundFX.PlayerCharacterDepressedDeath,

        // Comedian
        PlayerCharacterComedianCatchphrase  = BattleSoundFX.PlayerCharacterComedianCatchphrase,
        PlayerCharacterComedianHitCharacter = BattleSoundFX.PlayerCharacterComedianHitCharacter,
        PlayerCharacterComedianHitShield    = BattleSoundFX.PlayerCharacterComedianHitShield,
        PlayerCharacterComedianDeath        = BattleSoundFX.PlayerCharacterComedianDeath,

        // Joker
        PlayerCharacterJokerCatchphrase  = BattleSoundFX.PlayerCharacterJokerCatchphrase,
        PlayerCharacterJokerHitCharacter = BattleSoundFX.PlayerCharacterJokerHitCharacter,
        PlayerCharacterJokerHitShield    = BattleSoundFX.PlayerCharacterJokerHitShield,
        PlayerCharacterJokerDeath        = BattleSoundFX.PlayerCharacterJokerDeath,

        // Conman
        PlayerCharacterConmanCatchphrase  = BattleSoundFX.PlayerCharacterConmanCatchphrase,
        PlayerCharacterConmanHitCharacter = BattleSoundFX.PlayerCharacterConmanHitCharacter,
        PlayerCharacterConmanHitShield    = BattleSoundFX.PlayerCharacterConmanHitShield,
        PlayerCharacterConmanDeath        = BattleSoundFX.PlayerCharacterConmanDeath,

        // Seducer
        PlayerCharacterSeducerCatchphrase  = BattleSoundFX.PlayerCharacterSeducerCatchphrase,
        PlayerCharacterSeducerHitCharacter = BattleSoundFX.PlayerCharacterSeducerHitCharacter,
        PlayerCharacterSeducerHitShield    = BattleSoundFX.PlayerCharacterSeducerHitShield,
        PlayerCharacterSeducerDeath        = BattleSoundFX.PlayerCharacterSeducerDeath,

        // Religious
        PlayerCharacterReligiousCatchphrase  = BattleSoundFX.PlayerCharacterReligiousCatchphrase,
        PlayerCharacterReligiousHitCharacter = BattleSoundFX.PlayerCharacterReligiousHitCharacter,
        PlayerCharacterReligiousHitShield    = BattleSoundFX.PlayerCharacterReligiousHitShield,
        PlayerCharacterReligiousDeath        = BattleSoundFX.PlayerCharacterSeducerDeath,

        // Yesman
        PlayerCharacterYesmanCatchphrase  = BattleSoundFX.PlayerCharacterYesmanCatchphrase,
        PlayerCharacterYesmanHitCharacter = BattleSoundFX.PlayerCharacterYesmanHitCharacter,
        PlayerCharacterYesmanHitShield    = BattleSoundFX.PlayerCharacterYesmanHitShield,
        PlayerCharacterYesmanDeath        = BattleSoundFX.PlayerCharacterYesmanDeath,

        // SlaveOfTheLaw
        PlayerCharacterSlaveOfTheLawCatchphrase  = BattleSoundFX.PlayerCharacterSlaveOfTheLawCatchphrase,
        PlayerCharacterSlaveOfTheLawHitCharacter = BattleSoundFX.PlayerCharacterSlaveOfTheLawHitCharacter,
        PlayerCharacterSlaveOfTheLawHitShield    = BattleSoundFX.PlayerCharacterSlaveOfTheLawHitShield,
        PlayerCharacterSlaveOfTheLawDeath        = BattleSoundFX.PlayerCharacterSlaveOfTheLawDeath,

        // FashionSlave
        PlayerCharacterFashionSlaveCatchphrase  = BattleSoundFX.PlayerCharacterFashionSlaveCatchphrase,
        PlayerCharacterFashionSlaveHitCharacter = BattleSoundFX.PlayerCharacterFashionSlaveHitCharacter,
        PlayerCharacterFashionSlaveHitShield    = BattleSoundFX.PlayerCharacterFashionSlaveHitShield,
        PlayerCharacterFashionSlaveDeath        = BattleSoundFX.PlayerCharacterFashionSlaveDeath,

        // MammasBoy
        PlayerCharacterMammasBoyCatchphrase  = BattleSoundFX.PlayerCharacterMammasBoyCatchphrase,
        PlayerCharacterMammasBoyHitCharacter = BattleSoundFX.PlayerCharacterMammasBoyHitCharacter,
        PlayerCharacterMammasBoyHitShield    = BattleSoundFX.PlayerCharacterMammasBoyHitShield,
        PlayerCharacterMammasBoyDeath        = BattleSoundFX.PlayerCharacterMammasBoyDeath,

        // Superstitious
        PlayerCharacterSuperstitiousCatchphrase  = BattleSoundFX.PlayerCharacterSuperstitiousCatchphrase,
        PlayerCharacterSuperstitiousHitCharacter = BattleSoundFX.PlayerCharacterSuperstitiousHitCharacter,
        PlayerCharacterSuperstitiousHitShield    = BattleSoundFX.PlayerCharacterSuperstitiousHitShield,
        PlayerCharacterSuperstitiousDeath        = BattleSoundFX.PlayerCharacterSuperstitiousDeath,

        // Artist
        PlayerCharacterArtistCatchphrase  = BattleSoundFX.PlayerCharacterArtistCatchphrase,
        PlayerCharacterArtistHitCharacter = BattleSoundFX.PlayerCharacterArtistHitCharacter,
        PlayerCharacterArtistHitShield    = BattleSoundFX.PlayerCharacterArtistHitShield,
        PlayerCharacterArtistDeath        = BattleSoundFX.PlayerCharacterArtistDeath,

        // Arguer
        PlayerCharacterArguerCatchphrase  = BattleSoundFX.PlayerCharacterArguerCatchphrase,
        PlayerCharacterArguerHitCharacter = BattleSoundFX.PlayerCharacterArguerHitCharacter,
        PlayerCharacterArguerHitShield    = BattleSoundFX.PlayerCharacterArguerDeath,
        PlayerCharacterArguerDeath,

        // Reflector
        PlayerCharacterReflectorCatchphrase  = BattleSoundFX.PlayerCharacterReflectorCatchphrase,
        PlayerCharacterReflectorHitCharacter = BattleSoundFX.PlayerCharacterReflectorHitCharacter,
        PlayerCharacterReflectorHitShield    = BattleSoundFX.PlayerCharacterReflectorHitShield,
        PlayerCharacterReflectorDeath        = BattleSoundFX.PlayerCharacterReflectorDeath,

        // Delusional
        PlayerCharacterDelusionalCatchphrase  = BattleSoundFX.PlayerCharacterDelusionalCatchphrase,
        PlayerCharacterDelusionalHitCharacter = BattleSoundFX.PlayerCharacterDelusionalHitCharacter,
        PlayerCharacterDelusionalHitShield    = BattleSoundFX.PlayerCharacterDelusionalHitShield,
        PlayerCharacterDelusionalDeath        = BattleSoundFX.PlayerCharacterDelusionalDeath,

        // Overeater
        PlayerCharacterOvereaterCatchphrase  = BattleSoundFX.PlayerCharacterOvereaterCatchphrase,
        PlayerCharacterOvereaterHitCharacter = BattleSoundFX.PlayerCharacterOvereaterHitCharacter,
        PlayerCharacterOvereaterHitShield    = BattleSoundFX.PlayerCharacterOvereaterHitShield,
        PlayerCharacterOvereaterDeath        = BattleSoundFX.PlayerCharacterOvereaterDeath,

        // Alcoholic
        PlayerCharacterAlcoholicCatchphrase  = BattleSoundFX.PlayerCharacterAlcoholicCatchphrase,
        PlayerCharacterAlcoholicHitCharacter = BattleSoundFX.PlayerCharacterAlcoholicHitCharacter,
        PlayerCharacterAlcoholicHitShield    = BattleSoundFX.PlayerCharacterAlcoholicHitShield,
        PlayerCharacterAlcoholicDeath        = BattleSoundFX.PlayerCharacterAlcoholicDeath,

        // Anorectic
        PlayerCharacterAnorecticCatchphrase  = BattleSoundFX.PlayerCharacterAnorecticCatchphrase,
        PlayerCharacterAnorecticHitCharacter = BattleSoundFX.PlayerCharacterAnorecticHitCharacter,
        PlayerCharacterAnorecticHitShield    = BattleSoundFX.PlayerCharacterAnorecticHitShield,
        PlayerCharacterAnorecticDeath        = BattleSoundFX.PlayerCharacterAnorecticDeath,

        // Stoner
        PlayerCharacterStonerCatchphrase  = BattleSoundFX.PlayerCharacterStonerCatchphrase,
        PlayerCharacterStonerHitCharacter = BattleSoundFX.PlayerCharacterStonerHitCharacter,
        PlayerCharacterStonerHitShield    = BattleSoundFX.PlayerCharacterStonerHitShield,
        PlayerCharacterStonerDeath        = BattleSoundFX.PlayerCharacterStonerDeath,

        // Martyr
        PlayerCharacterMartyrCatchphrase  = BattleSoundFX.PlayerCharacterMartyrCatchphrase,
        PlayerCharacterMartyrHitCharacter = BattleSoundFX.PlayerCharacterMartyrHitCharacter,
        PlayerCharacterMartyrHitShield    = BattleSoundFX.PlayerCharacterMartyrHitShield,
        PlayerCharacterMartyrDeath        = BattleSoundFX.PlayerCharacterMartyrDeath,

        // Suicidal
        PlayerCharacterSuicidalCatchphrase  = BattleSoundFX.PlayerCharacterSuicidalCatchphrase,
        PlayerCharacterSuicidalHitCharacter = BattleSoundFX.PlayerCharacterSuicidalHitCharacter,
        PlayerCharacterSuicidalHitShield    = BattleSoundFX.PlayerCharacterSuicidalHitShield,
        PlayerCharacterSuicidalDeath        = BattleSoundFX.PlayerCharacterSuicidalDeath,

        // Soulsisters
        PlayerCharacterSoulsistersCatchphrase  = BattleSoundFX.PlayerCharacterSoulsistersCatchphrase,
        PlayerCharacterSoulsistersHitCharacter = BattleSoundFX.PlayerCharacterSoulsistersHitCharacter,
        PlayerCharacterSoulsistersHitShield    = BattleSoundFX.PlayerCharacterSoulsistersHitShield,
        PlayerCharacterSoulsistersDeath        = BattleSoundFX.PlayerCharacterSoulsistersDeath,

        // Lovers
        PlayerCharacterLoversCatchphrase  = BattleSoundFX.PlayerCharacterLoversCatchphrase,
        PlayerCharacterLoversHitCharacter = BattleSoundFX.PlayerCharacterLoversHitCharacter,
        PlayerCharacterLoversHitShield    = BattleSoundFX.PlayerCharacterLoversHitShield,
        PlayerCharacterLoversDeath        = BattleSoundFX.PlayerCharacterLoversDeath,

        // SleepyHead
        PlayerCharacterSleepyHeadCatchphrase  = BattleSoundFX.PlayerCharacterSleepyHeadCatchphrase,
        PlayerCharacterSleepyHeadHitCharacter = BattleSoundFX.PlayerCharacterSleepyHeadHitCharacter,
        PlayerCharacterSleepyHeadHitShield    = BattleSoundFX.PlayerCharacterSleepyHeadHitShield,
        PlayerCharacterSleepyHeadDeath        = BattleSoundFX.PlayerCharacterSleepyHeadDeath,

        // Tribalist
        PlayerCharacterTribalistCatchphrase  = BattleSoundFX.PlayerCharacterTribalistCatchphrase,
        PlayerCharacterTribalistHitCharacter = BattleSoundFX.PlayerCharacterTribalistHitCharacter,
        PlayerCharacterTribalistHitShield    = BattleSoundFX.PlayerCharacterTribalistHitShield,
        PlayerCharacterTribalistDeath        = BattleSoundFX.PlayerCharacterTribalistDeath,

        // GangBanger
        PlayerCharacterGangBangerCatchphrase  = BattleSoundFX.PlayerCharacterGangBangerCatchphrase,
        PlayerCharacterGangBangerHitCharacter = BattleSoundFX.PlayerCharacterGangBangerHitCharacter,
        PlayerCharacterGangBangerHitShield    = BattleSoundFX.PlayerCharacterGangBangerHitShield,
        PlayerCharacterGangBangerDeath        = BattleSoundFX.PlayerCharacterGangBangerDeath,

        // Booksmart
        PlayerCharacterBooksmartCatchphrase  = BattleSoundFX.PlayerCharacterBooksmartCatchphrase,
        PlayerCharacterBooksmartHitCharacter = BattleSoundFX.PlayerCharacterBooksmartHitCharacter,
        PlayerCharacterBooksmartHitShield    = BattleSoundFX.PlayerCharacterBooksmartHitShield,
        PlayerCharacterBooksmartDeath        = BattleSoundFX.PlayerCharacterBooksmartDeath,

        // Capitalist
        PlayerCharacterCapitalistCatchphrase  = BattleSoundFX.PlayerCharacterCapitalistCatchphrase,
        PlayerCharacterCapitalistHitCharacter = BattleSoundFX.PlayerCharacterCapitalistHitCharacter,
        PlayerCharacterCapitalistHitShield    = BattleSoundFX.PlayerCharacterCapitalistHitShield,
        PlayerCharacterCapitalistDeath        = BattleSoundFX.PlayerCharacterCapitalistDeath,

        // ObsessiveCompulsive
        PlayerCharacterObsessiveCompulsiveCatchphrase  = BattleSoundFX.PlayerCharacterObsessiveCompulsiveCatchphrase,
        PlayerCharacterObsessiveCompulsiveHitCharacter = BattleSoundFX.PlayerCharacterObsessiveCompulsiveHitCharacter,
        PlayerCharacterObsessiveCompulsiveHitShield    = BattleSoundFX.PlayerCharacterObsessiveCompulsiveHitShield,
        PlayerCharacterObsessiveCompulsiveDeath        = BattleSoundFX.PlayerCharacterObsessiveCompulsiveDeath,

        // Overcompilator
        PlayerCharacterOvercompilatorCatchphrase  = BattleSoundFX.PlayerCharacterOvercompilatorCatchphrase,
        PlayerCharacterOvercompilatorHitCharacter = BattleSoundFX.PlayerCharacterOvercompilatorHitCharacter,
        PlayerCharacterOvercompilatorHitShield    = BattleSoundFX.PlayerCharacterOvercompilatorHitShield,
        PlayerCharacterOvercompilatorDeath        = BattleSoundFX.PlayerCharacterOvercompilatorDeath,

        // NitPicker
        PlayerCharacterNitPickerCatchphrase  = BattleSoundFX.PlayerCharacterNitPickerCatchphrase,
        PlayerCharacterNitPickerHitCharacter = BattleSoundFX.PlayerCharacterNitPickerHitCharacter,
        PlayerCharacterNitPickerHitShield    = BattleSoundFX.PlayerCharacterNitPickerHitShield,
        PlayerCharacterNitPickerDeath        = BattleSoundFX.PlayerCharacterNitPickerDeath

        #endregion  Character Sound Effects
    }
}
