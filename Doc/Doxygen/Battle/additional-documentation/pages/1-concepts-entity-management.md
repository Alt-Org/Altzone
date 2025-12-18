# %Quantum Entity Management {#page-concepts-entity-management}

<br/>

## Entity Manager {#page-concepts-entity-management-entity-manager}

The @cref{Battle.QSimulation.Game,BattleEntityManager} handles **%Quantum Entity** management in the game. **Entities** are registered, either individually or as an [{Entity Group}](#page-concepts-entity-management-entity-group), and can be retrieved and returned.<br/>
**Entities** are stored offscreen when not in use and are assigned an [{Entity ID}](#page-concepts-entity-management-entity-id) for accessing them.<br/>
**Entities** are retrieved using @cref{Battle.QSimulation.Game.BattleEntityManager,Get(Frame\, BattleEntityID)} to be used in the game.<br/>
**Entities** can be returned using @cref{Battle.QSimulation.Game.BattleEntityManager,Return(Frame\, BattleEntityID)}, teleporting them back offscreen.<br/>
[{Entity Groups}](#page-concepts-entity-management-entity-group) have their own @cref{Battle.QSimulation.Game.BattleEntityManager,Get(Frame\, BattleEntityID\, int)} and @cref{Battle.QSimulation.Game.BattleEntityManager,Return(Frame\, BattleEntityID\, int)} methods.

<br/>

## Entity ID {#page-concepts-entity-management-entity-id}

Each registered **%Quantum Entity** is assigned an **Entity ID** by [{Entity Manager}](#page-concepts-entity-management-entity-manager).<br/>
The @cref{Quantum,BattleEntityID} struct, which is a wrapper for an `int` value, is used to represent **Entity IDs** to make code more readable by referencing **Entities** with a named type instead of an `int` value.

<br/>

## Entity Group {#page-concepts-entity-management-entity-group}

**Entities** can be registered to [{Entity Manager}](#page-concepts-entity-management-entity-manager) in **Entity Groups**.<br/>
An **Entity Group** has one [{Entity ID}](#page-concepts-entity-management-entity-id), and individual **Entities** within the **Group** are accessed by using an **offset value**.