
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
    /// <b>{Managed Composite Entity Type Handle}</b> for handling players.
    /// </summary>
    ///
    /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
    public unsafe struct BattlePlayerHandle : BattleManagedEntityHandle.IComposite
    {
        /// <summary>
        /// <b>Creates</b> a <b>{PlayerHandle}</b> for a <b>%Player</b> based on <paramref name="slot"/>.
        /// </summary>
        ///
        /// You can use @cref{Battle.QSimulation.Game.BattlePlayerHandle,Create(BattlePlayerData.Ref)}
        /// if you already have a @cref{Quantum,BattlePlayerData.Ref}.
        ///
        /// Use @cref{Battle.QSimulation.Game.BattlePlayerHandle,Create(Frame\, EntityRef)}
        /// to <b>Create</b> a <b>Handle</b> for the <b>%Player</b> and a <b>Character</b>.
        ///
        /// See [{PlayerHandle}](#page-concepts-player-simulation-management-playerhandle) for more info.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="slot">Slot of the player the handle is being created for.</param>
        ///
        /// <returns>The created handle.</returns>
        public static BattlePlayerHandle Create(Frame f, BattlePlayerSlot slot)
        {
            BattlePlayerHandle handle = new();
            handle.PlayerData = BattlePlayerManager.GetPlayerData(f, slot);
            return handle;
        }

        /// <summary>
        /// <b>Creates</b> a <b>{PlayerHandle}</b> for a <b>%Player</b> based on <paramref name="playerData"/>.
        /// </summary>
        ///
        /// You can use @cref{Battle.QSimulation.Game.BattlePlayerHandle,Create(Frame\, BattlePlayerSlot)}
        /// if you don't have a @cref{Quantum,BattlePlayerData.Ref}.
        ///
        /// Use @cref{Battle.QSimulation.Game.BattlePlayerHandle,Create(Frame\, EntityRef)}
        /// to <b>Create</b> a <b>Handle</b> for the <b>%Player</b> and a <b>Character</b>.
        ///
        /// See [{PlayerHandle}](#page-concepts-player-simulation-management-playerhandle) for more info.
        ///
        /// <param name="playerData">Data of the player the handle is being created for.</param>
        ///
        /// <returns>The created handle.</returns>
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
        /// <param name="entityRef">EntityRef of the player the handle is being created for.</param>
        ///
        /// <returns>The created handle.</returns>
        public static BattlePlayerHandle Create(Frame f, EntityRef entityRef)
        {
            BattlePlayerLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerLinkQComponent>(entityRef);

            BattlePlayerHandle handle = new();

            handle.PlayerData = BattlePlayerManager.GetPlayerData(f, link->Slot);
            handle.SetCharacter(f, BattleManagedEntityHandle.Create(f, link->CharacterEntityRef));

            if (link->Type == BattlePlayerLinkEntityType.Hitbox) handle.SetHitbox(f, entityRef);

            return handle;
        }

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

        /// <summary>
        /// Whether player has <b>{Selected a Character}</b> or not.
        /// </summary>
        ///
        /// See [{Selected Character}](#page-concepts-player-character-entity-selected-character) for more info.
        public readonly bool PlayerHasSelectedCharacter => PlayerData.SelectedCharacterNumber != BattlePlayerData.NoSelectedCharacter;

        /// <summary>
        /// Whether this handle has a loaded character or not.
        /// </summary>
        public readonly bool HandleHasCharacterLoaded => LoadedCharacterEntityHandle.ERef != EntityRef.None;

        /// <summary>
        /// Whether this handle's <b>{Character is Selected}</b> or not.
        /// </summary>
        ///
        /// See [{Selected Character}](#page-concepts-player-character-entity-selected-character) for more info.
        public readonly bool LoadedCharacterIsSelected => LoadedCharacterData->Number == PlayerData.SelectedCharacterNumber;

        /// <summary>
        /// Whether this handle's loaded character has an attached shield or not.
        /// </summary>
        public readonly bool LoadedCharacterHasShieldAttached => LoadedCharacterData->AttachedShieldEntityRef != EntityRef.None;

        /// <summary>
        /// Whether this handle has a <see cref="CollidedHitboxEntityRef"/> available or not.
        /// </summary>
        public readonly bool HandleHasCollidedHitbox => CollidedHitboxEntityRef != EntityRef.None;

        /// <summary>
        /// <see cref="BattlePlayerData.Ref"/> of this handle's player.
        /// </summary>
        public BattlePlayerData.Ref PlayerData { get; private set; }

        /// <summary>
        /// <see cref="BattleManagedEntityHandle"/> of this player's loaded character, if any are loaded.
        /// </summary>
        ///
        /// Use @cref{HandleHasCharacterLoaded} to check if the handle has a loaded character.
        public BattleManagedEntityHandle LoadedCharacterEntityHandle { get; private set; }

        /// <summary>
        /// Pointer to the <see cref="BattlePlayerCharacterDataQComponent"/> of this player's character, if any are loaded.
        /// </summary>
        ///
        /// Use @cref{HandleHasCharacterLoaded} to check if the handle has a loaded character.
        public BattlePlayerCharacterDataQComponent* LoadedCharacterData { get; private set; }

        /// <summary>
        /// Pointer to the <see cref="Transform2D"/> of this player's character when their <b>{Registered Entity}</b> is <b>{InPlay}</b>, if any character is loaded.
        /// @note <b>{Protected}</b> from possible unauthorized modification when the player's character's <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.
        /// </summary>
        ///
        /// Use @cref{LoadedCharacterTransformCopy} to access the transform when the player's character's <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.<br/>
        /// Use @cref{HandleHasCharacterLoaded} to check if the handle has a loaded character.<br/>
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
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
        /// Access to a copy of the <b>{Protected}</b> <see cref="Transform2D"/> of this player's character,
        /// ensuring immutability when their <b>{Registered Entity}</b> is <b>{OutOfPlay}</b>.
        /// </summary>
        ///
        /// Use @cref{LoadedCharacterTransformPtr} to modify the transform when the player's character's <b>{Registered Entity}</b> is <b>{InPlay}</b>.<br/>
        /// Use @cref{HandleHasCharacterLoaded} to check if the handle has a loaded character.<br/>
        ///
        /// See [{Protected Transform Component}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.<br/>
        /// See [{Registered Entities}](#page-concepts-entity-management-registered-entities) for more info.<br/>
        /// See [{InPlay/OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate) for more info.
        public Transform2D LoadedCharacterTransformCopy => *_loadedCharacterTransformInternalPtr;

        /// <summary>
        /// The EntityRef of the collided hitbox, if available.
        /// </summary>
        ///
        /// Use @cref{HandleHasCollidedHitbox} to check if collided hitbox is available.
        public EntityRef CollidedHitboxEntityRef { get; private set; }

        /// <summary>
        /// Pointer to the <see cref="BattlePlayerHitboxQComponent"/> of the collided hitbox, if available.
        /// </summary>
        ///
        /// Use @cref{HandleHasCollidedHitbox} to check if collided hitbox is available.
        public BattlePlayerHitboxQComponent* CollidedHitboxComponent { get; private set; }

        /// <summary>
        /// Pointer to the <see cref="Transform2D"/> of the collided hitbox, if available.
        /// </summary>
        ///
        /// Use @cref{HandleHasCollidedHitbox} to check if collided hitbox is available.
        public Transform2D* CollidedHitboxTransform { get; private set; }

        //! @name Loaded Character Methods
        //! Methods for [{Loading Characters}]( #page-concepts-player-simulation-management-playerhandle-loaded-character)
        //! and other related methods.
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

        private Transform2D* _loadedCharacterTransformInternalPtr {  get; set; }

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
    }

    /// <summary>
    /// <b>{Managed Composite Entity Type Handle}</b> for handling shields.
    /// </summary>
    ///
    /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
    public unsafe struct BattlePlayerShieldHandle : BattleManagedEntityHandle.IComposite
    {
        public static BattlePlayerShieldHandle Create(Frame f, EntityRef entity)
        {
            BattlePlayerShieldHandle handle = new();

            BattlePlayerShieldLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerShieldLinkQComponent>(entity);

            BattleManagedEntityHandle managedEntity = BattleManagedEntityHandle.Create(f, link->ERef);

            handle.SetShield(f, managedEntity);

            if (link->Type == BattlePlayerShieldLinkEntityType.Hitbox) handle.SetHitbox(f, entity);

            return handle;
        }

        public static void CreateLink(Frame f, EntityRef shieldEntityRef, BattlePlayerShieldLinkEntityType type, EntityRef linkEntityRef)
        {
            BattlePlayerShieldLinkQComponent shieldLink = new()
            {
                ERef = shieldEntityRef,
                Type = type,
            };

            f.Add(linkEntityRef, shieldLink);
        }

        public readonly bool HandleHasCollidedHitbox => CollidedHitboxEntityRef != EntityRef.None;

        public BattleManagedEntityHandle ShieldEntityHandle {  get; private set; }

        public BattlePlayerShieldDataQComponent* ShieldData { get; private set; }

        public Transform2D* ShieldTransformPtr
        {
            get
            {
                ShieldEntityHandle.DevAssertIsInPlay(nameof(BattlePlayerShieldHandle), "Improper transform access: possible unauthorized modification");
                return _shieldTransformInternalPtr;
            }
        }

        public Transform2D ShieldTransformCopy => *_shieldTransformInternalPtr;

        public EntityRef CollidedHitboxEntityRef { get; private set; }

        public BattlePlayerHitboxQComponent* CollidedHitboxComponent { get; private set; }

        public Transform2D* CollidedHitboxTransform { get; private set; }

        private Transform2D* _shieldTransformInternalPtr { get; set; }

        private void SetShield(Frame f, BattleManagedEntityHandle managedEntity)
        {
            ShieldEntityHandle          = managedEntity;
            ShieldData                  = f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(managedEntity);
            _shieldTransformInternalPtr = f.Unsafe.GetPointer<Transform2D>(managedEntity);
        }

        private void SetHitbox(Frame f, EntityRef entity)
        {
            CollidedHitboxEntityRef = entity;
            CollidedHitboxComponent = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(entity);
            CollidedHitboxTransform = f.Unsafe.GetPointer<Transform2D>(entity);
        }

        void BattleManagedEntityHandle.IComposite.FromManagedEntity(Frame f, BattleManagedEntityHandle managedEntity)
        {
            SetShield(f, managedEntity);
        }
    }

    /// <summary>
    /// <b>{Managed Composite Entity Type Handle}</b> for handling projectiles.
    /// </summary>
    ///
    /// See [{Managed Composite Type Handle}](#page-concepts-battle-handle-managed-composite-type-handle) for more info.
    public unsafe struct BattleProjectileHandle : BattleManagedEntityHandle.IComposite
    {
        public static BattleProjectileHandle Create(Frame f, EntityRef entity)
        {
            BattleProjectileHandle handle = new();

            BattleProjectileLinkQComponent* link = f.Unsafe.GetPointer<BattleProjectileLinkQComponent>(entity);

            handle.EntityHandle          = BattleManagedEntityHandle.Create(f, link->ERef);
            handle.Data                  = f.Unsafe.GetPointer<BattleProjectileQComponent>(link->ERef);
            handle._transformInternalPtr = f.Unsafe.GetPointer<Transform2D>(link->ERef);

            return handle;
        }

        public static BattleProjectileHandle Create(Frame f, BattleProjectileQSystem.Filter filter)
        {
            BattleProjectileHandle handle = new();

            handle.EntityHandle          = BattleManagedEntityHandle.Create(f, filter.Entity);
            handle.Data                  = filter.Projectile;
            handle._transformInternalPtr = filter.Transform;

            return handle;
        }

        public static void CreateLink(Frame f, EntityRef projectileEntity, EntityRef linkEntityRef)
        {
            BattleProjectileLinkQComponent projectileLink = new()
            {
                ERef = projectileEntity
            };

            f.Add(linkEntityRef, projectileLink);
        }

        public BattleManagedEntityHandle EntityHandle { get; private set; }

        public BattleProjectileQComponent* Data {  get; private set; }

        public Transform2D* TransformPtr
        {
            get
            {
                EntityHandle.DevAssertIsInPlay(nameof(BattleProjectileHandle), "Improper transform access: possible unauthorized modification");
                return _transformInternalPtr;
            }
        }

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

        private Transform2D* _transformInternalPtr { get; set; }

        void BattleManagedEntityHandle.IComposite.FromManagedEntity(Frame f, BattleManagedEntityHandle managedEntity)
        {
            EntityHandle          = managedEntity;
            Data                  = f.Unsafe.GetPointer<BattleProjectileQComponent>(managedEntity);
            _transformInternalPtr = f.Unsafe.GetPointer<Transform2D>(managedEntity);
        }
    }
}
