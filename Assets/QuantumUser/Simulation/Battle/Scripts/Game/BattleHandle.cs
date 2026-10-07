using System.Runtime.CompilerServices;
using Battle.QSimulation.Player;
using Battle.QSimulation.Projectile;
using Photon.Deterministic;
using Quantum;

namespace Battle.QSimulation.Game
{
    public struct BattleManageEntityHandle
    {
        public EntityRef ERef;

        public static implicit operator EntityRef(BattleManageEntityHandle manageEntityRef) => manageEntityRef.ERef;
    }

    public unsafe struct BattlePlayerHandle
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

            if (link->Type == PlayerEntityType.Hitbox) handle.SetHitbox(f, entityRef);

            return handle;
        }

        public static void CreateLink(Frame f, BattlePlayerSlot slot, EntityRef characterEntityRef, PlayerEntityType type, EntityRef entity)
        {
            BattlePlayerLinkQComponent playerLink = new()
            {
                CharacterEntityRef = characterEntityRef,
                Slot = slot,
                Type = type
            };

            f.Add(entity, playerLink);
        }

        public readonly bool PlayerHasSelectedCharacter => PlayerData.SelectedCharacterNumber != BattlePlayerData.NoSelectedCharacter;

        public readonly bool HandleHasCharacterLoaded => LoadedCharacterEntityRef != EntityRef.None;

        public readonly bool LoadedCharacterIsSelected => LoadedCharacterData->Number == PlayerData.SelectedCharacterNumber;

        public readonly bool LoadedCharacterHasShieldAttached => LoadedCharacterData->AttachedShieldEntityRef != EntityRef.None;

        public readonly bool HandleHasCollidedHitbox => CollidedHitboxEntityRef != EntityRef.None;

        public BattlePlayerData.Ref PlayerData { get; private set; }

        public EntityRef LoadedCharacterEntityRef { get; private set; }

        public BattlePlayerCharacterDataQComponent* LoadedCharacterData { get; private set; }

        public Transform2D* LoadedCharacterTransform { get; private set; }

        public EntityRef CollidedHitboxEntityRef { get; private set; }

        public BattlePlayerHitboxQComponent* CollidedHitboxComponent { get; private set; }

        public Transform2D* CollidedHitboxTransform { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidCharacterNumber(int characterNumber)
        {
            return BattlePlayerData.IsValidCharacterNumber(characterNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidShieldNumber(int characterNumber, int shieldNumber)
        {
            BattlePlayerData.DevAssertIsValidCharacterNumber(nameof(BattlePlayerHandle), characterNumber);
            return shieldNumber >= 0 && LoadedCharacterData->ShieldCount < shieldNumber;
        }

        public void LoadCharacter(Frame f, int characterNumber)
        {
            BattlePlayerData.DevAssertIsValidCharacterNumber(nameof(BattlePlayerHandle), characterNumber);
            SetCharacter(f, (BattlePlayerEntityRef)BattleEntityManager.Get(f, PlayerData.Low_Level.CharacterEntityGroupID, characterNumber));
        }

        public void LoadCharacter(Frame f, EntityRef entity)
        {
            BattlePlayerLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerLinkQComponent>(entity);

            SetCharacter(f, link->CharacterEntityRef);

            if (link->Type == PlayerEntityType.Hitbox)
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

        public BattlePlayerCharacterShieldHandle GetLoadedCharacterAttachedShield(Frame f)
        {
            return BattlePlayerCharacterShieldHandle.Create(f, LoadedCharacterData->AttachedShieldEntityRef);
        }

        private void SetCharacter(Frame f, EntityRef entity)
        {
            LoadedCharacterEntityRef  = entity;
            LoadedCharacterData = f.Unsafe.GetPointer<BattlePlayerCharacterDataQComponent>(entity);
            LoadedCharacterTransform  = f.Unsafe.GetPointer<Transform2D>(entity);
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
    }

    public unsafe struct BattlePlayerCharacterShieldHandle
    {
        public static BattlePlayerCharacterShieldHandle Create(Frame f, EntityRef entity)
        {
            BattlePlayerCharacterShieldHandle handle = new();

            BattlePlayerCharacterShieldLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerCharacterShieldLinkQComponent>(entity);

            handle.SetShield(f, link->ERef);

            if (link->Type == PlayerCharacterShieldEntityType.Hitbox) handle.SetHitbox(f, entity);

            return handle;
        }

        public static void CreateLink(Frame f, EntityRef shieldEntityRef, PlayerCharacterShieldEntityType type, EntityRef entity)
        {
            BattlePlayerCharacterShieldLinkQComponent shieldLink = new()
            {
                ERef = shieldEntityRef,
                Type = type,
            };

            f.Add(entity, shieldLink);
        }

        public readonly bool HandleHasCollidedHitbox => CollidedHitboxEntityRef != EntityRef.None;

        public EntityRef ShieldEntityRef {  get; private set; }

        public BattlePlayerShieldDataQComponent* ShieldData { get; private set; }

        public Transform2D* ShieldTransform { get; private set; }

        public EntityRef CollidedHitboxEntityRef { get; private set; }

        public BattlePlayerHitboxQComponent* CollidedHitboxComponent { get; private set; }

        public Transform2D* CollidedHitboxTransform { get; private set; }

        private void SetShield(Frame f, EntityRef entity)
        {
            ShieldEntityRef = entity;
            ShieldData      = f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(entity);
            ShieldTransform = f.Unsafe.GetPointer<Transform2D>(entity);
        }

        private void SetHitbox(Frame f, EntityRef entity)
        {
            CollidedHitboxEntityRef = entity;
            CollidedHitboxComponent = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(entity);
            CollidedHitboxTransform = f.Unsafe.GetPointer<Transform2D>(entity);
        }
    }

    public unsafe struct BattleProjectileHandle
    {
        public static BattleProjectileHandle Create(Frame f, EntityRef entity)
        {
            BattleProjectileHandle handle = new();

            BattleProjectileLinkQComponent* link = f.Unsafe.GetPointer<BattleProjectileLinkQComponent>(entity);

            handle.ERef      = link->ERef;
            handle.Data      = f.Unsafe.GetPointer<BattleProjectileQComponent>(link->ERef);
            handle.Transform = f.Unsafe.GetPointer<Transform2D>(link->ERef);

            return handle;
        }

        public static BattleProjectileHandle Create(BattleProjectileQSystem.Filter filter)
        {
            BattleProjectileHandle handle = new();

            handle.ERef      = filter.Entity;
            handle.Data      = filter.Projectile;
            handle.Transform = filter.Transform;

            return handle;
        }

        public static void CreateLink(Frame f, EntityRef projectileEntity, EntityRef entity)
        {
            BattleProjectileLinkQComponent projectileLink = new()
            {
                ERef = projectileEntity
            };

            f.Add(entity, projectileLink);
        }

        public EntityRef ERef { get; private set; }

        public BattleProjectileQComponent* Data {  get; private set; }

        public Transform2D* Transform { get; private set; }

        /// <summary>
        /// Sets a specific collision flag for the current frame.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="projectile">Pointer to the projectile component.</param>
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
        /// <param name="projectile">Pointer to the projectile component.</param>
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
        /// <param name="projectile">Pointer to the projectile component.</param>
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
        /// <param name="projectile">Pointer to the projectile component.</param>
        /// <param name="attack">The new attack value to assign to the projectile.</param>
        public void SetAttack(Frame f, FP attack)
        {
            Data->Attack = attack;
            f.Events.BattleProjectileChangeGlowStrength(Data->Attack / Data->AttackMax);
        }
    }
}
