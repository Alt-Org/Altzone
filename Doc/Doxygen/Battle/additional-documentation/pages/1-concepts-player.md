# Player concepts {#page-concepts-player}
%Quantum handles recognizing players through a [PlayerRef🡵](https://doc-api.photonengine.com/en/quantum/current/struct_quantum_1_1_player_ref.html), but we prefer to use **PlayerSlot** as defined by us whenever possible.  
Each player has data with them that isn't connected to any specific character under the player's control.  
Each player controls multiple character entities.  
See [{Player slots and teams}](#page-concepts-player-slots-teams)  
See [{Player manager data}](#page-concepts-player-manager-data)  
See [{Player character entity}](#page-concepts-player-character-entity)

```dot
digraph Player {
  color=white;
  fontcolor=white;
  bgcolor=black;

  node [shape=box, style=filled, color=white, fontcolor=white, fillcolor=black];
  edge [color=gray];

  Player              [label="Player"];
  Data                [label="Data\nNot related to individual characters"];
  PlayerRef           [label="PlayerRef\nUsed by Quantum"];
  PlayerSlot          [label="PlayerSlot\nDefined and preferred by us"];
  CharacterEntities   [label="Character entities"];
  CharacterEntityList [label="Character entity 1 | Character entity 2 | Character entity 3", shape=record];

  Player -> Data -> PlayerRef, PlayerSlot;
  Player -> CharacterEntities;
  CharacterEntities -> CharacterEntityList [dir=none];
}
```

## Player slots and teams {#page-concepts-player-slots-teams}
Each player has an assigned @cref{Quantum,BattlePlayerSlot} and a @cref{Quantum,BattleTeamNumber}.  
The possible slots are 1-4.  
The teams are TeamAlpha and TeamBeta.  
Players in slots 1 and 2 are in TeamAlpha and players in slots 3 and 4 in TeamBeta.  
Guest and Spectator slots also exist, but are not currently used in %Battle.  
The enums for player slots and teams are defined in [{Simulation}](#page-concepts-player-simulation).

```dot
graph PlayerSlotsAndTeams {
  layout=osage;
  color=white;
  fontcolor=white;
  bgcolor=black;
  pack=0;

  node [shape=box, style=filled, color=white, fontcolor=white, fillcolor=black];

  subgraph cluster_beta {
    label="Team Beta";
    Slot4
    Slot3
  }

  subgraph cluster_alpha {
    label="Team Alpha";
    Slot2
    Slot1
  }
}
```

<br/>

## Player manager data {#page-concepts-player-manager-data}
Player data not connected to individual player characters is handled by @cref{Battle.QSimulation.Player,BattlePlayerManager}.  
The [{PlayerManagerData}](#page-concepts-player-simulation-playermanagerdata) %Quantum singleton component is used to store player data.
The [{PlayerHandle}](#page-concepts-player-simulation-playerhandle) struct allows the code to access player manager data of specific individual players.  
Player manager data is defined and used in [{Simulation}](#page-concepts-player-simulation).

<br/>

## Joining and initializing {#page-concepts-player-initializing}
%Quantum handles players connecting to the game. The data from %Quantum is used to initialize everything related to players in the game.
@cref{Battle.QSimulation.Game,BattleGameControlQSystem} receives player information from %Quantum when a player joins and passes it to @cref{Battle.QSimulation.Player,BattlePlayerManager}, which processes the player's data and registers them as having joined.

Once all players have joined, @cref{Battle.QSimulation.Game,BattleGameControlQSystem} tells @cref{Battle.QSimulation.Player,BattlePlayerManager} to create [{Player character entities}](#page-concepts-player-character-entity) for all players.

```dot
digraph PlayerJoining {
  color=white;
  fontcolor=white;
  bgcolor=black;

  node [shape=box, style=filled, color=white, fontcolor=white, fillcolor=black];
  edge [color=gray];

  Quantum [label="Quantum", color="#F159E4", fontcolor="#F159E4"];

  subgraph cluster_gameControlSystem {
    color=transparent;
    node [shape=box, style=filled, color=white, fontcolor=white, fillcolor=black];

    BattleGameControlQSystem [label="BattleGameControlQSystem", color="#F159E4", fontcolor="#F159E4"];

    node [color=transparent, fillcolor=transparent];

    OnPlayerAdded_in     [label="OnPlayerAdded"];
    OnPlayerAdded_out    [label="OnPlayerAdded"];
    AllPlayersJoined_out [label="When all players have joined"];
  }

  subgraph cluster_playerManager {
    color=transparent;
    node [shape=box, style=filled, color=white, fontcolor=white, fillcolor=black];

    BattlePlayerManager      [label="BattlePlayerManager", color="#F159E4", fontcolor="#F159E4"];

    node [color=transparent, fillcolor=transparent];

    RegisterPlayer_in   [label="RegisterPlayer"];
    CreatePlayers_in    [label="CreatePlayers"];
  }

  edge [dir=none];

  OnPlayerAdded_in -> BattleGameControlQSystem -> OnPlayerAdded_out, AllPlayersJoined_out;
  RegisterPlayer_in, CreatePlayers_in -> BattlePlayerManager;

  edge [dir=forward];

  Quantum -> OnPlayerAdded_in;
  OnPlayerAdded_out -> RegisterPlayer_in;
  AllPlayersJoined_out -> CreatePlayers_in;

}
```

<br/>

## PlayerInput {#page-concepts-player-input}
Players can interact with the game through moving and rotating their character, as well as switching between their available characters.  
Player inputs are processed and compiled into a @ref Quantum.Input "Quantum input struct" on the Unity/View side in @cref{Battle.View.Player,BattlePlayerInput}. The created struct is passed over to %Quantum.  
%Quantum synchronises the struct for all connected clients, and classes on the [{Quantum simulation}](#page-concepts-player-simulation) side use the contained data.

See [Input🡵](https://doc.photonengine.com/quantum/current/manual/input) %Quantum's documentation for more info.

```dot
digraph PlayerInputGraph {
  color=white;
  fontcolor=white;
  bgcolor=black;

  node [shape=box, style=filled, color=white, fontcolor=white, fillcolor=black];
  edge [color=gray];

  subgraph cluster_unityview {
    color="#41C7F1";
    fontcolor="#41C7F1";
    label = "Unity View";

    node [color="#41C7F1", fontcolor="#41C7F1"];

    PlayerInput [label="PlayerInput\n\nInputs are processed and compiled\ninto a Quantum input struct"];
  }

  InputStructView       [label="Quantum input struct\nPolled by Quantum", color=transparent];
  Quantum               [label="Quantum"];
  InputStructSimulation [label="Quantum input struct\nRead from Quantum", color=transparent];

  subgraph cluster_simulation {
    color="#F159E4";
    fontcolor="#F159E4";
    label = "Simulation";

    node [color="#F159E4", fontcolor="#F159E4"];

    PlayerQSystem            [label="PlayerQSystem\n\nReads, processes and passes\nthe Quantum input struct forward"];
  }

  PlayerInput -> InputStructView [dir=none];
  InputStructView -> Quantum;
  Quantum -> InputStructSimulation [dir=back];
  InputStructSimulation -> PlayerQSystem [dir=none];
}
```

<br/>

## Player character entity {#page-concepts-player-character-entity}
Each player controls three character %Quantum entities in the game.  
For each player one character is present on the stage at a time and @cref{Battle.QSimulation.Player,BattlePlayerManager} handles spawning and despawning character entities when switching between them.

```dot
digraph PlayerCharacterEntities {
  color=white;
  fontcolor=white;
  bgcolor=black;

  node [shape=box, style=filled, color=white, fontcolor=white, fillcolor=black];
  edge [color=gray];

  Player              [label="Player", shape=ellipse];
  CharacterEntities   [label="Character entities"];
  CharacterEntityList [label="Character entity 1\n(In play) | Character entity 2\n(Out of play) | Character entity 3\n(Out of play)", shape=record];

  Player -> CharacterEntities;
  CharacterEntities -> CharacterEntityList [dir=none];
}
```

These entities are created based on Unity prefabs. Entities are controlled by %Quantum simulation.  
The Unity prefab root GameObject contains a %Quantum entity prototype component, where the entity is defined.

During gameplay the player character exists both as a %Quantum entity inside [{Simulation}](#page-concepts-player-simulation) and a Unity gameObject inside **Unity View**. %Quantum links the gameObject and entity together.  
The entity contains %Quantum components used by the %Quantum simulation. The most significant of these is the [{PlayerData (Quantum component)}](#page-concepts-player-simulation-playerdata), which is our own defined data relating to player character entities.  
The Unity root gameObject has the child object PlayerViewModel, containing all things related to the visible elements of player characters. The attached @cref{Battle.View.Player,BattlePlayerViewController} component implements **Unity View/Visual** logic for player characters.

```dot
digraph PlayerCharacterEntity {
  color=white;
  fontcolor=white;
  bgcolor=black;


  edge [color=gray];

  subgraph cluster_quantum {
    color="#F159E4";
    fontcolor="#F159E4";
    label = "Quantum";

    node [shape=box, style=filled, color="#F159E4", fontcolor="#F159E4", fillcolor=black];

    Entity         [label="Quantum Entity"];
    Transform2D    [label="Transform2D\n(Quantum component)"];
    PlayerData     [label="PlayerDataQComponent\n(Quantum component)"];
    View           [label="Quantum view\n(Quantum component)"];

    Entity -> Transform2D, PlayerData, View;
  }

  subgraph cluster_unity {
    color="#41C7F1";
    fontcolor="#41C7F1";
    label = "Unity View";

    node [shape=box, style=filled, color="#41C7F1", fontcolor="#41C7F1", fillcolor=black];

    RootGameObject       [label="Root\n(GameObject)"];
    ViewModel            [label="View model\n(GameObject)"];
    PlayerViewController [label="Player View Controller\n(Unity MonoBehavior)"];

    RootGameObject -> ViewModel -> PlayerViewController;
  }

  Entity -> RootGameObject;
}
```

<br/>

---

<br/>

## %Quantum simulation {#page-concepts-player-simulation}

<br/>

### Overview {#page-concepts-player-simulation-overview}

```dot
digraph PlayerSimulation {
  color=white;
  fontcolor=white;
  bgcolor=black;

  node [shape=box, style=filled, color="#F159E4", fontcolor="#F159E4", fillcolor=black];
  edge [color=gray];

  PlayerManager            [label="PlayerManager\n\nHandles player management,\nallowing other classes to focus on gameplay logic."];
  PlayerManagerData        [label="PlayerManagerData\n\nPlayerManager related data\naccessed by other classes through PlayerHandle."];
  PlayerHandle             [label="PlayerHandle\n\nA struct defined in PlayerManager,\nwhich allows other classes to access PlayerManagerData for each individual player."];
  PlayerQSystem            [label="PlayerQSystem\n\nHandles primary gameplay logic for players.\nUses other classes for specific player logic."];
  PlayerMovementController [label="PlayerMovementController\n\nHandles the logic for moving and rotating player characters."];
  PlayerBotController      [label="PlayerBotController\n\nHandles AI and other logic for bots."];
  PlayerData               [label="PlayerData\n\nAn individual player character's data."];

  PlayerManager -> PlayerManagerData, PlayerHandle [dir=none];
  PlayerManagerData -> PlayerHandle [dir=none];
  PlayerManager, PlayerHandle -> PlayerQSystem [dir=back];
  PlayerQSystem -> PlayerMovementController, PlayerBotController;
  PlayerManager, PlayerQSystem, PlayerMovementController, PlayerBotController -> PlayerData [dir=none];
}
```
<br/>

### PlayerManagerData (%Quantum singleton) {#page-concepts-player-simulation-playermanagerdata}
The @cref{Quantum,BattlePlayerManagerDataQSingleton} struct is a %Quantum singleton component defined in and generated from BattlePlayerManagerData.qtn containing all our defined data for players. [{PlayerHandle}](#page-concepts-player-simulation-playerhandle) is used to access this data for each individual player.

<br/>

### PlayerManager {#page-concepts-player-simulation-playermanager}

<br/>

### PlayerHandle {#page-concepts-player-simulation-playerhandle}
The @cref{Battle.QSimulation.Player.BattlePlayerManager,PlayerHandle} struct defined in @cref{Battle.QSimulation.Player,BattlePlayerManager} allows the code to access [{Player manager data}](#page-concepts-player-manager-data) of each individual player.

There is both a private @cref{Battle.QSimulation.Player.BattlePlayerManager,PlayerHandleInternal} struct containing all the data for use internally within @cref{Battle.QSimulation.Player,BattlePlayerManager}, and the public @cref{Battle.QSimulation.Player.BattlePlayerManager,PlayerHandle} exposing some parts to the rest of the game.

<br/>

### PlayerData (%Quantum component) {#page-concepts-player-simulation-playerdata}
The @cref{Quantum,BattlePlayerDataQComponent} struct is defined in and generated from BattlePlayerData.qtn. This contains data specific to each [{Player character entity}](#page-concepts-player-character-entity) used by the %Quantum simulation during gameplay.

<br/>

### PlayerQSystem {#page-concepts-player-simulation-playerqsystem}
@cref{Battle.QSimulation.Player,BattlePlayerQSystem} contains the primary %Quantum player logic. This %Quantum system contains code for handling collisions and the update method for player characters.
Other classes are utilized for specific aspects of player logic.
See [{PlayerMovementController}](#page-concepts-player-simulation-playerqsystem-movement-controller)
See [{PlayerBotController}]()

<br/>

### PlayerMovementController {#page-concepts-player-simulation-playerqsystem-movement-controller}
@cref{Battle.QSimulation.Player,BattlePlayerMovementController} contains the primary @cref{Battle.QSimulation.Player.BattlePlayerMovementController,UpdateMovement} method which handles player movement, and is called by [{BattlePlayerQSystem}](#page-concepts-player-simulation-playerqsystem).
Also contains individual helper methods for moving and rotating players, which can be used by other scripts.

<br/>

### PlayerBotController {#page-concepts-player-simulation-botcontroller}

<br/>

---

<br/>

### Character classes {#page-concepts-player-characters-classes}
Player character classes function by having implementable methods that are called in certain situations during a game, such as when a projectile collides with a player character. Classes can also implement an update method. These methods can be used to implement functionality on top of for example the default collision methods to change how different character classes function.

Character classes are implemented by creating a unique class that inherits one of the two BattlePlayerClassBase classes defined in BattlePlayerClassManager.cs. These classes can choose to implement any of the available methods for functionality. Character classes can also optionally have a data QComponent for additional data the class will use.

Scripts such as BattlePlayerQSystem call methods in BattlePlayerClassManager, which then in turn call the corresponding method for the character class of the specified player character. This way each characters possible character class methods are always correctly called.

<br/>

---