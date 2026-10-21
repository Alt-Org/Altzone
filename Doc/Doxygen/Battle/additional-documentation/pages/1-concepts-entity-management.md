# %Quantum Entity Management {#page-concepts-entity-management}

<br/>

## Managed Entities {#page-concepts-entity-management-managed-entities}

**%Quantum Entities** managed by the [{Entity Manager}](#page-concepts-entity-management-entity-manager) have a @cref{Quantum,BattleManageEntityQComponent}.  
There are 3 types of **Managed Entities**:
[{Registered}](#page-concepts-entity-management-registered-entities),
[{Compound}](#page-concepts-entity-management-compound-entities) and
**Registered Compound**.
These types are tracked by the @cref{Quantum,BattleManageEntityQComponent.IsRegistered} and @cref{Quantum,BattleManageEntityQComponent.IsCompound} booleans.  
**Registered Compound Entities** are simply both
[{Registered}](#page-concepts-entity-management-registered-entities) and
[{Compound}](#page-concepts-entity-management-compound-entities) **Entities** at the same time.

### Compound Entities {#page-concepts-entity-management-compound-entities}

**Compound Entities** are [{Managed Entities of type Compound}](#page-concepts-entity-management-managed-entities) that are composed of a **Parent** **Entity** with other **Entities** linked to it,
forming the **Compound**, which can then be positioned and rotated together as one unit.  
The **Parent** has a @cref{Quantum,BattleManageEntityQComponent.CompoundLinkedEntities} list which contains **Entities** that are linked to the **Compound** **Parent**.  
**Compound Entities** are made from a @cref{Battle.QSimulation.Game.BattleEntityManager,CompoundEntityTemplate}.

**Compound Entities** are handled by [{Entity Manager}](#page-concepts-entity-management-entity-manager).  
**Compound Entities** can be registered to [{Entity Manager}](#page-concepts-entity-management-entity-manager) both individually and in groups.  
**Compound Entities** can also be made and used without registering.

## Entity Manager {#page-concepts-entity-management-entity-manager}

The @cref{Battle.QSimulation.Game,BattleEntityManager} handles **%Quantum Entity** management in the game.  
Uses [{Entity Manager Data}](#page-concepts-entity-management-entity-manager-data) singleton internally
to manage [{Registered Entities}](#page-concepts-entity-management-registered-entities).

### Registered Entities {#page-concepts-entity-management-registered-entities}

**Registered Entities** are [{Managed Entities of type Registered}](#page-concepts-entity-management-managed-entities)
that are **Registered** to the **Entity Manager**, are assigned an [{Entity ID}](#page-concepts-entity-management-entity-id) and
have a [{PlayState}](#page-concepts-entity-management-registered-entities-playstate).  
**Registered Entities** are stored offscreen when [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate).  

**%Quantum Entities** can be **Registered** using @clink{Register:Battle.QSimulation.Game.BattleEntityManager.Register(Frame, EntityRef)},
either individually or as an [{Entity Group}](#page-concepts-entity-management-entity-group).
[{Compound Entities}](#page-concepts-entity-management-compound-entities)
have their own @clink{RegisterCompound:Battle.QSimulation.Game.BattleEntityManager.RegisterCompound(Frame, Battle.QSimulation.Game.BattleEntityManager.CompoundEntityTemplate)} method.  
**Registered Entities** can be **Accessed**, **Used** and **Returned**.  

**Registered Entities** can be **Accessed** using @clink{Get:Battle.QSimulation.Game.BattleEntityManager.Get(Frame, BattleEntityID)},
whether or not they are [{InPlay}](#page-concepts-entity-management-registered-entities-playstate) or [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate),
allowing access to the **%Quantum Entity**.  
**Registered Entities** can be **Used** using @clink{Use:Battle.QSimulation.Game.BattleEntityManager.Use(Frame, BattleEntityID)},
marking them [{InPlay}](#page-concepts-entity-management-registered-entities-playstate) and ready to be used in the arena.  
**Registered Entities** can be **Returned** using @clink{Return:Battle.QSimulation.Game.BattleEntityManager.Return(Frame, BattleEntityID)},
marking them [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate) and **teleporting** them back offscreen.  
[{Entity Groups}](#page-concepts-entity-management-entity-group)
have their own @clink{Get:Battle.QSimulation.Game.BattleEntityManager.Get(Frame, BattleEntityID)}, @clink{Use:Battle.QSimulation.Game.BattleEntityManager.Use(Frame, BattleEntityID)}
and @clink{Return:Battle.QSimulation.Game.BattleEntityManager.Return(Frame, BattleEntityID)} methods that take an additional **offset** argument.  

**Registered Entities** can always be accessed, regardless of whether they are
[{InPlay}](#page-concepts-entity-management-registered-entities-playstate) or [{OutOfPlay}](#page-concepts-entity-management-registered-entities-playstate).  
However, the **Use** method should be called to clearly indicate the intent to use the **Registered Entity** in the arena,
and the **Return** method should be called when the **Entity** is no longer needed,
ensuring its [{PlayState}](#page-concepts-entity-management-registered-entities-playstate) accurately reflects whether it is current in use.

#### PlayState {#page-concepts-entity-management-registered-entities-playstate}

[{Registered Entities}](#page-concepts-entity-management-registered-entities) have 2 different states, **InPlay** and **OutOfPlay**, tracked by @cref{Quantum,BattleEntityPlayState}.  
When in the arena, a **Registered Entity** is **InPlay**. When not in the arena, it is **OutOfPlay**.  
When **OutOfPlay**, **Registered Entities** will be stored offscreen.  
The **Use** and **Return** methods update the  send a @ref BattlePlayStateUpdate event to update
the **Unity/View** code on the **ViewPlayState** so it can use and react to that information.

### Compound Entity Handling {#page-concepts-entity-management-compound-entity-handling}

[{Compound Entities}](#page-concepts-entity-management-compound-entities) can be made using
@clink{MakeCompound:Battle.QSimulation.Game.BattleEntityManager.MakeCompound(Frame, Battle.QSimulation.Game.BattleEntityManager.CompoundEntityTemplate)}
or @clink{RegisterCompound:Battle.QSimulation.Game.BattleEntityManager.RegisterCompound(Frame, Battle.QSimulation.Game.BattleEntityManager.CompoundEntityTemplate)} methods.  
[{Compound Entities}](#page-concepts-entity-management-compound-entities) can be **moved** and **teleported** using the
@clink{MoveCompound:Battle.QSimulation.Game.BattleEntityManager.MoveCompound(Frame, EntityRef, FPVector2, FP)}
and @clink{TeleportCompound:Battle.QSimulation.Game.BattleEntityManager.TeleportCompound(Frame, EntityRef, FPVector2, FP)} methods.

@note
**Registered Compound Entities** should only be made using
@clink{RegisterCompound:Battle.QSimulation.Game.BattleEntityManager.RegisterCompound(Frame, Battle.QSimulation.Game.BattleEntityManager.CompoundEntityTemplate)} methods
which marks the resulting [{Entity ID}](#page-concepts-entity-management-entity-id) as a **Compound** allowing the **Entity Manager** to handle it correctly.

<br/>

## Entity ID {#page-concepts-entity-management-entity-id}

Each registered **%Quantum Entity** is assigned an **Entity ID** by [{Entity Manager}](#page-concepts-entity-management-entity-manager).  
Each **Registered** [{Compound Entity}](#page-concepts-entity-management-compound-entities) is also marked as a **Compound**.  
The @cref{Quantum,BattleEntityID} struct is used to represent **Entity IDs**.

<br/>

## Entity Group {#page-concepts-entity-management-entity-group}

**Entities** can be registered to [{Entity Manager}](#page-concepts-entity-management-entity-manager) in **Entity Groups**.  
An **Entity Group** has one [{Entity ID}](#page-concepts-entity-management-entity-id), and individual **Entities** within the **Group**
are accessed by using an **offset value**.

<br/>

---

<br/>

## EntityManagerData (%Quantum Singleton) {#page-concepts-entity-management-entity-manager-data}

The @cref{Quantum,BattleEntityManagerDataQSingleton} struct is a **%Quantum Singleton Component** defined in and generated from BattleEntityManagerData.qtn that
is used internally by [{EntityManager}](#page-concepts-entity-management-entity-manager) to manage registered entities.