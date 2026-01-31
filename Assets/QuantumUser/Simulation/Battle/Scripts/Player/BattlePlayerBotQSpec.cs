/// @file BattlePlayerBotQSpec.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerBotQSpec} class for defining bot behavior and other spec settings.
/// </summary>
///
/// @bigtext{Filled with data from @ref BattlePlayerBotQSpec.asset "BattlePlayerBotQSpec" data asset.}

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Class for defining bot behavior and other spec settings.
    /// </summary>
    ///
    /// This class is used to define the data asset's structure, the data itself is not contained here.<br/>
    /// Can be used to make multiple %BattlePlayerBotQSpec data assets.<br/>
    /// @bigtext{Filled with data from @ref BattlePlayerBotQSpec.asset "BattlePlayerBotQSpec" data asset.}
    public class BattlePlayerBotQSpec : AssetObject
    {
        [Header("AI")]

        [Tooltip("Min cooldown between movements")]
        /// <summary>Min cooldown between movements.</summary>
        public FP MovementCooldownSecMin;

        [Tooltip("Max cooldown between movements")]
        /// <summary>Max cooldown between movements.</summary>
        public FP MovementCooldownSecMax;

        [Tooltip("How many seconds bot look ahead in the future when predicting the projectile")]
        /// <summary>How many seconds bot look ahead in the future when predicting the projectile.</summary>
        public FP LookAheadTimeSec;

        [Tooltip("Chance for bot to perform a random movement\n0 = never misses\n1 = always misses")]
        /// <summary>Chance for bot to perform a random movement.</summary>
        /// 0 = never misses.<br/>
        /// 1 = always misses.
        [RangeEx(0, 1)]
        public FP MissClickChance;

        [Tooltip("How much the bot movement can differ from the predicted position\nIgnored when bot performs a random movement")]
        /// <summary>
        /// How much the bot movement can differ from the predicted position.<br/>
        /// Ignored when bot performs a random movement.
        /// </summary>
        public FP Inaccuracy;

        [Header("Character")]

        [Tooltip("Characters that bots can use")]
        /// <summary>Character selection that all bots use.</summary>
        public BattleCharacterBase[] BotCharacterSelection;
    }
}
