/// @file BattlePlayerClassNoneViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerClassNoneViewController} class,
/// which is a default none/empty <see cref="Battle.View.Player.BattlePlayerClassNoneViewController">class %view controller</see> with no additional view logic.
/// </summary>

// Quantum usings
using Quantum;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">None <see cref="Battle.View.Player.BattlePlayerClassBaseViewController">class %view controller</see>.</span><br/>
    /// Default none/empty view controller with no additional view logic.
    /// </summary>
    ///
    /// See [{PlayerClassViewControllers}](#page-concepts-player-view-class-controller) for more info.<br/>
    /// See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.
    public class BattlePlayerClassNoneViewController : BattlePlayerClassBaseViewController
    {
        /// <summary>
        /// Gets the character class associated with this controller.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.None">BattlePlayerCharacterClass.None</see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.None;
    }
}
