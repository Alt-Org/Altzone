using System.Runtime.CompilerServices;
using Quantum;

namespace Battle.QSimulation.Game
{
    public unsafe struct BattlePlayerHandle
    {
        public static BattlePlayerHandle Create(BattlePlayerSlot slot)
        {
            BattlePlayerHandle handle = new();
            handle.SetPlayerData(slot);

            return handle;
        }

        public static BattlePlayerHandle Create(Frame f, EntityRef entity)
        {
            BattlePlayerHandle handle = new();

            BattlePlayerLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerLinkQComponent>(entity);

            handle.SetPlayerData(link->Slot);
            handle.SetPlayerCharacter(f, link->CharacterEntityRef);
            if (link->Type == PlayerEntityType.Hitbox) handle.SetPlayerHitbox(f, entity);

            return handle;
        }

        public void LoadPlayerCharacter(Frame f, EntityRef entity)
        {
            BattlePlayerLinkQComponent* link = f.Unsafe.GetPointer<BattlePlayerLinkQComponent>(entity);

            SetPlayerCharacter(f, link->CharacterEntityRef);

            if (link->Type == PlayerEntityType.Hitbox)
            {
                SetPlayerHitbox(f, entity);
            }
            else
            {
                UnsetHitbox();
            }
        }

        public BattlePlayerData* playerData {  get; private set; }

        public EntityRef characterEntityRef { get; private set; }

        public BattlePlayerCharacterDataQComponent* playerCharacterData { get; private set; }

        public Transform2D* characterTransform { get; private set; }

        public EntityRef hitboxEntityRef { get; private set; }

        public BattlePlayerHitboxQComponent* hitboxComponent { get; private set; }

        public Transform2D* hitboxTransform { get; private set; }

        private void SetPlayerData(BattlePlayerSlot slot)
        {

        }

        private void SetPlayerCharacter(Frame f, EntityRef entity)
        {
            characterEntityRef  = entity;
            playerCharacterData = f.Unsafe.GetPointer<BattlePlayerCharacterDataQComponent>(entity);
            characterTransform  = f.Unsafe.GetPointer<Transform2D>(entity);
        }

        private void SetPlayerHitbox(Frame f, EntityRef entity)
        {
            hitboxEntityRef = entity;
            hitboxComponent = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(entity);
            hitboxTransform = f.Unsafe.GetPointer<Transform2D>(entity);
        }

        private void UnsetHitbox()
        {
            hitboxEntityRef = EntityRef.None;
            hitboxComponent = null;
            hitboxTransform = null;
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

        public EntityRef shieldEntityRef {  get; private set; }

        public BattlePlayerShieldDataQComponent* shieldData { get; private set; }

        public Transform2D* shieldTransform { get; private set; }

        public EntityRef shieldHitboxEntityRef { get; private set; }

        public BattlePlayerHitboxQComponent* shieldHitboxComponent { get; private set; }

        public Transform2D* shieldHitboxTransform { get; private set; }

        private void SetShield(Frame f, EntityRef entity)
        {
            shieldEntityRef = entity;
            shieldData      = f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(entity);
            shieldTransform = f.Unsafe.GetPointer<Transform2D>(entity);
        }

        private void SetHitbox(Frame f, EntityRef entity)
        {
            shieldHitboxEntityRef = entity;
            shieldHitboxComponent = f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(entity);
            shieldHitboxTransform = f.Unsafe.GetPointer<Transform2D>(entity);
        }
    }

    public unsafe struct BattleProjectileHandle
    {
        public static BattleProjectileHandle Create(Frame f, EntityRef entity)
        {
            BattleProjectileHandle handle = new();

            BattleProjectileLinkQComponent* link = f.Unsafe.GetPointer<BattleProjectileLinkQComponent>(entity);

            handle.ERef       = link->ERef;
            handle.projectile = f.Unsafe.GetPointer<BattleProjectileQComponent>(link->ERef);
            handle.transform  = f.Unsafe.GetPointer<Transform2D>(link->ERef);

            return handle;
        }

        public EntityRef ERef { get; private set; }

        public BattleProjectileQComponent* projectile {  get; private set; }

        public Transform2D* transform { get; private set; }
    }
}
