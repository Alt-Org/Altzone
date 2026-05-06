# %Quantum Entity Management {#page-concepts-entity-management}

<br/>

## Entity Manager {#page-concepts-entity-management-entity-manager}

The @cref{Battle.QSimulation.Game,BattleEntityManager} handles **%Quantum Entity** management in the game.  
Uses [{Entity Manager Data}](#page-concepts-entity-management-entity-manager-data) singleton internally
to manage [{Registered Entities}](#page-concepts-entity-management-registered-entities).

### Registered Entities {#page-concepts-entity-management-registered-entities}

**%Quantum Entities** are **Registered** using @clink{Register:Battle.QSimulation.Game.BattleEntityManager.Register(Frame, EntityRef)},
either individually or as an [{Entity Group}](#page-concepts-entity-management-entity-group), and can be **Retrieved** and **Returned**.<br/>
**Registered Entities** are stored offscreen when [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate)
and are assigned an [{Entity ID}](#page-concepts-entity-management-entity-id) for accessing them.<br/>
[{Compound Entities}](#page-concepts-entity-management-compound-entities)
have their own @clink{RegisterCompound:Battle.QSimulation.Game.BattleEntityManager.RegisterCompound(Frame, Battle.QSimulation.Game.BattleEntityManager.CompoundEntityTemplate)} method.<br/>

**Entities** can be **Retrieved** using
@clink{Get:Battle.QSimulation.Game.BattleEntityManager.Get(Frame, BattleEntityID, bool)} to be used in the arena.
[{InPlay}](#page-concepts-entity-management-registered-entities-playstate)<br/>
**Entities** can be **Returned** using
@clink{Return:Battle.QSimulation.Game.BattleEntityManager.Return(Frame, BattleEntityID)}, **teleporting** them back offscreen.
[{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate)<br/>
[{Entity Groups}](#page-concepts-entity-management-entity-group)
have their own @clink{Get:Battle.QSimulation.Game.BattleEntityManager.Get(Frame, BattleEntityID, bool)}
and @clink{Return:Battle.QSimulation.Game.BattleEntityManager.Return(Frame, BattleEntityID)} methods that take an additional **offset** argument.<br/>

#### PlayState {#page-concepts-entity-management-registered-entities-playstate}

The **PlayState** isn't explicitly tracked in the simulation. This is just the terminology we use and a useful way to think about the state of **Entities**.  
[{Registered Entities}](#page-concepts-entity-management-registered-entities) have 2 different states, **InPlay** and **OutOfPlay**.  
When in the arena, a **Registered Entity** is considered **InPlay**. When not in the arena, it is considered **OutOfPlay**.  
When **OutOfPlay**, **Registered Entities** will be stored offscreen.  
The **Get** and **Return** methods send a @ref BattlePlayStateUpdate event to update
the **Unity/View** code on the **ViewPlayState** so it can use and react to that information.

### Compound Entity Handling {#page-concepts-entity-management-compound-entity-handling}

[{Compound Entities}](#page-concepts-entity-management-compound-entities) can be made using
@clink{MakeCompound:Battle.QSimulation.Game.BattleEntityManager.MakeCompound(Frame, Battle.QSimulation.Game.BattleEntityManager.CompoundEntityTemplate)}
or @clink{RegisterCompound:Battle.QSimulation.Game.BattleEntityManager.RegisterCompound(Frame, Battle.QSimulation.Game.BattleEntityManager.CompoundEntityTemplate)} methods.<br/>
[{Compound Entities}](#page-concepts-entity-management-compound-entities) can be **moved** and **teleported** using the
@clink{MoveCompound:Battle.QSimulation.Game.BattleEntityManager.MoveCompound(Frame, EntityRef, FPVector2, FP)}
and @clink{TeleportCompound:Battle.QSimulation.Game.BattleEntityManager.TeleportCompound(Frame, EntityRef, FPVector2, FP)} methods.

@note
**Registered Compound Entities** should only be made using
@clink{RegisterCompound:Battle.QSimulation.Game.BattleEntityManager.RegisterCompound(Frame, Battle.QSimulation.Game.BattleEntityManager.CompoundEntityTemplate)} methods
which marks the resulting [{Entity ID}](#page-concepts-entity-management-entity-id) as a **Compound** allowing the **Entity Manager** to handle it correctly.

<br/>

## Entity ID {#page-concepts-entity-management-entity-id}

Each registered **%Quantum Entity** is assigned an **Entity ID** by [{Entity Manager}](#page-concepts-entity-management-entity-manager).<br/>
Each **Registered** [{Compound Entity}](#page-concepts-entity-management-compound-entities) is also marked as a **Compound**.<br/>
The @cref{Quantum,BattleEntityID} struct is used to represent **Entity IDs**.

<br/>

## Entity Group {#page-concepts-entity-management-entity-group}

**Entities** can be registered to [{Entity Manager}](#page-concepts-entity-management-entity-manager) in **Entity Groups**.<br/>
An **Entity Group** has one [{Entity ID}](#page-concepts-entity-management-entity-id), and individual **Entities** within the **Group**
are accessed by using an **offset value**.

<br/>

## Compound Entities {#page-concepts-entity-management-compound-entities}

**%Quantum Entities** can be linked to a **Parent** **Entity** to form a **Compound** which can then be positioned and rotated together as a one unit.<br/>
The **Parent** has a @cref{Quantum,BattleCompoundEntityQComponent} which contains a list of **Entities** that are linked to the **Compound** **Parent**.<br/>
**Compound Entities** are made from a @cref{Battle.QSimulation.Game.BattleEntityManager,CompoundEntityTemplate}.

**Compound Entities** are handled by [{Entity Manager}](#page-concepts-entity-management-entity-manager).<br/>
**Compound Entities** can be registered to [{Entity Manager}](#page-concepts-entity-management-entity-manager) both individually and in groups.<br/>
**Compound Entities** can also be made and used without registering.

<br/>

---

<br/>

## EntityManagerData (%Quantum Singleton) {#page-concepts-entity-management-entity-manager-data}

The @cref{Quantum,BattleEntityManagerDataQSingleton} struct is a **%Quantum Singleton Component** defined in and generated from BattleEntityManagerData.qtn that
is used internally by [{EntityManager}](#page-concepts-entity-management-entity-manager) to manage registered entities.