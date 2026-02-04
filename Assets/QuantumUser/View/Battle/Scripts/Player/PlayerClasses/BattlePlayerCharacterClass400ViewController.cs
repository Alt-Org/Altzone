/// @file BattlePlayerCharacterClass400ViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerCharacterClass400ViewController} class,
/// which is a <see cref="Battle.View.Player.BattlePlayerCharacterClass400ViewController">character class %view controller</see> for the Projector character class.
/// </summary>

// Quantum usings
using Quantum;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">Projector <see cref="Battle.View.Player.BattlePlayerCharacterClassBaseViewController">character class %view controller</see>.</span><br/>
    /// Handles view logic for the Projector character class
    /// </summary>
    ///
    /// @bigtext{See [{PlayerCharacterClassViewController}](#page-concepts-player-view-character-class-controller) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext{See [{Player Character Class 400 - Projector}](#page-concepts-player-class-400) for more info.}
    public class BattlePlayerCharacterClass400ViewController : BattlePlayerCharacterClassBaseViewController
    {
        /// <summary>
        /// Gets the character class associated with this Class.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.Projector">BattlePlayerCharacterClass.Projector</see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.Class400;
    }
}
