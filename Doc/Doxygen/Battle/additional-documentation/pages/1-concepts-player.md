# Player concepts {#page-concepts-player}

<br/>

## Quantum and Unity {#page-concepts-player-quantum-unity}
The concept of a player encompasses both the player and their characters in the game and the player as a user in %Quantum.

In %Quantum the player is a user. Each player has a set of data. The contents of the data is defined by us. This data is read and used by BattlePlayerManager when a player connects to the room for the game.

Within the game itself each player is at any given moment a controlled character entity interacting with the rest of the game.

<br/>

---

<br/>

## Simulation {#page-concepts-player-simulation}
All character functionality happens in %Quantum simulation.

<br/>

### playerHandle {#page-concepts-player-simulation-playerhandle}
The playerHandle struct defined in BattlePlayerManager contains data related to each player, separate from any individual character entity, such as their slot, team and current play state.  
There is both a private struct containing all the data for use within BattlePlayerManager, and a public version exposing some parts to the rest of the game.

<br/>

### Joining and initializing {#page-concepts-player-simulation-initializing}
%Quantum handles players connecting to the game. The data from %Quantum is used to initialize everything related to players in the game.  
When a player connects to the game, the OnPlayerAdded method in BattleGameControlQSystem is called, which then calls the RegisterPlayers method of BattlePlayerManager.  
The player's user ID and slot data is verified and their playerRef is added to their playerHandle.

Once all player's have connected and been registered, BattleGameControlQSystem calls the CreatePlayers and SpawnPlayers methods of BattlePlayerManager.  
CreatePlayers initializes all character entities, including their hitboxes, playerData, character classes etc, and their %View elements.  
SpawnPlayers simply spawns each player's first character into the game.

<br/>

### Player character logic {#page-concepts-player-simulation-character-logic}
Player character logic is defined in BattlePlayerQSystem. This %Quantum system contains code for handling collisions. It also contains the update method for player characters, which handles relaying input data to other scripts for movement and character swapping.

<br/>

#### playerData {#page-concepts-player-simulation-character-logic-playerdata}
Defined in BattlePlayerData.qtn. This data is specific to each player character. A characters class, stats and hitboxes are defined here.

<br/>

#### Player interaction {#page-concepts-player-simulation-character-logic-interaction}
Players can interact with the game through moving and rotating their character, as well as switching between their available characters.  
Player inputs are processed in BattlePlayerInput, and compiled into an input struct defined in BattlePlayerInput.qtn to be used by other scripts.

<br/>

##### Movement and rotation {#page-concepts-player-simulation-character-logic-interaction-movement}
Player movement and rotation are handled by BattlePlayerMovementController.  
Input data is read from the received struct, and the position and rotation of the character entity and associated hitbox entities are updated accordingly.

<br/>

##### Character swapping {#page-concepts-player-simulation-character-logic-interaction-swapping}
Character swapping happens through calling the SpawnPlayer method of BattlePlayerManager. This is done by BattlePlayerQSystem itself.

<br/>

#### Collisions {#page-concepts-player-simulation-character-logic-collisions}
Player character entities are given associated hitbox entities. Collisions between %Quantum hitbox entities are handled by %Quantum, and that information is used to perform our own operations.

Collisions between the player character or shield and the projectile are handled in BattlePlayerQSystem.  
Damage is applied to the character or shield, and in case of a shield collision the damage of the projectile is updated to that of the character, as defined in its playerData.

<br/>

---

<br/>

## Player characters {#page-concepts-player-characters}
Player characters are created using both a prefab and an entity prototype.  
Character prefabs contain the character sprites and a BattlePlayerViewController script for controlling the visible character in the game.  
The entity prototype for a character is where QComponents such as the playerData are set. A characters entity prototype is found connected to it's prefab's root gameObject.

<br/>

### Character classes {#page-concepts-player-characters-classes}
Player character classes function by having implementable methods that are called in certain situations during a game, such as when a projectile collides with a player character. Classes can also implement an update method. These methods can be used to implement functionality on top of for example the default collision methods to change how different character classes function.

Character classes are implemented by creating a unique class that inherits one of the two BattlePlayerClassBase classes defined in BattlePlayerClassManager.cs. These classes can choose to implement any of the available methods for functionality. Character classes can also optionally have a data QComponent for additional data the class will use.

Scripts such as BattlePlayerQSystem call methods in BattlePlayerClassManager, which then in turn call the corresponding method for the character class of the specified player character. This way each characters possible character class methods are always correctly called.

<br/>

---