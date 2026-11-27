
// System usings
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;
using Quantum.Collections;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Player;
using Battle.QSimulation.Projectile;

namespace Battle.QSimulation.Game
{
    /// <summary>
    /// <b>{Entity Type Handle}</b> for handling <b>{Managed Entities}</b>
    /// </summary>
    ///
    /// This **Type Handle** differs from the usual implementation, as it wraps the **EntityRef**
    /// and the @cref{Quantum,BattleManagedEntityQComponent} instead of containing a collection of data.<br/>
    /// The BattleManagedEntityHandle can be used as an **EntityRef** and exposes the data of the underlying @cref{Quantum,BattleManagedEntityQComponent}.
    ///
    ///
    /// See [{Entity Type Handle}](#page-concepts-battle-handle-entity-type-handle) for more info.<br/>
    /// See [{Managed entities}](#page-concepts-entity-management-managed-entities) for more info.
    public unsafe struct BattleManagedEntityHandle
    {
        /// <summary>
        /// The interface implemented by <b>{Managed Composite Entity Type Handles}</b>
        /// </summary>
        ///
        /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
        public interface IComposite
        {
            /// <summary>
            /// Used internally by <see cref="BattleManagedEntityHandle"/><b>.To</b> method in order to upgrade
            /// the <see cref="BattleManagedEntityHandle"/> to a specific <b>{Managed Composite Entity Type Handle}</b>.
            /// </summary>
            ///
            /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
            ///
            /// <param name="f">Current simulation frame.</param>
            /// <param name="managedEntity">The managed entity handle being upgraded.</param>
            public void FromManagedEntity(Frame f, BattleManagedEntityHandle managedEntity);
        }

        /// <summary>
        /// Method for creating a new handle.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entity">Entity the handle is being created for.</param>
        ///
        /// <returns>The created handle.</returns>
        public static BattleManagedEntityHandle Create(Frame f, EntityRef entity)
        {
            BattleManagedEntityHandle handle = new()
            {
                ERef = entity,
                _component = f.Unsafe.GetPointer<BattleManagedEntityQComponent>(entity)
            };

            return handle;
        }

        /// <summary>
        /// Implements implicit cast from <see cref="BattleManagedEntityHandle"/> to <b>EntityRef</b>.
        /// </summary>
        ///
        /// <param name="managedEntityHandle">Managed Entity Handle to be cast.</param>
        public static implicit operator EntityRef(BattleManagedEntityHandle managedEntityHandle) => managedEntityHandle.ERef;

        /// <summary>
        /// The underlying entity.
        /// </summary>
        public EntityRef ERef { get; private set; }

        /// <summary>
        /// Whether the <b>{Managed Entity}</b> is of type <b>{Registered}</b> or not.
        /// </summary>
        public bool IsRegistered => _component->IsRegistered;

        /// <summary>
        /// The <b>{PlayState}</b> of a <b>{Registered Entity}</b>. Unused if not <see cref="IsRegistered"/>.
        /// </summary>
        public BattleEntityPlayState RegisteredState { get
        {
            DevAssertIsRegistered("Can not read the play state");
            return _component->RegisteredState;
        }}

        /// <summary>
        /// Whether the <b>{Managed Entity}</b> is of type <b>{Compound}</b> or not.
        /// </summary>
        public bool IsCompound => _component->IsCompound;

        /// <summary>
        /// Fetches the <b>list of Entities linked</b> to the <b>{Compound}</b> parent. Should not be used if not <see cref="IsCompound"/>.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>The fetched <b>list of linked Entities</b>.</returns>
        public QList<BattleEntityLink> CompoundGetLinkedEntities(Frame f)
        {
            DevAssertIsCompound("Can not " + nameof(CompoundGetLinkedEntities));
            return f.ResolveList(_component->CompoundLinkedEntities);
        }

        /// <summary>
        /// Upgrades the <see cref="BattleManagedEntityHandle"/> to a <b>{Managed Composite Entity Type Handle}</b> of type <typeparamref name="T"/>.
        /// </summary>
        ///
        /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
        ///
        /// <typeparam name="T">The type of <b>{Managed Composite Entity Type Handle}</b> to upgrade to.</typeparam>
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>The upgraded <b>{Managed Composite Entity Type Handle}</b>.</returns>
        public T To<T>(Frame f) where T : IComposite, new()
        {
            T composite = new();
            composite.FromManagedEntity(f, this);
            return composite;
        }

        /// <summary>
        /// Asserts that <see cref="IsRegistered"/>.
        /// </summary>
        ///
        /// <param name="context">Context included in the assert error message to indicate where the assertion was called from.</param>
        public void DevAssertIsRegistered(string context) => DevAssertIsRegistered(nameof(BattleManagedEntityHandle), context);

        /// <summary>
        /// Asserts that <see cref="IsRegistered"/>.
        /// </summary>
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "{Error Message Source}" for more info.
        ///
        /// <param name="source">Specifies the <b>{Source}</b> of the assert error message.</param>
        /// <param name="context">Context included in the assert error message to indicate where the assertion was called from.</param>
        public void DevAssertIsRegistered(string source, string context)
        {
            BattleDebugLogger.DevAssertFormat(source, _component->IsRegistered, "{0}, entity is not registered", context);
        }

        /// <summary>
        /// Asserts that <see cref="RegisteredState"/> is <see cref="BattleEntityPlayState.InPlay">InPlay</see>.
        /// </summary>
        ///
        /// <param name="context">Context included in the assert error message to indicate where the assertion was called from.</param>
        public void DevAssertIsInPlay(string context) => DevAssertIsInPlay(nameof(BattleManagedEntityHandle), context);

        /// <summary>
        /// Asserts that <see cref="RegisteredState"/> is <see cref="BattleEntityPlayState.InPlay">InPlay</see>.
        /// </summary>
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "{Error Message Source}" for more info.
        ///
        /// <param name="source">Specifies the <b>{Source}</b> of the assert error message.</param>
        /// <param name="context">Context included in the assert error message to indicate where the assertion was called from.</param>
        public void DevAssertIsInPlay(string source, string context)
        {
            BattleDebugLogger.DevAssertFormat(source, _component->RegisteredState == BattleEntityPlayState.InPlay, "{0}, entity is not in play", context);
        }

        /// <summary>
        /// Asserts that <see cref="IsCompound"/>.
        /// </summary>
        ///
        /// <param name="context">Context included in the assert error message to indicate where the assertion was called from.</param>
        public void DevAssertIsCompound(string context) => DevAssertIsCompound(nameof(BattleManagedEntityHandle), context);

        /// <summary>
        /// Asserts that <see cref="IsCompound"/>.
        /// </summary>
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "{Error Message Source}" for more info.
        ///
        /// <param name="source">Specifies the <b>{Source}</b> of the assert error message.</param>
        /// <param name="context">Context included in the assert error message to indicate where the assertion was called from.</param>
        public void DevAssertIsCompound(string source, string context)
        {
            BattleDebugLogger.DevAssertFormat(source, _component->IsCompound, "{0}, entity is not a compound", context);
        }

        /// <summary>
        /// Private internal pointer to the underlying <see cref="BattleManagedEntityQComponent"/>.
        /// </summary>
        private BattleManagedEntityQComponent* _component;
    }

    /// <summary>
    /// A <b>{Handle}</b> for a <b>%Player</b> and a <b>{Managed Composite Entity Type Handle}</b> for handling their <b>{Character Entities}</b>.
    /// </summary>
    ///
    /// See [{PlayerHandle}](#page-concepts-player-simulation-management-playerhandle) for more info.<br/>
    /// See [{BattleHandle}](#page-concepts-battle-handle) for more info.<br/>
    /// See [{Managed Composite Entity Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
    /// See [{Character Entity}](#page-concepts-player-character-and-shield-entity) for more info.
    public unsafe struct BattlePlayerHandle : BattleManagedEntityHandle.IComposite
    {
        //! @anchor BattlePlayerHandle-CreateMethods
        //! @name Create Methods
        //!
        //! Methods for **Creating** a [{PlayerHandle}](#page-concepts-player-simulation-management-playerhandle).
        //!
        //! @{

        /// <summary>
        /// <b>Creates</b> a <b>{PlayerHandle}</b> for a <b>%Player</b> based on <paramref name="slot"/>.
        /// </summary>
        ///
        /// You can use @cref{Battle.QSimulation.Game.BattlePlayerHandle,Create(BattlePlayerData.Ref)}
        /// if you already have a @cref{Quantum,BattlePlayerData.Ref}.<br/>
        /// Use @cref{Battle.QSimulation.Game.BattlePlayerHandle,Create(Frame\, EntityRef)}
        /// to <b>Create</b> a <b>Handle</b> for the <b>%Player</b> and a <b>Character</b>.
        ///
        /// See [{PlayerHandle}](#page-concepts-player-simulation-management-playerhandle) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="slot">Slot of the <b>%Player</b> the handle is being created for.</param>
        ///
        /// <returns>The <b>Created Handle</b>.</returns>
        public static BattlePlayerHandle Create(Frame f, BattlePlayerSlot slot)
        {
            BattlePlayerHandle handle = new();
            handle.PlayerData = BattlePlayerManager.GetPlayerData(f, slot);
            return handle;
        }

        /// <summary>
        /// <b>Creates</b> a <b>{PlayerHandle}</b> for a <b>%Player</b> from <paramref name="playerData"/>.
        /// </summary>
        ///
        /// You can use @cref{Battle.QSimulation.Game.BattlePlayerHandle,Create(Frame\, BattlePlayerSlot)}
        /// if you don't have a @cref{Quantum,BattlePlayerData.Ref}.<br/>
        /// Use @cref{Battle.QSimulation.Game.BattlePlayerHandle,Create(Frame\, EntityRef)}
        /// to <b>Create</b> a <b>Handle</b> for the <b>%Player</b> and a <b>Character</b>.
        ///
        /// See [{PlayerHandle}](#page-concepts-player-simulation-management-playerhandle) for more info.<br/>
        /// See [{Player Data}](#page-concepts-player-data) for more info.
        ///
        /// <param name="playerData">Reference to <b>{Data}</b> of the <b>%Player</b> the handle is being <b>Created</b> for.</param>
        ///
        /// <returns>The <b>Created Handle</b>.</returns>
        public static BattlePlayerHandle Create(BattlePlayerData.Ref playerData)
        {
            BattlePlayerHandle handle = new();
            handle.PlayerData = playerData;
            return handle;
        }

        /// <summary>
        /// <b>Creates</b> a <b>{PlayerHandle}</b> for a <b>%Player</b> with a <b>{Loaded Character}</b>
        /// based on <b>{Linked/Associated}</b> <paramref name="entityRef"/>.
        /// </summary>
        ///
        /// Use @cref{Battle.QSimulation.Game.BattlePlayerHandle,Create(Frame\, BattlePlayerSlot)}
        /// or @cref{Battle.QSimulation.Game.BattlePlayerHandle,Create(BattlePlayerData.Ref)}
        /// to <b>Create</b> a <b>Handle</b> for just the <b>%Player</b> itself.
        ///
        /// See [{PlayerHandle}](#page-concepts-player-simulation-management-playerhandle) for more info.<br/>
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.<br/>
        /// See [{Linked/Associated Entity}](#page-concepts-player-simulation-management-playerhandle-linked-entities) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityRef">EntityRef of the <b>%Player</b> the handle is being created for.</param>
        ///
        /// <returns>The <b>Created Handle</b>.</returns>
        public static BattlePlayerHandle Create(Frame f, EntityRef entityRef)
        {
            BattlePlayerLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerLinkQComponent>(entityRef);

            BattlePlayerHandle handle = new();

            handle.PlayerData = BattlePlayerManager.GetPlayerData(f, link->Slot);
            handle.SetCharacter(f, BattleManagedEntityHandle.Create(f, link->CharacterEntityRef));

            if (link->Type == BattlePlayerLinkEntityType.Hitbox) handle.SetHitbox(f, entityRef);

            return handle;
        }

        //! @}

        //! @anchor BattlePlayerHandle-LinkCreationMethods
        //! @name Link/Association Creation Methods
        //!
        //! Methods for creating a [{Link/Association}](#page-concepts-player-simulation-management-playerhandle-linked-entities) between the **%Player** and **Entities**.
        //!
        //! @{

        /// <summary>
        /// Creates a <b>{Link/Association}</b> between (<paramref name="slot"/>, <paramref name="characterEntityRef"/>)
        /// and <paramref name="linkEntityRef"/> of <paramref name="type"/>.
        /// </summary>
        ///
        /// See [{Linked/Associated Entity}](#page-concepts-player-simulation-management-playerhandle-linked-entities) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="slot">Slot <b>{Associated}</b> with <paramref name="linkEntityRef"/></param>
        /// <param name="characterEntityRef">Character EntityRef <b>{Associated}</b> with <paramref name="linkEntityRef"/></param>
        /// <param name="type">The type of entity this <b>{Association}</b> is being created for.</param>
        /// <param name="linkEntityRef">EntityRef of the entity this <b>{Link/Association}</b> is being created for.</param>
        public static void CreateLink(Frame f, BattlePlayerSlot slot, EntityRef characterEntityRef, BattlePlayerLinkEntityType type, EntityRef linkEntityRef)
        {
            BattlePlayerLinkQComponent playerLink = new()
            {
                CharacterEntityRef = characterEntityRef,
                Slot = slot,
                Type = type
            };

            f.Add(linkEntityRef, playerLink);
        }

        //! @}

        //! @anchor BattlePlayerHandle-PlayerProperties
        //! @name Player Properties
        //!
        //! Data related to the **%Player** itself.
        //!
        //! @{

        /// <summary>
        /// <b>{%Player Data}</b> of <b>this Handle's</b> <b>%Player</b>.
        /// </summary>
        ///
        /// See [{Player Data}](#page-concepts-player-data) for more info.
        public BattlePlayerData.Ref PlayerData { get; private set; }

        /// <summary>
        /// Whether <b>%Player</b> has <b>{Selected a Character}</b> or not.
        /// </summary>
        ///
        /// See [{Selected Character}](#page-concepts-player-character-entity-selected-character) for more info.
        public readonly bool PlayerHasSelectedCharacter => PlayerData.SelectedCharacterNumber != BattlePlayerData.NoSelectedCharacter;

        //! @}

        //! @anchor BattlePlayerHandle-LoadedCharacterProperties
        //! @name Loaded Character Properties
        //!
        //! Data related to the [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character).
        //! - @ref BattlePlayerHandle-LoadedCharacterMethods "Loaded Character Methods"
        //!
        //! @{

        /// <summary>
        /// Whether <b>this Handle</b> has a <b>{Loaded Character}</b> or not.
        /// </summary>
        ///
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.
        public readonly bool HandleHasCharacterLoaded => LoadedCharacterEntityHandle.ERef != EntityRef.None;

        /// <summary>
        /// Whether <b>this Handle's</b> <b>{Character is Selected}</b> or not.
        /// </summary>
        ///
        /// See [{Selected Character}](#page-concepts-player-character-entity-selected-character) for more info.
        public readonly bool LoadedCharacterIsSelected => LoadedCharacterData->Number == PlayerData.SelectedCharacterNumber;

        /// <summary>
        /// <see cref="BattleManagedEntityHandle"/> of the <b>{Loaded Character}</b>, if any <b>{Character is Loaded}</b>.
        /// </summary>
        ///
        /// Use @cref{HandleHasCharacterLoaded} to check if the handle has a <b>{Loaded Character}</b>.
        ///
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.
        public BattleManagedEntityHandle LoadedCharacterEntityHandle { get; private set; }

        /// <summary>
        /// Pointer to the <see cref="BattlePlayerCharacterDataQComponent"/> of the <b>{Loaded Character}</b>, if any <b>{Character is Loaded}</b>.
        /// </summary>
        ///
        /// Use @cref{HandleHasCharacterLoaded} to check if the handle has a <b>{Loaded Character}</b>.
        ///
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.
        public BattlePlayerCharacterDataQComponent* LoadedCharacterData { get; private set; }

        /// <summary>
        /// Pointer to the <b>Transform2D</b> of the <b>{Loaded Character}</b> when their <b>{Registered Entity}</b> is <b>{InPlay}</b>, if any <b>{Character is Loaded}</b>.
        /// @note <b>{Protected}</b> from possible unauthorized modification when the <b>{Loaded Character's}</b> <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.
        /// </summary>
        ///
        /// Use @cref{LoadedCharacterTransformCopy} to access the transform when the <b>Player's Character's</b> <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.<br/>
        /// Use @cref{HandleHasCharacterLoaded} to check if the handle has a <b>{Loaded Character}</b>.<br/>
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.<br/>
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        public Transform2D* LoadedCharacterTransformPtr
        {
            get
            {
                LoadedCharacterEntityHandle.DevAssertIsInPlay(nameof(BattlePlayerHandle), "Improper transform access: possible unauthorized modification");
                return _loadedCharacterTransformInternalPtr;
            }
        }

        /// <summary>
        /// Access to a copy of the <b>{Protected}</b> <b>Transform2D</b> of the <b>{Loaded Character}</b>, if any <b>{Character is Loaded}</b>.<br/>
        /// The <b>Transform2D</b> is copied to ensure immutability when the <b>{Loaded Character}</b>'s <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.
        /// </summary>
        ///
        /// Use @cref{LoadedCharacterTransformPtr} to modify the transform when the <b>Player's Character's</b> <b>{Registered Entity}</b> is <b>{InPlay}</b>.<br/>
        /// Use @cref{HandleHasCharacterLoaded} to check if the handle has a <b>{Loaded Character}</b>.<br/>
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.<br/>
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{InPlay/OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        public Transform2D LoadedCharacterTransformCopy => *_loadedCharacterTransformInternalPtr;

        //! @}

        //! @anchor BattlePlayerHandle-LoadedCharacterAttachedShieldProperties
        //! @name Loaded Character Attached Shield Properties
        //!
        //! Data related to the [{Loaded Character's}](#page-concepts-player-simulation-management-playerhandle-loaded-character)
        //! [{Attached Shield}](#page-concepts-player-character-entity-shield-attach)
        //!
        //! - @ref BattlePlayerHandle-LoadedCharacterAttachedShieldMethods "Loaded Character Attached Shield Methods"
        //!
        //! @{

        /// <summary>
        /// Whether <b>this Handle's</b> <b>{Loaded Character}</b> has an <b>{Attached Shield}</b> or not.
        /// </summary>
        ///
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.<br/>
        /// See [{Attached Shield}](#page-concepts-player-character-entity-shield-attach) for more info.
        public readonly bool LoadedCharacterHasShieldAttached => LoadedCharacterData->AttachedShieldEntityRef != EntityRef.None;

        //! @}

        //! @anchor BattlePlayerHandle-CollidedHitboxProperties
        //! @name Collided Hitbox Properties
        //!
        //! Data related to the [{Collided Hitbox}](#page-concepts-player-simulation-management-playerhandle-collided-hitbox),
        //! which may or may not be [{Available}](#page-concepts-player-simulation-management-playerhandle-collided-hitbox).
        //!
        //! @{

        /// <summary>
        /// Whether this handle has a <see cref="CollidedHitboxEntityRef"/> <b>{Available}</b> or not.
        /// </summary>
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playerhandle-collided-hitbox) for more info.
        public readonly bool HandleHasCollidedHitbox => CollidedHitboxEntityRef != EntityRef.None;

        /// <summary>
        /// The EntityRef of the collided hitbox, if <b>{Available}</b>.
        /// </summary>
        ///
        /// Use @cref{HandleHasCollidedHitbox} to check if collided hitbox is <b>{Available}</b>.
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playerhandle-collided-hitbox) for more info.
        public EntityRef CollidedHitboxEntityRef { get; private set; }

        /// <summary>
        /// Pointer to the <see cref="BattlePlayerHitboxQComponent"/> of the collided hitbox, if <b>{Available}</b>.
        /// </summary>
        ///
        /// Use @cref{HandleHasCollidedHitbox} to check if collided hitbox is <b>{Available}</b>.
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playerhandle-collided-hitbox) for more info.
        public BattlePlayerHitboxQComponent* CollidedHitboxComponent { get; private set; }

        /// <summary>
        /// Pointer to the <b>Transform2D</b> of the collided hitbox, if <b>{Available}</b>.
        /// </summary>
        ///
        /// Use @cref{HandleHasCollidedHitbox} to check if collided hitbox is <b>{Available}</b>.
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playerhandle-collided-hitbox) for more info.
        public Transform2D* CollidedHitboxTransform { get; private set; }

        //! @}

        //! @anchor BattlePlayerHandle-LoadedCharacterMethods
        //! @name Loaded Character Methods
        //!
        //! Methods for [{Loading Characters}]( #page-concepts-player-simulation-management-playerhandle-loaded-character)
        //! and other related methods.
        //!
        //! - @ref BattlePlayerHandle-LoadedCharacterProperties "Loaded Character Properties"
        //!
        //! @{

        /// <summary>
        /// <b>{Loads a Character}</b> based on <paramref name="characterNumber"/>
        /// </summary>
        ///
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.<br/>
        /// See [{Character Entity}](#page-concepts-player-character-and-shield-entity) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="characterNumber">Number of the <b>{Character}</b> to be <b>{Loaded}</b>.</param>
        public void LoadCharacter(Frame f, int characterNumber)
        {
            BattlePlayerData.DevAssertIsValidCharacterNumber(nameof(BattlePlayerHandle), characterNumber);
            SetCharacter(f, BattleEntityManager.Get(f, PlayerData.Low_Level.CharacterEntityGroupID, characterNumber));
        }

        /// <summary>
        /// <b>Creates</b> a <b>Copy</b> of the <b>{%Player Handle}</b> with a specific <b>{Loaded Character}</b>
        /// based on <paramref name="characterNumber"/>.
        /// </summary>
        ///
        /// See [{PlayerHandle}](#page-concepts-player-simulation-management-playerhandle) for more info.<br/>
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.<br/>
        /// See [{Character Entity}](#page-concepts-player-character-and-shield-entity) for more info.<br/>
        /// See [{Character Number}](#page-concepts-player-character-entity-character-number) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="characterNumber">Number of the <b>{Character}</b> to be <b>{Loaded}</b> in the <b>Copy</b>.</param>
        ///
        /// <returns>The <b>Created</b> <b>Copy</b> of the <b>{%Player Handle}</b>.</returns>
        public BattlePlayerHandle LoadCharacterCopy(Frame f, int characterNumber)
        {
            BattlePlayerData.DevAssertIsValidCharacterNumber(nameof(BattlePlayerHandle), characterNumber);
            BattlePlayerHandle copy = Create(PlayerData);
            copy.LoadCharacter(f, characterNumber);
            return copy;
        }

        /// <summary>
        /// Asserts that <see cref="HandleHasCharacterLoaded"/>
        /// </summary>
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "{Error Message Source}" for more info.
        ///
        /// <param name="source">Specifies the <b>{Source}</b> of the assert error message.</param>
        /// <param name="errorContext">Context included in the assert error message to indicate where the assertion was called from.</param>
        public void DevAssertPlayerHandleHasCharacterLoaded(string source, string errorContext)
        {
            BattleDebugLogger.DevAssertFormat(source, HandleHasCharacterLoaded, "{0}, PlayerHandle has no loaded character", errorContext);
        }

        //! @}

        //! @anchor BattlePlayerHandle-LoadedCharacterAttachedShieldMethods
        //! @name Loaded Character Attached Shield Methods
        //!
        //! Methods for handling the [{Loaded Character's}](#page-concepts-player-simulation-management-playerhandle-loaded-character)
        //! [{Attached Shield}](#page-concepts-player-character-entity-shield-attach)
        //!
        //! - @ref BattlePlayerHandle-LoadedCharacterAttachedShieldProperties "Loaded Character Attached Shield Properties"
        //!
        //! @{

        /// <summary>
        /// Retrieves the <see cref="BattlePlayerShieldHandle"/> of the <b>{Loaded Character's}</b> <b>{Attached Shield}</b>.
        /// </summary>
        ///
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.<br/>
        /// See [{Attached Shield}](#page-concepts-player-character-entity-shield-attach) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>The <see cref="BattlePlayerShieldHandle"/> of the <b>{Loaded Character's}</b> <b>{Attached Shield}</b>.</returns>
        public BattlePlayerShieldHandle GetLoadedCharacterAttachedShield(Frame f)
        {
            return BattlePlayerShieldHandle.Create(f, LoadedCharacterData->AttachedShieldEntityRef);
        }

        //! @}

        /// <summary>
        /// Private internal storage for the <b>{Protected}</b> <b>Transform2D</b> pointer of the <b>{Loaded Character}</b>.
        /// </summary>
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.
        private Transform2D* _loadedCharacterTransformInternalPtr;

        /// <summary>
        /// Private helper method that handles <b>{Loading}</b> a <b>{Character}</b> <paramref name="entityHandle"/>.
        /// </summary>
        ///
        /// See [{Loaded Character}](#page-concepts-player-simulation-management-playerhandle-loaded-character) for more info.<br/>
        /// See [{Character Entity}](#page-concepts-player-character-and-shield-entity) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityHandle"><b>{Character Entity}</b> to be <b>{Loaded}</b></param>
        private void SetCharacter(Frame f, BattleManagedEntityHandle entityHandle)
        {
            LoadedCharacterEntityHandle          = BattleManagedEntityHandle.Create(f, entityHandle);
            LoadedCharacterData                  = f.Unsafe.GetPointer<BattlePlayerCharacterDataQComponent>(entityHandle);
            _loadedCharacterTransformInternalPtr = f.Unsafe.GetPointer<Transform2D>(entityHandle);
        }

        /// <summary>
        /// Private helper method for capturing a <b>Hitbox</b> <paramref name="entityRef"/>
        /// and making it <b>{Available}</b>.
        /// </summary>
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playerhandle-collided-hitbox) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityRef">Hitbox to capture and make <b>{Available}</b>.</param>
        private void SetHitbox(Frame f, EntityRef entityRef)
        {
            CollidedHitboxEntityRef = entityRef;
            CollidedHitboxComponent = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(entityRef);
            CollidedHitboxTransform = f.Unsafe.GetPointer<Transform2D>(entityRef);
        }

        /// <summary>
        /// Private helper method for <b>Removing</b> the current <b>{Available Collided Hitbox}</b>.
        /// </summary>
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playerhandle-collided-hitbox) for more info.
        private void UnsetHitbox()
        {
            CollidedHitboxEntityRef = EntityRef.None;
            CollidedHitboxComponent = null;
            CollidedHitboxTransform = null;
        }

        //! @name BattleManagedEntityHandle.IComposite Implementation
        //!
        //! See @cref{Battle.QSimulation.Game,BattleManagedEntityHandle.IComposite} for more info.
        //!
        //! @{

        /// @private
        /// <summary>
        /// Used internally by <see cref="BattleManagedEntityHandle"/><b>.To</b> method in order to upgrade
        /// the <see cref="BattleManagedEntityHandle"/> to a <see cref="BattlePlayerHandle"/>.
        /// </summary>
        ///
        /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="managedEntity">The managed entity handle being upgraded.</param>
        void BattleManagedEntityHandle.IComposite.FromManagedEntity(Frame f, BattleManagedEntityHandle managedEntity)
        {
            BattlePlayerLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerLinkQComponent>(managedEntity);

            PlayerData = BattlePlayerManager.GetPlayerData(f, link->Slot);
            SetCharacter(f, managedEntity);
        }

        //! @}
    }

    /// <summary>
    /// <b>{Managed Composite Entity Type Handle}</b> for handling <b>{Shield Entities}</b>.
    /// </summary>
    ///
    /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
    /// See [{PlayerShieldHandle}](#page-concepts-player-simulation-management-playershieldhandle) for more info.<br/>
    /// See [{Shield Entity}](#page-concepts-player-character-and-shield-entity) for more info.
    public unsafe struct BattlePlayerShieldHandle : BattleManagedEntityHandle.IComposite
    {
        /// <summary>
        /// <b>Creates</b> a <b>{PlayerShieldHandle}</b> for a shield based on <b>{Linked/Associated}</b> <paramref name="entityRef"/>
        /// </summary>
        ///
        /// See [{PlayerShieldHandle}](#page-concepts-player-simulation-management-playershieldhandle) for more info.
        /// See [{Linked/Associated Entity}](#page-concepts-player-simulation-management-playershieldhandle-linked-associated-entities) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityRef">EntityRef of the shield the handle is being created for.</param>
        ///
        /// <returns>The created handle.</returns>
        public static BattlePlayerShieldHandle Create(Frame f, EntityRef entityRef)
        {
            BattlePlayerShieldHandle handle = new();

            BattlePlayerShieldLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerShieldLinkQComponent>(entityRef);

            BattleManagedEntityHandle managedEntity = BattleManagedEntityHandle.Create(f, link->ERef);

            handle.SetShield(f, managedEntity);

            if (link->Type == BattlePlayerShieldLinkEntityType.Hitbox) handle.SetHitbox(f, entityRef);

            return handle;
        }

        /// <summary>
        /// Creates a <b>{Link/Association}</b> between <paramref name="shieldEntityRef"/>
        /// and <paramref name="linkEntityRef"/> of <paramref name="type"/>.
        /// </summary>
        ///
        /// See [{Linked/Associated Entity}](#page-concepts-player-simulation-management-playershieldhandle-linked-associated-entities) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="shieldEntityRef">Shield EntityRef <b>{Associated}</b> with <paramref name="linkEntityRef"/></param>
        /// <param name="type">The type of entity this <b>{Association}</b> is being created for.</param>
        /// <param name="linkEntityRef">EntityRef of the entity this <b>{Link/Association}</b> is being created for.</param>
        public static void CreateLink(Frame f, EntityRef shieldEntityRef, BattlePlayerShieldLinkEntityType type, EntityRef linkEntityRef)
        {
            BattlePlayerShieldLinkQComponent shieldLink = new()
            {
                ERef = shieldEntityRef,
                Type = type,
            };

            f.Add(linkEntityRef, shieldLink);
        }

        //! @anchor BattlePlayerShieldHandle-ShieldProperties
        //! @name Shield Properties
        //!
        //! Data related to the [{Shield Entity}](#page-concepts-player-character-and-shield-entity).
        //!
        //! @{

        /// <summary>
        /// <see cref="BattleManagedEntityHandle"/> of the shield.
        /// </summary>
        public BattleManagedEntityHandle ShieldEntityHandle {  get; private set; }

        /// <summary>
        /// Pointer to the <see cref="BattlePlayerShieldDataQComponent"/> of the shield.
        /// </summary>
        public BattlePlayerShieldDataQComponent* ShieldData { get; private set; }

        /// <summary>
        /// Pointer to the <b>Transform2D</b> of the shield when their <b>{Registered Entity}</b> is <b>{InPlay}</b>.
        /// @note <b>{Protected}</b> from possible unauthorized modification when the shield's <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.
        /// </summary>
        ///
        /// Use @cref{ShieldTransformCopy} to access the transform when the shield's <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        public Transform2D* ShieldTransformPtr
        {
            get
            {
                ShieldEntityHandle.DevAssertIsInPlay(nameof(BattlePlayerShieldHandle), "Improper transform access: possible unauthorized modification");
                return _shieldTransformInternalPtr;
            }
        }

        /// <summary>
        /// Access to a copy of the <b>{Protected}</b> <b>Transform2D</b> of the shield.<br/>
        /// The <b>Transform2D</b> is coped to ensure immutability when their <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.
        /// </summary>
        ///
        /// Use @cref{ShieldTransformPtr} to modify the transform when the shield's <b>{Registered Entity}</b> is <b>{InPlay}</b>.
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{InPlay/OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        public Transform2D ShieldTransformCopy => *_shieldTransformInternalPtr;

        //! @}

        //! @anchor BattlePlayerShieldHandle-CollidedHitboxProperties
        //! @name Collided Hitbox Properties
        //!
        //! Data related to the [{Collided Hitbox}](#page-concepts-player-simulation-management-playershieldhandle-available-collided-hitbox),
        //! which may or may not be [{Available}](#page-concepts-player-simulation-management-playershieldhandle-available-collided-hitbox).
        //!
        //! @{

        /// <summary>
        /// Whether this handle has a <see cref="CollidedHitboxEntityRef"/> <b>{Available}</b> or not.
        /// </summary>
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playershieldhandle-available-collided-hitbox) for more info.
        public readonly bool HandleHasCollidedHitbox => CollidedHitboxEntityRef != EntityRef.None;

        /// <summary>
        /// The EntityRef of the collided hitbox, if <b>{Available}</b>.
        /// </summary>
        ///
        /// Use @cref{HandleHasCollidedHitbox} to check if collided hitbox is <b>{Available}</b>.
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playershieldhandle-available-collided-hitbox) for more info.
        public EntityRef CollidedHitboxEntityRef { get; private set; }

        /// <summary>
        /// Pointer to the <see cref="BattlePlayerHitboxQComponent"/> of the collided hitbox, if <b>{Available}</b>.
        /// </summary>
        ///
        /// Use @cref{HandleHasCollidedHitbox} to check if collided hitbox is <b>{Available}</b>.
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playershieldhandle-available-collided-hitbox) for more info.
        public BattlePlayerHitboxQComponent* CollidedHitboxComponent { get; private set; }

        /// <summary>
        /// Pointer to the <b>Transform2D</b> of the collided hitbox, if <b>{Available}</b>.
        /// </summary>
        ///
        /// Use @cref{HandleHasCollidedHitbox} to check if collided hitbox is <b>{Available}</b>.
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playershieldhandle-available-collided-hitbox) for more info.
        public Transform2D* CollidedHitboxTransform { get; private set; }

        //! @}

        /// <summary>
        /// Private internal storage for the <b>{Protected}</b> <b>Transform2D</b> pointer of the shield.
        /// </summary>
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
        private Transform2D* _shieldTransformInternalPtr { get; set; }

        /// <summary>
        /// Private helper method that handles loading a <b>{Shield}</b> <paramref name="entityHandle"/>.
        /// </summary>
        ///
        /// See [{Shield Entity}](#page-concepts-player-character-and-shield-entity) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityHandle"><b>{Shield Entity}</b> to be loaded.</param>
        private void SetShield(Frame f, BattleManagedEntityHandle entityHandle)
        {
            ShieldEntityHandle          = entityHandle;
            ShieldData                  = f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(entityHandle);
            _shieldTransformInternalPtr = f.Unsafe.GetPointer<Transform2D>(entityHandle);
        }

        /// <summary>
        /// Private helper method for capturing a <b>Hitbox</b> <paramref name="entityRef"/>
        /// and making it <b>{Available}</b>.
        /// </summary>
        ///
        /// See [{Available Collided Hitbox}](#page-concepts-player-simulation-management-playershieldhandle-available-collided-hitbox) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityRef">Hitbox to capture and make <b>{Available}</b>.</param>
        private void SetHitbox(Frame f, EntityRef entityRef)
        {
            CollidedHitboxEntityRef = entityRef;
            CollidedHitboxComponent = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(entityRef);
            CollidedHitboxTransform = f.Unsafe.GetPointer<Transform2D>(entityRef);
        }

        //! @name BattleManagedEntityHandle.IComposite Implementation
        //!
        //! See @cref{Battle.QSimulation.Game,BattleManagedEntityHandle.IComposite} for more info.
        //!
        //! @{

        /// @private
        /// <summary>
        /// Used internally by <see cref="BattleManagedEntityHandle"/><b>.To</b> method in order to upgrade
        /// the <see cref="BattleManagedEntityHandle"/> to a <see cref="BattlePlayerShieldHandle"/>.
        /// </summary>
        ///
        /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="managedEntity">The managed entity handle being upgraded.</param>
        void BattleManagedEntityHandle.IComposite.FromManagedEntity(Frame f, BattleManagedEntityHandle managedEntity)
        {
            SetShield(f, managedEntity);
        }

        //! @}
    }

    /// <summary>
    /// <b>{Managed Composite Entity Type Handle}</b> for handling projectiles.
    /// </summary>
    ///
    /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
    public unsafe struct BattleProjectileHandle : BattleManagedEntityHandle.IComposite
    {
        /// <summary>
        /// Creates a <see cref="BattleProjectileHandle"/> for a projectile based on {Linked/Associated} <paramref name="entityRef"/>
        /// </summary>
        ///
        /// See [{Linked/Associated Entity}](#page-concepts-player-simulation-management-playerhandle-linked-entities) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="entityRef">EntityRef of the projectile the handle is being created for.</param>
        ///
        /// <returns>The created handle.</returns>
        public static BattleProjectileHandle Create(Frame f, EntityRef entityRef)
        {
            BattleProjectileHandle handle = new();

            BattleProjectileLinkQComponent* link = f.Unsafe.GetPointer<BattleProjectileLinkQComponent>(entityRef);

            handle.EntityHandle          = BattleManagedEntityHandle.Create(f, link->ERef);
            handle.Data                  = f.Unsafe.GetPointer<BattleProjectileQComponent>(link->ERef);
            handle._transformInternalPtr = f.Unsafe.GetPointer<Transform2D>(link->ERef);

            return handle;
        }

        /// <summary>
        /// Creates a <see cref="BattleProjectileHandle"/> for a projectile based on <paramref name="filter"/>
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="filter">Filter containing the projectile's data.</param>
        ///
        /// <returns>The created handle.</returns>
        public static BattleProjectileHandle Create(Frame f, BattleProjectileQSystem.Filter filter)
        {
            BattleProjectileHandle handle = new();

            handle.EntityHandle          = BattleManagedEntityHandle.Create(f, filter.Entity);
            handle.Data                  = filter.Projectile;
            handle._transformInternalPtr = filter.Transform;

            return handle;
        }

        /// <summary>
        /// Creates a <b>{Link/Association}</b> between <paramref name="projectileEntityRef"/>
        /// and <paramref name="linkEntityRef"/>.
        /// </summary>
        ///
        /// See [{Linked/Associated Entity}](#page-concepts-player-simulation-management-playerhandle-linked-entities) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectileEntityRef">Shield EntityRef <b>{Associated}</b> with <paramref name="linkEntityRef"/></param>
        /// <param name="linkEntityRef">EntityRef of the entity this <b>{Link/Association}</b> is being created for.</param>
        public static void CreateLink(Frame f, EntityRef projectileEntityRef, EntityRef linkEntityRef)
        {
            BattleProjectileLinkQComponent projectileLink = new()
            {
                ERef = projectileEntityRef
            };

            f.Add(linkEntityRef, projectileLink);
        }

        /// <summary>
        /// <see cref="BattleManagedEntityHandle"/> of the projectile.
        /// </summary>
        public BattleManagedEntityHandle EntityHandle { get; private set; }

        /// <summary>
        /// Pointer to the <see cref="BattleProjectileQComponent"/> of the projectile.
        /// </summary>
        public BattleProjectileQComponent* Data {  get; private set; }

        /// <summary>
        /// Pointer to the <b>Transform2D</b> of the projectile when their <b>{Registered Entity}</b> is <b>{InPlay}</b>.
        /// @note <b>{Protected}</b> from possible unauthorized modification when the projectile's <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.
        /// </summary>
        ///
        /// Use @cref{TransformCopy} to access the transform when the projectile's <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        public Transform2D* TransformPtr
        {
            get
            {
                EntityHandle.DevAssertIsInPlay(nameof(BattleProjectileHandle), "Improper transform access: possible unauthorized modification");
                return _transformInternalPtr;
            }
        }

        /// <summary>
        /// Access to a copy of the <b>{Protected}</b> <b>Transform2D</b> of the projectile.<br/>
        /// The <b>Transform2D</b> is coped to ensure immutability when their <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.
        /// </summary>
        ///
        /// Use @cref{TransformPtr} to modify the transform when the projectile's <b>{Registered Entity}</b> is <b>{InPlay}</b>.
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{InPlay/OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        public Transform2D TransformCopy => *_transformInternalPtr;

        /// <summary>
        /// Sets a specific collision flag for the current frame.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="flag">Collision flag to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCollisionFlag(Frame f, BattleProjectileCollisionFlags flag)
        {
            BattleProjectileCollisionFlags flags = Data->CollisionFlags[f.Number % 2];
            Data->CollisionFlags[f.Number % 2] = flags.SetFlag(flag);
        }

        /// <summary>
        /// Sets whether the projectile is currently held.
        /// </summary>
        ///
        /// <param name="isHeld">True/False : held / not held.</param>
        public void SetHeld(bool isHeld)
        {
            Data->IsHeld = isHeld;
        }

        /// <summary>
        /// Sets the emotion state of the projectile and triggers the corresponding event.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="emotion">The new emotion state to assign to the projectile.</param>
        public void SetEmotion(Frame f, BattleEmotionState emotion)
        {
            if (emotion != BattleEmotionState.Love)
            {
                Data->EmotionBase = emotion;
            }
            Data->EmotionCurrent = emotion;
            f.Events.BattleChangeEmotionState(Data->EmotionCurrent);
        }

        /// <summary>
        /// Sets the attack value of the projectile and updates its glow strength.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="attack">The new attack value to assign to the projectile.</param>
        public void SetAttack(Frame f, FP attack)
        {
            Data->Attack = attack;
            f.Events.BattleProjectileChangeGlowStrength(Data->Attack / Data->AttackMax);
        }

        /// <summary>
        /// Private internal storage for the <b>{Protected}</b> <b>Transform2D</b> pointer of the projectile.
        /// </summary>
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
        private Transform2D* _transformInternalPtr { get; set; }

        //! @name BattleManagedEntityHandle.IComposite Implementation
        //!
        //! See @cref{Battle.QSimulation.Game,BattleManagedEntityHandle.IComposite} for more info.
        //!
        //! @{

        /// @private
        /// <summary>
        /// Used internally by <see cref="BattleManagedEntityHandle"/><b>.To</b> method in order to upgrade
        /// the <see cref="BattleManagedEntityHandle"/> to a <see cref="BattleProjectileHandle"/>.
        /// </summary>
        ///
        /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="managedEntity">The managed entity handle being upgraded.</param>
        void BattleManagedEntityHandle.IComposite.FromManagedEntity(Frame f, BattleManagedEntityHandle managedEntity)
        {
            EntityHandle          = managedEntity;
            Data                  = f.Unsafe.GetPointer<BattleProjectileQComponent>(managedEntity);
            _transformInternalPtr = f.Unsafe.GetPointer<Transform2D>(managedEntity);
        }

        //! @}
    }
}
