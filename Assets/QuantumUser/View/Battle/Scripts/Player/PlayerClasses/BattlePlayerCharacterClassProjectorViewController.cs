/// @file BattlePlayerCharacterClassProjectorViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerCharacterClassProjectorViewController} class,
/// which is a <see cref="Battle.View.Player.BattlePlayerCharacterClassProjectorViewController">class %view controller</see> for the Projector character class.
/// </summary>

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle View usings
using Battle.View.Game;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">Projector <see cref="Battle.View.Player.BattlePlayerCharacterClassBaseViewController">class %view controller</see>.</span><br/>
    /// Handles view logic for the Projector character class
    /// </summary>
    ///
    /// @bigtext{See [{PlayerClassViewController}](#page-concepts-player-view-class-controller) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext{See [{Player Character Class 400 - Projector}](#page-concepts-player-class-400-projector) for more info.}
    public class BattlePlayerCharacterClassProjectorViewController : BattlePlayerCharacterClassBaseViewController
    {

        /// <summary>
        /// Gets the character class associated with this Class.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.Projector">BattlePlayerCharacterClass.Projector</see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Projector;
    }
}
