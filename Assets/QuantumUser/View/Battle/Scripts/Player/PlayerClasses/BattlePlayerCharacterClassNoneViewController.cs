/// @file BattlePlayerClassNoneViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerCharacterClassNoneViewController} class,
/// which is a default none/empty <see cref="Battle.View.Player.BattlePlayerCharacterClassNoneViewController">character class %view controller</see> with no additional view logic.
/// </summary>

// Quantum usings
using Quantum;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">None <see cref="Battle.View.Player.BattlePlayerCharacterClassBaseViewController">character class %view controller</see>.</span><br/>
    /// Default none/empty character view controller with no additional view logic.
    /// </summary>
    ///
    /// See [{PlayerCharacterClassViewControllers}](#page-concepts-player-view-character-class-controller) for more info.<br/>
    /// See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.
    public class BattlePlayerCharacterClassNoneViewController : BattlePlayerCharacterClassBaseViewController
    {
        /// <summary>
        /// Gets the character class associated with this controller.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.None">BattlePlayerCharacterClass.None</see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.None;
    }
}
