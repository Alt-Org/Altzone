namespace Quantum
{
    // Adds functionality to create entities from prototypes and retrieve transform and data components.
    // Main struct definition in BattleCustomEntityReferences.qtn
    // Main struct documentation in qtn-BattleCustomEntityReferences.dox
    public unsafe partial struct BattlePlayerEntityRef
    {
        /// <summary>
        /// The BattlePlayerEntityRef value defining a non-existent BattlePlayerEntity.
        /// </summary>
        public static BattlePlayerEntityRef None => default;

        /// <summary>
        /// Creates a player entity.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <returns>Reference to the created entity.</returns>
        public static BattlePlayerEntityRef Create(Frame f) => (BattlePlayerEntityRef)f.Create();

        /// <summary>
        /// Creates a player entity from a prototype.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="prototype">Prototype player entity is created from.</param>
        /// <returns>Reference to the created entity.</returns>
        public static BattlePlayerEntityRef Create(Frame f, AssetRef<EntityPrototype> prototype) => (BattlePlayerEntityRef)f.Create(prototype);

        /// <summary>
        /// Implicit conversion from BattlePlayerEntityRef to EntityRef.
        /// </summary>
        /// <param name="battlePlayerEntityRef">BattlePlayerEntityRef that's being converted.</param>
        public static implicit operator EntityRef(BattlePlayerEntityRef battlePlayerEntityRef) => battlePlayerEntityRef.ERef;

        /// <summary>
        /// Explicit conversion from EntityRef to BattlePlayerEntityRef.
        /// </summary>
        /// <param name="entityRef">EntityRef that's being converted.</param>
        public static explicit operator BattlePlayerEntityRef(EntityRef entityRef) => new() { ERef = entityRef };

        /// <summary>
        /// Retrieves Transform2D of this entity.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <returns>Transform2D of this entity.</returns>
        public readonly Transform2D* GetTransform(Frame f) => f.Unsafe.GetPointer<Transform2D>(ERef);

        /// <summary>
        /// Retrieves this entity's player data component's pointer.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <returns>This entity's player data component's pointer</returns>
        public readonly BattlePlayerDataQComponent* GetDataQComponent(Frame f) => f.Unsafe.GetPointer<BattlePlayerDataQComponent>(ERef);
    }

    // Adds functionality to create entities from prototypes and retrieve transform and data components.
    // Main struct definition in BattleCustomEntityReferences.qtn
    // Main struct documentation in qtn-BattleCustomEntityReferences.dox
    public unsafe partial struct BattlePlayerShieldEntityRef
    {
        /// <summary>
        /// The BattlePlayerShieldEntityRef value defining a non-existent BattlePlayerShieldEntity.
        /// </summary>
        public static BattlePlayerShieldEntityRef None => default;

        /// <summary>
        /// Creates a shield entity.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <returns>Reference to the created entity.</returns>
        public static BattlePlayerShieldEntityRef Create(Frame f) => (BattlePlayerShieldEntityRef)f.Create();

        /// <summary>
        /// Creates a shield entity from a prototype.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <param name="prototype">Prototype shield entity is created from.</param>
        /// <returns>Reference to the created entity.</returns>
        public static BattlePlayerShieldEntityRef Create(Frame f, AssetRef<EntityPrototype> prototype) => (BattlePlayerShieldEntityRef)f.Create(prototype);

        /// <summary>
        /// Implicit conversion from BattlePlayerShieldEntityRef to EntityRef.
        /// </summary>
        /// <param name="battlePlayerShieldEntityRef">BattlePlayerShieldEntityRef that's being converted.</param>
        public static implicit operator EntityRef(BattlePlayerShieldEntityRef battlePlayerShieldEntityRef) => battlePlayerShieldEntityRef.ERef;

        /// <summary>
        /// Explicit conversion from EntityRef to BattlePlayerShieldEntityRef.
        /// </summary>
        /// <param name="entityRef">EntityRef that's being converted.</param>
        public static explicit operator BattlePlayerShieldEntityRef(EntityRef entityRef) => new() { ERef = entityRef };

        /// <summary>
        /// Retrieves Transform2D of this entity.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <returns>Transform2D of this entity.</returns>
        public readonly Transform2D* GetTransform(Frame f) => f.Unsafe.GetPointer<Transform2D>(ERef);

        /// <summary>
        /// Retrieves this entity's shield data component's pointer.
        /// </summary>
        /// <param name="f">Current simulation frame.</param>
        /// <returns>This entity's shield data component's pointer</returns>
        public readonly BattlePlayerShieldDataQComponent* GetDataQComponent(Frame f) => f.Unsafe.GetPointer<BattlePlayerShieldDataQComponent>(ERef);
    }
}
