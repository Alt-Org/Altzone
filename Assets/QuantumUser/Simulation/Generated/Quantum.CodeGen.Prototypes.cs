// <auto-generated>
// This code was auto-generated by a tool, every time
// the tool executes this code will be reset.
//
// If you need to extend the classes generated to add
// fields or methods to them, please create partial
// declarations in another file.
// </auto-generated>
#pragma warning disable 0109
#pragma warning disable 1591


namespace Quantum.Prototypes {
  using Battle.QSimulation.Goal;
  using Photon.Deterministic;
  using Quantum;
  using Quantum.Core;
  using Quantum.Collections;
  using Quantum.Inspector;
  using Quantum.Physics2D;
  using Quantum.Physics3D;
  using Byte = System.Byte;
  using SByte = System.SByte;
  using Int16 = System.Int16;
  using UInt16 = System.UInt16;
  using Int32 = System.Int32;
  using UInt32 = System.UInt32;
  using Int64 = System.Int64;
  using UInt64 = System.UInt64;
  using Boolean = System.Boolean;
  using String = System.String;
  using Object = System.Object;
  using FlagsAttribute = System.FlagsAttribute;
  using SerializableAttribute = System.SerializableAttribute;
  using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
  using MethodImplOptions = System.Runtime.CompilerServices.MethodImplOptions;
  using FieldOffsetAttribute = System.Runtime.InteropServices.FieldOffsetAttribute;
  using StructLayoutAttribute = System.Runtime.InteropServices.StructLayoutAttribute;
  using LayoutKind = System.Runtime.InteropServices.LayoutKind;
  #if QUANTUM_UNITY //;
  using TooltipAttribute = UnityEngine.TooltipAttribute;
  using HeaderAttribute = UnityEngine.HeaderAttribute;
  using SpaceAttribute = UnityEngine.SpaceAttribute;
  using RangeAttribute = UnityEngine.RangeAttribute;
  using HideInInspectorAttribute = UnityEngine.HideInInspector;
  using PreserveAttribute = UnityEngine.Scripting.PreserveAttribute;
  using FormerlySerializedAsAttribute = UnityEngine.Serialization.FormerlySerializedAsAttribute;
  using MovedFromAttribute = UnityEngine.Scripting.APIUpdating.MovedFromAttribute;
  using CreateAssetMenu = UnityEngine.CreateAssetMenuAttribute;
  using RuntimeInitializeOnLoadMethodAttribute = UnityEngine.RuntimeInitializeOnLoadMethodAttribute;
  #endif //;
  
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleArenaBorderQComponent))]
  public unsafe partial class BattleArenaBorderQComponentPrototype : ComponentPrototype<Quantum.BattleArenaBorderQComponent> {
    public FPVector2 Normal;
    public FP CollisionMinOffset;
    partial void MaterializeUser(Frame frame, ref Quantum.BattleArenaBorderQComponent result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattleArenaBorderQComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattleArenaBorderQComponent result, in PrototypeMaterializationContext context = default) {
        result.Normal = this.Normal;
        result.CollisionMinOffset = this.CollisionMinOffset;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleCollisionTriggerQComponent))]
  public unsafe partial class BattleCollisionTriggerQComponentPrototype : ComponentPrototype<Quantum.BattleCollisionTriggerQComponent> {
    public Quantum.QEnum32<BattleCollisionTriggerType> Type;
    partial void MaterializeUser(Frame frame, ref Quantum.BattleCollisionTriggerQComponent result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattleCollisionTriggerQComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattleCollisionTriggerQComponent result, in PrototypeMaterializationContext context = default) {
        result.Type = this.Type;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleDiamondCounterQSingleton))]
  public unsafe partial class BattleDiamondCounterQSingletonPrototype : ComponentPrototype<Quantum.BattleDiamondCounterQSingleton> {
    public Int32 AlphaDiamonds;
    public Int32 BetaDiamonds;
    partial void MaterializeUser(Frame frame, ref Quantum.BattleDiamondCounterQSingleton result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattleDiamondCounterQSingleton component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattleDiamondCounterQSingleton result, in PrototypeMaterializationContext context = default) {
        result.AlphaDiamonds = this.AlphaDiamonds;
        result.BetaDiamonds = this.BetaDiamonds;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleDiamondDataQComponent))]
  public unsafe partial class BattleDiamondDataQComponentPrototype : ComponentPrototype<Quantum.BattleDiamondDataQComponent> {
    public Quantum.QEnum32<BattleTeamNumber> OwnerTeam;
    public FP TimeUntilDisappearance;
    partial void MaterializeUser(Frame frame, ref Quantum.BattleDiamondDataQComponent result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattleDiamondDataQComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattleDiamondDataQComponent result, in PrototypeMaterializationContext context = default) {
        result.OwnerTeam = this.OwnerTeam;
        result.TimeUntilDisappearance = this.TimeUntilDisappearance;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleGameSessionQSingleton))]
  public unsafe partial class BattleGameSessionQSingletonPrototype : ComponentPrototype<Quantum.BattleGameSessionQSingleton> {
    public QBoolean GameInitialized;
    public Quantum.QEnum32<BattleGameState> State;
    public FP TimeUntilStart;
    public FP GameTimeSec;
    partial void MaterializeUser(Frame frame, ref Quantum.BattleGameSessionQSingleton result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattleGameSessionQSingleton component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattleGameSessionQSingleton result, in PrototypeMaterializationContext context = default) {
        result.GameInitialized = this.GameInitialized;
        result.State = this.State;
        result.TimeUntilStart = this.TimeUntilStart;
        result.GameTimeSec = this.GameTimeSec;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleGoalQComponent))]
  public unsafe partial class BattleGoalQComponentPrototype : ComponentPrototype<Quantum.BattleGoalQComponent> {
    public Quantum.QEnum32<BattleTeamNumber> TeamNumber;
    public QBoolean HasTriggered;
    partial void MaterializeUser(Frame frame, ref Quantum.BattleGoalQComponent result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattleGoalQComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattleGoalQComponent result, in PrototypeMaterializationContext context = default) {
        result.TeamNumber = this.TeamNumber;
        result.HasTriggered = this.HasTriggered;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleGridPosition))]
  public unsafe partial class BattleGridPositionPrototype : StructPrototype {
    public Int32 Row;
    public Int32 Col;
    partial void MaterializeUser(Frame frame, ref Quantum.BattleGridPosition result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.BattleGridPosition result, in PrototypeMaterializationContext context = default) {
        result.Row = this.Row;
        result.Col = this.Col;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattlePlayerDataQComponent))]
  public unsafe class BattlePlayerDataQComponentPrototype : ComponentPrototype<Quantum.BattlePlayerDataQComponent> {
    public PlayerRef PlayerRef;
    public Quantum.QEnum32<BattlePlayerSlot> Slot;
    public Quantum.QEnum32<BattleTeamNumber> TeamNumber;
    public Int32 CharacterId;
    public Int32 CharacterClass;
    public Quantum.Prototypes.BattlePlayerStatsPrototype Stats;
    public Int32 GridExtendTop;
    public Int32 GridExtendBottom;
    public QBoolean HasTargetPosition;
    public FPVector2 TargetPosition;
    public FP RotationBase;
    public FP RotationOffset;
    public FP CurrentHp;
    public MapEntityId HitboxShieldEntity;
    public MapEntityId HitboxCharacterEntity;
    public Quantum.Prototypes.FrameTimerPrototype DamageCooldown;
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattlePlayerDataQComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattlePlayerDataQComponent result, in PrototypeMaterializationContext context = default) {
        result.PlayerRef = this.PlayerRef;
        result.Slot = this.Slot;
        result.TeamNumber = this.TeamNumber;
        result.CharacterId = this.CharacterId;
        result.CharacterClass = this.CharacterClass;
        this.Stats.Materialize(frame, ref result.Stats, in context);
        result.GridExtendTop = this.GridExtendTop;
        result.GridExtendBottom = this.GridExtendBottom;
        result.HasTargetPosition = this.HasTargetPosition;
        result.TargetPosition = this.TargetPosition;
        result.RotationBase = this.RotationBase;
        result.RotationOffset = this.RotationOffset;
        result.CurrentHp = this.CurrentHp;
        PrototypeValidator.FindMapEntity(this.HitboxShieldEntity, in context, out result.HitboxShieldEntity);
        PrototypeValidator.FindMapEntity(this.HitboxCharacterEntity, in context, out result.HitboxCharacterEntity);
        this.DamageCooldown.Materialize(frame, ref result.DamageCooldown, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattlePlayerDataTemplateQComponent))]
  public unsafe partial class BattlePlayerDataTemplateQComponentPrototype : ComponentPrototype<Quantum.BattlePlayerDataTemplateQComponent> {
    public Int32 GridExtendTop;
    public Int32 GridExtendBottom;
    public Quantum.Prototypes.BattlePlayerHitboxTemplatePrototype HitboxShield;
    public Quantum.Prototypes.BattlePlayerHitboxTemplatePrototype HitboxCharacter;
    partial void MaterializeUser(Frame frame, ref Quantum.BattlePlayerDataTemplateQComponent result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattlePlayerDataTemplateQComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattlePlayerDataTemplateQComponent result, in PrototypeMaterializationContext context = default) {
        result.GridExtendTop = this.GridExtendTop;
        result.GridExtendBottom = this.GridExtendBottom;
        this.HitboxShield.Materialize(frame, ref result.HitboxShield, in context);
        this.HitboxCharacter.Materialize(frame, ref result.HitboxCharacter, in context);
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattlePlayerHitboxColliderTemplate))]
  public unsafe partial class BattlePlayerHitboxColliderTemplatePrototype : StructPrototype {
    public IntVector2 Position;
    public IntVector2 Size;
    partial void MaterializeUser(Frame frame, ref Quantum.BattlePlayerHitboxColliderTemplate result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.BattlePlayerHitboxColliderTemplate result, in PrototypeMaterializationContext context = default) {
        result.Position = this.Position;
        result.Size = this.Size;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattlePlayerHitboxQComponent))]
  public unsafe class BattlePlayerHitboxQComponentPrototype : ComponentPrototype<Quantum.BattlePlayerHitboxQComponent> {
    public MapEntityId PlayerEntity;
    public Quantum.QEnum32<BattlePlayerHitboxType> HitboxType;
    public Quantum.QEnum32<BattlePlayerCollisionType> CollisionType;
    public FPVector2 Normal;
    public FP CollisionMinOffset;
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattlePlayerHitboxQComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattlePlayerHitboxQComponent result, in PrototypeMaterializationContext context = default) {
        PrototypeValidator.FindMapEntity(this.PlayerEntity, in context, out result.PlayerEntity);
        result.HitboxType = this.HitboxType;
        result.CollisionType = this.CollisionType;
        result.Normal = this.Normal;
        result.CollisionMinOffset = this.CollisionMinOffset;
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattlePlayerHitboxTemplate))]
  public unsafe partial class BattlePlayerHitboxTemplatePrototype : StructPrototype {
    [FreeOnComponentRemoved()]
    [DynamicCollectionAttribute()]
    public Quantum.Prototypes.BattlePlayerHitboxColliderTemplatePrototype[] ColliderTemplateList = {};
    public Quantum.QEnum32<BattlePlayerCollisionType> CollisionType;
    partial void MaterializeUser(Frame frame, ref Quantum.BattlePlayerHitboxTemplate result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.BattlePlayerHitboxTemplate result, in PrototypeMaterializationContext context = default) {
        if (this.ColliderTemplateList.Length == 0) {
          result.ColliderTemplateList = default;
        } else {
          var list = frame.AllocateList(out result.ColliderTemplateList, this.ColliderTemplateList.Length);
          for (int i = 0; i < this.ColliderTemplateList.Length; ++i) {
            Quantum.BattlePlayerHitboxColliderTemplate tmp = default;
            this.ColliderTemplateList[i].Materialize(frame, ref tmp, in context);
            list.Add(tmp);
          }
        }
        result.CollisionType = this.CollisionType;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattlePlayerManagerDataQSingleton))]
  public unsafe class BattlePlayerManagerDataQSingletonPrototype : ComponentPrototype<Quantum.BattlePlayerManagerDataQSingleton> {
    public Int32 PlayerCount;
    [ArrayLengthAttribute(4)]
    public Quantum.QEnum32<BattlePlayerPlayState>[] PlayStates = new Quantum.QEnum32<BattlePlayerPlayState>[4];
    [ArrayLengthAttribute(4)]
    public PlayerRef[] PlayerRefs = new PlayerRef[4];
    [ArrayLengthAttribute(4)]
    public MapEntityId[] SelectedCharacters = new MapEntityId[4];
    [ArrayLengthAttribute(12)]
    public MapEntityId[] AllCharacters = new MapEntityId[12];
    [ArrayLengthAttribute(4)]
    public Int32[] SelectedCharacterNumbers = new Int32[4];
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattlePlayerManagerDataQSingleton component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattlePlayerManagerDataQSingleton result, in PrototypeMaterializationContext context = default) {
        result.PlayerCount = this.PlayerCount;
        for (int i = 0, count = PrototypeValidator.CheckLength(PlayStates, 4, in context); i < count; ++i) {
          *result.PlayStates.GetPointer(i) = this.PlayStates[i];
        }
        for (int i = 0, count = PrototypeValidator.CheckLength(PlayerRefs, 4, in context); i < count; ++i) {
          *result.PlayerRefs.GetPointer(i) = this.PlayerRefs[i];
        }
        for (int i = 0, count = PrototypeValidator.CheckLength(SelectedCharacters, 4, in context); i < count; ++i) {
          PrototypeValidator.FindMapEntity(this.SelectedCharacters[i], in context, out *result.SelectedCharacters.GetPointer(i));
        }
        for (int i = 0, count = PrototypeValidator.CheckLength(AllCharacters, 12, in context); i < count; ++i) {
          PrototypeValidator.FindMapEntity(this.AllCharacters[i], in context, out *result.AllCharacters.GetPointer(i));
        }
        for (int i = 0, count = PrototypeValidator.CheckLength(SelectedCharacterNumbers, 4, in context); i < count; ++i) {
          result.SelectedCharacterNumbers[i] = this.SelectedCharacterNumbers[i];
        }
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattlePlayerStats))]
  public unsafe partial class BattlePlayerStatsPrototype : StructPrototype {
    public FP Hp;
    public FP Speed;
    public FP CharacterSize;
    public FP Attack;
    public FP Defence;
    partial void MaterializeUser(Frame frame, ref Quantum.BattlePlayerStats result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.BattlePlayerStats result, in PrototypeMaterializationContext context = default) {
        result.Hp = this.Hp;
        result.Speed = this.Speed;
        result.CharacterSize = this.CharacterSize;
        result.Attack = this.Attack;
        result.Defence = this.Defence;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleProjectileQComponent))]
  public unsafe partial class BattleProjectileQComponentPrototype : ComponentPrototype<Quantum.BattleProjectileQComponent> {
    public QBoolean IsLaunched;
    public QBoolean IsMoving;
    public FP Speed;
    public FP SpeedPotential;
    public FP AccelerationTimer;
    public FPVector2 Direction;
    public FP Radius;
    public Quantum.QEnum32<BattleEmotionState> Emotion;
    [ArrayLengthAttribute(2)]
    public Quantum.QEnum8<BattleProjectileCollisionFlags>[] CollisionFlags = new Quantum.QEnum8<BattleProjectileCollisionFlags>[2];
    partial void MaterializeUser(Frame frame, ref Quantum.BattleProjectileQComponent result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattleProjectileQComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattleProjectileQComponent result, in PrototypeMaterializationContext context = default) {
        result.IsLaunched = this.IsLaunched;
        result.IsMoving = this.IsMoving;
        result.Speed = this.Speed;
        result.SpeedPotential = this.SpeedPotential;
        result.AccelerationTimer = this.AccelerationTimer;
        result.Direction = this.Direction;
        result.Radius = this.Radius;
        result.Emotion = this.Emotion;
        for (int i = 0, count = PrototypeValidator.CheckLength(CollisionFlags, 2, in context); i < count; ++i) {
          *result.CollisionFlags.GetPointer(i) = this.CollisionFlags[i];
        }
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleProjectileSpawnerQComponent))]
  public unsafe partial class BattleProjectileSpawnerQComponentPrototype : ComponentPrototype<Quantum.BattleProjectileSpawnerQComponent> {
    public QBoolean HasSpawned;
    partial void MaterializeUser(Frame frame, ref Quantum.BattleProjectileSpawnerQComponent result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattleProjectileSpawnerQComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattleProjectileSpawnerQComponent result, in PrototypeMaterializationContext context = default) {
        result.HasSpawned = this.HasSpawned;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleSoulWallQComponent))]
  public unsafe partial class BattleSoulWallQComponentPrototype : ComponentPrototype<Quantum.BattleSoulWallQComponent> {
    public Quantum.QEnum32<BattleTeamNumber> Team;
    public Quantum.QEnum32<BattleSoulWallRow> Row;
    public Int32 WallNumber;
    public Quantum.QEnum32<BattleEmotionState> Emotion;
    public FPVector2 Normal;
    public FP CollisionMinOffset;
    partial void MaterializeUser(Frame frame, ref Quantum.BattleSoulWallQComponent result, in PrototypeMaterializationContext context);
    public override Boolean AddToEntity(FrameBase f, EntityRef entity, in PrototypeMaterializationContext context) {
        Quantum.BattleSoulWallQComponent component = default;
        Materialize((Frame)f, ref component, in context);
        return f.Set(entity, component) == SetResult.ComponentAdded;
    }
    public void Materialize(Frame frame, ref Quantum.BattleSoulWallQComponent result, in PrototypeMaterializationContext context = default) {
        result.Team = this.Team;
        result.Row = this.Row;
        result.WallNumber = this.WallNumber;
        result.Emotion = this.Emotion;
        result.Normal = this.Normal;
        result.CollisionMinOffset = this.CollisionMinOffset;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.BattleSoulWallTemplate))]
  public unsafe partial class BattleSoulWallTemplatePrototype : StructPrototype {
    public Quantum.Prototypes.BattleGridPositionPrototype Position;
    public Int32 WidthType;
    public Int32 ColorIndex;
    partial void MaterializeUser(Frame frame, ref Quantum.BattleSoulWallTemplate result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.BattleSoulWallTemplate result, in PrototypeMaterializationContext context = default) {
        this.Position.Materialize(frame, ref result.Position, in context);
        result.WidthType = this.WidthType;
        result.ColorIndex = this.ColorIndex;
        MaterializeUser(frame, ref result, in context);
    }
  }
  [System.SerializableAttribute()]
  [Quantum.Prototypes.Prototype(typeof(Quantum.Input))]
  public unsafe partial class InputPrototype : StructPrototype {
    public Quantum.QEnum32<BattleMovementInputType> MovementInput;
    public QBoolean MovementDirectionIsNormalized;
    public Quantum.Prototypes.BattleGridPositionPrototype MovementPosition;
    public FPVector2 MovementDirection;
    public Button RotationInput;
    public FP RotationValue;
    public Int32 PlayerCharacterNumber;
    partial void MaterializeUser(Frame frame, ref Quantum.Input result, in PrototypeMaterializationContext context);
    public void Materialize(Frame frame, ref Quantum.Input result, in PrototypeMaterializationContext context = default) {
        result.MovementInput = this.MovementInput;
        result.MovementDirectionIsNormalized = this.MovementDirectionIsNormalized;
        this.MovementPosition.Materialize(frame, ref result.MovementPosition, in context);
        result.MovementDirection = this.MovementDirection;
        result.RotationInput = this.RotationInput;
        result.RotationValue = this.RotationValue;
        result.PlayerCharacterNumber = this.PlayerCharacterNumber;
        MaterializeUser(frame, ref result, in context);
    }
  }
}
#pragma warning restore 0109
#pragma warning restore 1591
