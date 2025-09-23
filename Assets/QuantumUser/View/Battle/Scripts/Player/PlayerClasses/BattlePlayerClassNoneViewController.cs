/// @file BattlePlayerClassNoneViewController.cs
/// <summary>
/// A view controller for the <see cref="Quantum.BattlePlayerCharacterClass.None"/> class.
/// </summary>
///
/// Provides a placeholder implementation of BattlePlayerClassBaseViewController.<br/>
/// Used when no character class is assigned, and contains no custom behavior.

using Quantum;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">None <see cref="Battle.View.Player.BattlePlayerClassBaseViewController">class %view controller</see>.</span><br/>
    /// Default none/empty view controller with no additional logic.
    /// </summary>
    public class BattlePlayerClassNoneViewController : BattlePlayerClassBaseViewController
    {
        /// <summary>
        /// Gets the character class associated with this controller.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.None"/>.
        /// </summary>
        public override BattlePlayerCharacterClass Class => BattlePlayerCharacterClass.None;
    }
}
