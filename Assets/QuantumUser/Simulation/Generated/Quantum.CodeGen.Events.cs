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


namespace Quantum {
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
  
  public unsafe partial class Frame {
    public unsafe partial struct FrameEvents {
      static partial void GetEventTypeCountCodeGen(ref Int32 eventCount) {
        eventCount = 16;
      }
      static partial void GetParentEventIDCodeGen(Int32 eventID, ref Int32 parentEventID) {
        switch (eventID) {
          default: break;
        }
      }
      static partial void GetEventTypeCodeGen(Int32 eventID, ref System.Type result) {
        switch (eventID) {
          case EventBattleViewWaitForPlayers.ID: result = typeof(EventBattleViewWaitForPlayers); return;
          case EventBattleViewInit.ID: result = typeof(EventBattleViewInit); return;
          case EventBattleViewActivate.ID: result = typeof(EventBattleViewActivate); return;
          case EventBattleViewGetReadyToPlay.ID: result = typeof(EventBattleViewGetReadyToPlay); return;
          case EventBattleViewGameStart.ID: result = typeof(EventBattleViewGameStart); return;
          case EventBattleViewGameOver.ID: result = typeof(EventBattleViewGameOver); return;
          case EventBattlePlayerViewInit.ID: result = typeof(EventBattlePlayerViewInit); return;
          case EventBattleSoulWallViewInit.ID: result = typeof(EventBattleSoulWallViewInit); return;
          case EventBattleStoneCharacterPieceViewInit.ID: result = typeof(EventBattleStoneCharacterPieceViewInit); return;
          case EventBattleChangeEmotionState.ID: result = typeof(EventBattleChangeEmotionState); return;
          case EventBattlePlaySoundFX.ID: result = typeof(EventBattlePlaySoundFX); return;
          case EventBattleLastRowWallDestroyed.ID: result = typeof(EventBattleLastRowWallDestroyed); return;
          case EventBattleCharacterTakeDamage.ID: result = typeof(EventBattleCharacterTakeDamage); return;
          case EventBattleDebugUpdateStatsOverlay.ID: result = typeof(EventBattleDebugUpdateStatsOverlay); return;
          case EventBattleDebugOnScreenMessage.ID: result = typeof(EventBattleDebugOnScreenMessage); return;
          default: break;
        }
      }
      public EventBattleViewWaitForPlayers BattleViewWaitForPlayers() {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleViewWaitForPlayers>(EventBattleViewWaitForPlayers.ID);
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleViewInit BattleViewInit() {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleViewInit>(EventBattleViewInit.ID);
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleViewActivate BattleViewActivate() {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleViewActivate>(EventBattleViewActivate.ID);
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleViewGetReadyToPlay BattleViewGetReadyToPlay() {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleViewGetReadyToPlay>(EventBattleViewGetReadyToPlay.ID);
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleViewGameStart BattleViewGameStart() {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleViewGameStart>(EventBattleViewGameStart.ID);
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleViewGameOver BattleViewGameOver(BattleTeamNumber WinningTeam, FP GameLengthSec) {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleViewGameOver>(EventBattleViewGameOver.ID);
        ev.WinningTeam = WinningTeam;
        ev.GameLengthSec = GameLengthSec;
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattlePlayerViewInit BattlePlayerViewInit(EntityRef Entity, BattlePlayerSlot Slot, FP ModelScale) {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattlePlayerViewInit>(EventBattlePlayerViewInit.ID);
        ev.Entity = Entity;
        ev.Slot = Slot;
        ev.ModelScale = ModelScale;
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleSoulWallViewInit BattleSoulWallViewInit(EntityRef Entity, FP ModelScale, Int32 EmotionIndicatorColorIndex, Int32 DebugColorIndex) {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleSoulWallViewInit>(EventBattleSoulWallViewInit.ID);
        ev.Entity = Entity;
        ev.ModelScale = ModelScale;
        ev.EmotionIndicatorColorIndex = EmotionIndicatorColorIndex;
        ev.DebugColorIndex = DebugColorIndex;
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleStoneCharacterPieceViewInit BattleStoneCharacterPieceViewInit(Int32 WallNumber, BattleTeamNumber Team, Int32 EmotionIndicatorColorIndex) {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleStoneCharacterPieceViewInit>(EventBattleStoneCharacterPieceViewInit.ID);
        ev.WallNumber = WallNumber;
        ev.Team = Team;
        ev.EmotionIndicatorColorIndex = EmotionIndicatorColorIndex;
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleChangeEmotionState BattleChangeEmotionState(BattleEmotionState Emotion) {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleChangeEmotionState>(EventBattleChangeEmotionState.ID);
        ev.Emotion = Emotion;
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattlePlaySoundFX BattlePlaySoundFX(BattleSoundFX Effect) {
        var ev = _f.Context.AcquireEvent<EventBattlePlaySoundFX>(EventBattlePlaySoundFX.ID);
        ev.Effect = Effect;
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleLastRowWallDestroyed BattleLastRowWallDestroyed(Int32 WallNumber, BattleTeamNumber Team, FP LightrayRotation, BattleLightrayColor LightrayColor, BattleLightraySize LightraySize) {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleLastRowWallDestroyed>(EventBattleLastRowWallDestroyed.ID);
        ev.WallNumber = WallNumber;
        ev.Team = Team;
        ev.LightrayRotation = LightrayRotation;
        ev.LightrayColor = LightrayColor;
        ev.LightraySize = LightraySize;
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleCharacterTakeDamage BattleCharacterTakeDamage(EntityRef Entity, BattleTeamNumber Team, BattlePlayerSlot Slot, Int32 CharacterNumber, FP HealthPercentage) {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleCharacterTakeDamage>(EventBattleCharacterTakeDamage.ID);
        ev.Entity = Entity;
        ev.Team = Team;
        ev.Slot = Slot;
        ev.CharacterNumber = CharacterNumber;
        ev.HealthPercentage = HealthPercentage;
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleDebugUpdateStatsOverlay BattleDebugUpdateStatsOverlay(BattlePlayerSlot Slot, BattlePlayerStats Stats) {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleDebugUpdateStatsOverlay>(EventBattleDebugUpdateStatsOverlay.ID);
        ev.Slot = Slot;
        ev.Stats = Stats;
        _f.AddEvent(ev);
        return ev;
      }
      public EventBattleDebugOnScreenMessage BattleDebugOnScreenMessage(QString512 Message) {
        if (_f.IsPredicted) return null;
        var ev = _f.Context.AcquireEvent<EventBattleDebugOnScreenMessage>(EventBattleDebugOnScreenMessage.ID);
        ev.Message = Message;
        _f.AddEvent(ev);
        return ev;
      }
    }
  }
  public unsafe partial class EventBattleViewWaitForPlayers : EventBase {
    public new const Int32 ID = 1;
    protected EventBattleViewWaitForPlayers(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleViewWaitForPlayers() : 
        base(1, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 41;
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleViewInit : EventBase {
    public new const Int32 ID = 2;
    protected EventBattleViewInit(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleViewInit() : 
        base(2, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 43;
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleViewActivate : EventBase {
    public new const Int32 ID = 3;
    protected EventBattleViewActivate(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleViewActivate() : 
        base(3, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 47;
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleViewGetReadyToPlay : EventBase {
    public new const Int32 ID = 4;
    protected EventBattleViewGetReadyToPlay(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleViewGetReadyToPlay() : 
        base(4, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 53;
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleViewGameStart : EventBase {
    public new const Int32 ID = 5;
    protected EventBattleViewGameStart(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleViewGameStart() : 
        base(5, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 59;
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleViewGameOver : EventBase {
    public new const Int32 ID = 6;
    public BattleTeamNumber WinningTeam;
    public FP GameLengthSec;
    protected EventBattleViewGameOver(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleViewGameOver() : 
        base(6, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 61;
        hash = hash * 31 + WinningTeam.GetHashCode();
        hash = hash * 31 + GameLengthSec.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventBattlePlayerViewInit : EventBase {
    public new const Int32 ID = 7;
    public EntityRef Entity;
    public BattlePlayerSlot Slot;
    public FP ModelScale;
    protected EventBattlePlayerViewInit(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattlePlayerViewInit() : 
        base(7, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 67;
        hash = hash * 31 + Entity.GetHashCode();
        hash = hash * 31 + Slot.GetHashCode();
        hash = hash * 31 + ModelScale.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleSoulWallViewInit : EventBase {
    public new const Int32 ID = 8;
    public EntityRef Entity;
    public FP ModelScale;
    public Int32 EmotionIndicatorColorIndex;
    public Int32 DebugColorIndex;
    protected EventBattleSoulWallViewInit(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleSoulWallViewInit() : 
        base(8, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 71;
        hash = hash * 31 + Entity.GetHashCode();
        hash = hash * 31 + ModelScale.GetHashCode();
        hash = hash * 31 + EmotionIndicatorColorIndex.GetHashCode();
        hash = hash * 31 + DebugColorIndex.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleStoneCharacterPieceViewInit : EventBase {
    public new const Int32 ID = 9;
    public Int32 WallNumber;
    public BattleTeamNumber Team;
    public Int32 EmotionIndicatorColorIndex;
    protected EventBattleStoneCharacterPieceViewInit(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleStoneCharacterPieceViewInit() : 
        base(9, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 73;
        hash = hash * 31 + WallNumber.GetHashCode();
        hash = hash * 31 + Team.GetHashCode();
        hash = hash * 31 + EmotionIndicatorColorIndex.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleChangeEmotionState : EventBase {
    public new const Int32 ID = 10;
    public BattleEmotionState Emotion;
    protected EventBattleChangeEmotionState(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleChangeEmotionState() : 
        base(10, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 79;
        hash = hash * 31 + Emotion.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventBattlePlaySoundFX : EventBase {
    public new const Int32 ID = 11;
    public BattleSoundFX Effect;
    protected EventBattlePlaySoundFX(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattlePlaySoundFX() : 
        base(11, EventFlags.Server|EventFlags.Client) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 83;
        hash = hash * 31 + Effect.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleLastRowWallDestroyed : EventBase {
    public new const Int32 ID = 12;
    public Int32 WallNumber;
    public BattleTeamNumber Team;
    public FP LightrayRotation;
    public BattleLightrayColor LightrayColor;
    public BattleLightraySize LightraySize;
    protected EventBattleLastRowWallDestroyed(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleLastRowWallDestroyed() : 
        base(12, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 89;
        hash = hash * 31 + WallNumber.GetHashCode();
        hash = hash * 31 + Team.GetHashCode();
        hash = hash * 31 + LightrayRotation.GetHashCode();
        hash = hash * 31 + LightrayColor.GetHashCode();
        hash = hash * 31 + LightraySize.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleCharacterTakeDamage : EventBase {
    public new const Int32 ID = 13;
    public EntityRef Entity;
    public BattleTeamNumber Team;
    public BattlePlayerSlot Slot;
    public Int32 CharacterNumber;
    public FP HealthPercentage;
    protected EventBattleCharacterTakeDamage(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleCharacterTakeDamage() : 
        base(13, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 97;
        hash = hash * 31 + Entity.GetHashCode();
        hash = hash * 31 + Team.GetHashCode();
        hash = hash * 31 + Slot.GetHashCode();
        hash = hash * 31 + CharacterNumber.GetHashCode();
        hash = hash * 31 + HealthPercentage.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleDebugUpdateStatsOverlay : EventBase {
    public new const Int32 ID = 14;
    public BattlePlayerSlot Slot;
    public BattlePlayerStats Stats;
    protected EventBattleDebugUpdateStatsOverlay(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleDebugUpdateStatsOverlay() : 
        base(14, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 101;
        hash = hash * 31 + Slot.GetHashCode();
        hash = hash * 31 + Stats.GetHashCode();
        return hash;
      }
    }
  }
  public unsafe partial class EventBattleDebugOnScreenMessage : EventBase {
    public new const Int32 ID = 15;
    public QString512 Message;
    protected EventBattleDebugOnScreenMessage(Int32 id, EventFlags flags) : 
        base(id, flags) {
    }
    public EventBattleDebugOnScreenMessage() : 
        base(15, EventFlags.Server|EventFlags.Client|EventFlags.Synced) {
    }
    public new QuantumGame Game {
      get {
        return (QuantumGame)base.Game;
      }
      set {
        base.Game = value;
      }
    }
    public override Int32 GetHashCode() {
      unchecked {
        var hash = 103;
        hash = hash * 31 + Message.GetHashCode();
        return hash;
      }
    }
  }
}
#pragma warning restore 0109
#pragma warning restore 1591
