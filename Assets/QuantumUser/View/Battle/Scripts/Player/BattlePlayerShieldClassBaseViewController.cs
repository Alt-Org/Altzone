/// @file BattlePlayerShieldClassBaseViewController.cs
/// <summary>
/// Contains @cref{Battle.View.Player,BattlePlayerShieldClassBaseViewController} class,
/// which is an abstract base class for the shield class view controllers to extend.<br/>
/// Works together with @cref{Battle.View.Player,BattlePlayerShieldViewController}, which handles the shared player shield view logic.
/// </summary>

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

namespace Battle.View.Player
{
    /// <summary>
    /// <span class="brief-h">%Abstract base shield class view controller <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Abstract base class for the shield class view controllers to extend.
    /// </summary>
    ///
    /// Defines shield class view controller structure, implements base non-specific view logic for all shield classes.
    /// Provides hooks for derived classes to implement shield class specific view logic.
    /// Works together with @cref{Battle.View.Player,BattlePlayerShieldViewController}, which handles the shared player shield view logic.
    ///
    /// See [{PlayerShieldClassViewControllers}](#page-concepts-player-view-shield-class-controller) for more info.<br/>
    /// See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.
    public abstract class BattlePlayerShieldClassBaseViewController : MonoBehaviour
    {
        /// <summary>Must be implemented by derived shield class view controllers.</summary>
        /// <value>The <see cref="BattlePlayerCharacterClass"/> associated with this shield view controller.</value>
        public abstract BattlePlayerCharacterClass Class { get; }

        /// <summary>
        /// Initializes this instance of a shield class view controller. Performs base non-specific initialization for all shield classes.<br/>
        /// Stores references to the parent controller and entity, then calls the protected <see cref="OnViewInitOverride"/> method that performs the shield class specific initialization.
        /// </summary>
        ///
        /// This is not an overridable method. Override the @cref{OnViewInitOverride} method in derived classes for implementing shield class specific custom initialization.
        ///
        /// <param name="parent">Reference to the parent (<see cref="BattlePlayerShieldViewController"/>).</param>
        /// <param name="entityRef">The entity reference for this player shield.</param>
        /// <param name="slot">The player slot associated with this shield.</param>
        /// <param name="characterId">The ID of the shield being initialized.</param>
        public void OnViewInit(BattlePlayerShieldViewController parent, EntityRef entityRef, BattlePlayerSlot slot, int characterId)
        {
            _parent = parent;
            _entityRef = entityRef;
            OnViewInitOverride(slot, characterId);
        }

        /// <summary>
        /// Called when the character's shield takes damage.
        /// Provides a hook for derived classes to implement shield class specific view logic.
        /// </summary>
        ///
        /// <param name="e">The shield damage event data.</param>
        public virtual void OnShieldTakeDamage(EventBattleShieldTakeDamage e) { }

        /// <summary>
        /// Called once per frame to update the shield class view.
        /// Provides a hook for derived classes to implement shield class specific view logic.
        /// </summary>
        public virtual void OnUpdateView() { }

        /// <summary>Reference to the parent <see cref="BattlePlayerShieldViewController">shield view controller</see> that manages shared player shield view logic.</summary>
        protected BattlePlayerShieldViewController _parent;
        /// <summary>Reference to the entity associated with this shield view controller.</summary>
        protected EntityRef _entityRef;

        /// <summary>
        /// Provides a hook for derived classes to perform shield class specific custom initialization.<br/>
        /// Called by <see cref="OnViewInit"/> that performs base non-specific initialization before calling this method.
        /// </summary>
        ///
        /// Use the public @cref{OnViewInit} method when initializing an instance of a shield class.
        ///
        /// <param name="slot">The player slot associated with this shield.</param>
        /// <param name="characterId">The ID of the shield being initialized.</param>
        protected virtual void OnViewInitOverride(BattlePlayerSlot slot, int characterId) { }
    }
}
