using System.Runtime.CompilerServices;
using Battle.QSimulation.Player;
using Battle.QSimulation.Projectile;
using Photon.Deterministic;
using Quantum;
using Quantum.Collections;

namespace Battle.QSimulation.Game
{
    public unsafe struct BattleManagedEntityHandle
    {
        public interface IComposite
        {
            public void FromManagedEntity(Frame f, BattleManagedEntityHandle managedEntity);
        }

        public static BattleManagedEntityHandle Create(Frame f, EntityRef entity)
        {
            BattleManagedEntityHandle handle = new()
            {
                ERef = entity,
                _component = f.Unsafe.GetPointer<BattleManagedEntityQComponent>(entity)
            };

            return handle;
        }

        public static implicit operator EntityRef(BattleManagedEntityHandle managedEntityRef) => managedEntityRef.ERef;

        public EntityRef ERef { get; private set; }

        public bool IsRegistered => _component->IsRegistered;

        public BattleEntityPlayState RegisteredState { get
        {
            DevAssertIsRegistered("Can not read the play state");
            return _component->RegisteredState;
        }}

        public bool IsCompound => _component->IsCompound;

        public QList<BattleEntityLink> CompoundGetLinkedEntities(Frame f)
        {
            DevAssertIsCompound("Can not " + nameof(CompoundGetLinkedEntities));
            return f.ResolveList(_component->CompoundLinkedEntities);
        }

        public T To<T>(Frame f) where T : IComposite, new()
        {
            T composite = new();
            composite.FromManagedEntity(f, this);
            return composite;
        }

        public void DevAssertIsRegistered(string context) => DevAssertIsRegistered(nameof(BattleManagedEntityHandle), context);

        public void DevAssertIsRegistered(string source, string context)
        {
            BattleDebugLogger.DevAssertFormat(source, _component->IsRegistered, "{0}, entity is not registered", context);
        }

        public void DevAssertIsInPlay(string context) => DevAssertIsInPlay(nameof(BattleManagedEntityHandle), context);

        public void DevAssertIsInPlay(string source, string context)
        {
            BattleDebugLogger.DevAssertFormat(source, _component->RegisteredState == BattleEntityPlayState.InPlay, "{0}, entity is not in play", context);
        }

        public void DevAssertIsCompound(string context) => DevAssertIsCompound(nameof(BattleManagedEntityHandle), context);

        public void DevAssertIsCompound(string source, string context)
        {
            BattleDebugLogger.DevAssertFormat(source, _component->IsCompound, "{0}, entity is not a compound", context);
        }

        private BattleManagedEntityQComponent* _component;
    }

    public unsafe struct BattlePlayerHandle : BattleManagedEntityHandle.IComposite
    {
        public static BattlePlayerHandle Create(Frame f, BattlePlayerSlot slot)
        {
            BattlePlayerHandle handle = new();
            handle.PlayerData = BattlePlayerManager.GetPlayerData(f, slot);
            return handle;
        }

        public static BattlePlayerHandle Create(BattlePlayerData.Ref playerData)
        {
            BattlePlayerHandle handle = new();
            handle.PlayerData = playerData;
            return handle;
        }

        public static BattlePlayerHandle Create(Frame f, EntityRef entityRef)
        {
            BattlePlayerLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerLinkQComponent>(entityRef);

            BattlePlayerHandle handle = new();

            handle.PlayerData = BattlePlayerManager.GetPlayerData(f, link->Slot);
            handle.SetCharacter(f, link->CharacterEntityRef);

            if (link->Type == BattlePlayerLinkEntityType.Hitbox) handle.SetHitbox(f, entityRef);

            return handle;
        }

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

        public readonly bool PlayerHasSelectedCharacter => PlayerData.SelectedCharacterNumber != BattlePlayerData.NoSelectedCharacter;

        public readonly bool HandleHasCharacterLoaded => LoadedCharacterEntityHandle.ERef != EntityRef.None;

        public readonly bool LoadedCharacterIsSelected => LoadedCharacterData->Number == PlayerData.SelectedCharacterNumber;

        public readonly bool LoadedCharacterHasShieldAttached => LoadedCharacterData->AttachedShieldEntityRef != EntityRef.None;

        public readonly bool HandleHasCollidedHitbox => CollidedHitboxEntityRef != EntityRef.None;

        public BattlePlayerData.Ref PlayerData { get; private set; }

        public BattleManagedEntityHandle LoadedCharacterEntityHandle { get; private set; }

        public BattlePlayerCharacterDataQComponent* LoadedCharacterData { get; private set; }

        public Transform2D* LoadedCharacterTransformPtr
        {
            get
            {
                LoadedCharacterEntityHandle.DevAssertIsInPlay(nameof(BattlePlayerHandle), "Improper transform access: possible unauthorized modification");
                return _loadedCharacterTransformInternalPtr;
            }
        }

        public Transform2D LoadedCharacterTransformCopy => *_loadedCharacterTransformInternalPtr;

        public EntityRef CollidedHitboxEntityRef { get; private set; }

        public BattlePlayerHitboxQComponent* CollidedHitboxComponent { get; private set; }

        public Transform2D* CollidedHitboxTransform { get; private set; }

        public void LoadCharacter(Frame f, int characterNumber)
        {
            BattlePlayerData.DevAssertIsValidCharacterNumber(nameof(BattlePlayerHandle), characterNumber);
            SetCharacter(f, BattleEntityManager.Get(f, PlayerData.Low_Level.CharacterEntityGroupID, characterNumber));
        }

        public void LoadCharacter(Frame f, EntityRef entity)
        {
            BattlePlayerLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerLinkQComponent>(entity);

            SetCharacter(f, link->CharacterEntityRef);

            if (link->Type == BattlePlayerLinkEntityType.Hitbox)
            {
                SetHitbox(f, entity);
            }
            else
            {
                UnsetHitbox();
            }
        }

        public BattlePlayerHandle LoadCharacterCopy(Frame f, int characterNumber)
        {
            BattlePlayerData.DevAssertIsValidCharacterNumber(nameof(BattlePlayerHandle), characterNumber);
            BattlePlayerHandle copy = Create(PlayerData);
            copy.LoadCharacter(f, characterNumber);
            return copy;
        }

        public BattlePlayerShieldHandle GetLoadedCharacterAttachedShield(Frame f)
        {
            return BattlePlayerShieldHandle.Create(f, LoadedCharacterData->AttachedShieldEntityRef);
        }

        public void DevAssertPlayerHandleHasCharacterLoaded(string source, string errorContext)
        {
            BattleDebugLogger.DevAssertFormat(source, HandleHasCharacterLoaded, "{0}, PlayerHandle has no loaded character", errorContext);
        }

        private Transform2D* _loadedCharacterTransformInternalPtr {  get; set; }

        private void SetCharacter(Frame f, EntityRef entity)
        {
            LoadedCharacterEntityHandle          = BattleManagedEntityHandle.Create(f, entity);
            LoadedCharacterData                  = f.Unsafe.GetPointer<BattlePlayerCharacterDataQComponent>(entity);
            _loadedCharacterTransformInternalPtr = f.Unsafe.GetPointer<Transform2D>(entity);
        }

        private void SetHitbox(Frame f, EntityRef entity)
        {
            CollidedHitboxEntityRef = entity;
            CollidedHitboxComponent = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(entity);
            CollidedHitboxTransform = f.Unsafe.GetPointer<Transform2D>(entity);
        }

        private void UnsetHitbox()
        {
            CollidedHitboxEntityRef = EntityRef.None;
            CollidedHitboxComponent = null;
            CollidedHitboxTransform = null;
        }

        void BattleManagedEntityHandle.IComposite.FromManagedEntity(Frame f, BattleManagedEntityHandle managedEntity)
        {
            SetCharacter(f, managedEntity);
        }
    }

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
