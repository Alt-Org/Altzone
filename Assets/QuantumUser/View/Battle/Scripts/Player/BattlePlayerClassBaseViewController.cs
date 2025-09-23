/// @file BattlePlayerClassBaseViewController.cs
/// <summary>
/// Has a class BattlePlayerClassBaseViewController which serves as the base for player character class view controllers.
/// </summary>
///
/// Provides the foundation for character class specific view controllers.<br/>
/// Works together with BattlePlayerViewController, which handles the shared player view logic.

using Quantum;
using UnityEngine;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">%Class base view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles player visuals, providing overridable functionality for class specific behavior.
    /// </summary>
    public abstract class BattlePlayerClassBaseViewController : MonoBehaviour
    {
        /// <summary>Must be implemented by derived class view controllers.</summary>
        /// <value>The <see cref="BattlePlayerCharacterClass"/> associated with this view controller.</value>
        public abstract BattlePlayerCharacterClass Class { get; }

        /// <summary>
        /// Initializes the view controller for the given player entity.<br/>
        /// Stores references to the parent controller and entity, then calls <see cref="OnViewInitOverride"/> for class specific setup.
        /// </summary>
        ///
        /// <param name="parent">The parent <see cref="BattlePlayerViewController"/> handling shared logic.</param>
        /// <param name="entityRef">The entity reference for this player character.</param>
        /// <param name="slot">The player slot associated with this character.</param>
        /// <param name="characterId">The ID of the character being initialized.</param>
        public void OnViewInit(BattlePlayerViewController parent, EntityRef entityRef, BattlePlayerSlot slot, int characterId)
        {
            _parent = parent;
            _entityRef = entityRef;
            OnViewInitOverride(slot, characterId);
        }

        /// <summary>
        /// Called when the character takes damage.
        /// Can be overridden in derived classes.
        /// </summary>
        ///
        /// <param name="e">The damage event data.</param>
        public virtual void OnCharacterTakeDamage(EventBattleCharacterTakeDamage e) { }

        /// <summary>
        /// Called when the character's shield takes damage.
        /// </summary>
        ///
        /// <param name="e">The shield damage event data.</param>
        public virtual void OnShieldTakeDamage(EventBattleShieldTakeDamage e) { }

        /// <summary>
        /// Called once per frame to update the character class view.
        /// Can be overridden in derived classes.
        /// </summary>
        public virtual void OnUpdateView() { }

        /// <summary>Reference to the parent <see cref="BattlePlayerViewController"/> that manages shared player view logic.</summary>
        protected BattlePlayerViewController _parent;
        /// <summary>Reference to the entity associated with this view controller.</summary>
        protected EntityRef _entityRef;

        /// <summary>
        /// Provides a hook for derived classes to perform custom initialization.<br/>
        /// Called by <see cref="OnViewInit"/> with with the player slot and character ID.
        /// </summary>
        ///
        /// <param name="slot">The player slot associated with this character.</param>
        /// <param name="characterId">The ID of the character being initialized.</param>
        protected virtual void OnViewInitOverride(BattlePlayerSlot slot, int characterId) { }
    }
}
