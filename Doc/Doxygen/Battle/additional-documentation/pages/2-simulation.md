# Simulation {#page-simulation}

## Namespaces {#page-simulation-namespaces}

|  Namespace                                                       || Description                              |
| :------------------------- | :----------------------------------- | :--------------------------------------- |
| @cref{Battle.QSimulation}                                        || @copybrief Battle.QSimulation            |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,Game}       | @copybrief Battle.QSimulation.Game       |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,Player}     | @copybrief Battle.QSimulation.Player     |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,SoulWall}   | @copybrief Battle.QSimulation.SoulWall   |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,Projectile} | @copybrief Battle.QSimulation.Projectile |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,Goal}       | @copybrief Battle.QSimulation.Goal       |
|                                                                                                            |||
| @cref{Quantum}                                                   || @copybrief Quantum                       |

@bigtext{[[Namespace Summary]](#index-namespace-summary)}

<br/>

## Directories {#page-simulation-directories}

|  Path                                                                                                                                              ||| Description                                                                                      |
| :---------------------------------- | :--------------------------------------------- | :------------------------------------------------------------ | :----------------------------------------------------------------------------------------------- |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation}                                                                || Game Simulation Logic Directory.<br/>Contains deterministic %Quantum Simulation logic and state. |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation} | @dirref{Altzone/Assets/QuantumUser/Simulation,Battle/Scripts} | Simulation %Battle C# Script                                                                     |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation} | @dirref{Altzone/Assets/QuantumUser/Simulation,Battle/Qtn}     | Simulation %Battle QTN files                                                                     |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation} | @dirref{Altzone/Assets/QuantumUser/Simulation,Generated}      | Generated Simulation scripts                                                                     |

@bigtext{[[File Summary]](#index-file-summary)}

<br/>

## Systems {#page-simulation-systems}

[Quantum Systems🡵] are **C# classes** that handle game logic.  
In **%Battle** [Quantum Systems🡵] have **"QSystem"** suffix. [[Naming]](#index-naming)

|  Namespace                                                        || Class                                                               | Description                                                             |
| :------------------------- | :------------------------------------ | :------------------------------------------------------------------ | :---------------------------------------------------------------------- |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Game}       | @cref{Battle.QSimulation.Game,BattleGameControlQSystem}             | @copybrief Battle.QSimulation.Game.BattleGameControlQSystem             |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Game}       | @cref{Battle.QSimulation.Game,BattleCollisionQSystem}               | @copybrief Battle.QSimulation.Game.BattleCollisionQSystem               |
|                                                                                                                                                                                                                 ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Player}     | @cref{Battle.QSimulation.Player,BattlePlayerQSystem}                | @copybrief Battle.QSimulation.Player.BattlePlayerQSystem                |
|                                                                                                                                                                                                                 ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,SoulWall}   | @cref{Battle.QSimulation.SoulWall,BattleSoulWallQSystem}            | @copybrief Battle.QSimulation.SoulWall.BattleSoulWallQSystem            |
|                                                                                                                                                                                                                 ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Projectile} | @cref{Battle.QSimulation.Projectile,BattleProjectileSpawnerQSystem} | @copybrief Battle.QSimulation.Projectile.BattleProjectileSpawnerQSystem |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Projectile} | @cref{Battle.QSimulation.Projectile,BattleProjectileQSystem}        | @copybrief Battle.QSimulation.Projectile.BattleProjectileQSystem        |
|                                                                                                                                                                                                                 ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Goal}       | @cref{Battle.QSimulation.Goal,BattleGoalQSystem}                    | @copybrief Battle.QSimulation.Goal.BattleGoalQSystem                    |
|                                                                                                                                                                                                                 ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Diamond}    | @cref{Battle.QSimulation.Diamond,BattleDiamondQSystem}              | @copybrief Battle.QSimulation.Diamond.BattleDiamondQSystem              |

<br/>

## Managers & Controllers {#page-simulation-managers}

In **%Battle** **Managers** have **"Manager"** suffix and **Controllers** have **"Controller"** suffix. [[Naming]](#index-naming)

|  Namespace                                                    || Class                                                           | Description                                                         |
| :------------------------- | :-------------------------------- | :---------------------------------------------------            | :------------------------------------------------------------------ |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Game}   | @cref{Battle.QSimulation.Game,BattleGridManager}                | @copybrief Battle.QSimulation.Game.BattleGridManager                |
|                                                                                                                                                                                                     ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Player} | @cref{Battle.QSimulation.Player,BattlePlayerManager}            | @copybrief Battle.QSimulation.Player.BattlePlayerManager            |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Player} | @cref{Battle.QSimulation.Player,BattlePlayerClassManager}       | @copybrief Battle.QSimulation.Player.BattlePlayerClassManager       |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Player} | @cref{Battle.QSimulation.Player,BattlePlayerMovementController} | @copybrief Battle.QSimulation.Player.BattlePlayerMovementController |

<br/>

## Components {#page-simulation-components}

[Quantum Components🡵] are **C# structs** that are generated from `.qtn` files which can be attached to **%Quantum Entities**.  
In %Battle [Quantum Components🡵] have **"QComponent"** suffix. [[Naming]](#index-naming)

|  Namespace        | Component                                          | Description                                            |
| :---------------- | :------------------------------------------------- | :----------------------------------------------------- |
| @crefd{Quantum}   | @cref{Quantum,BattleArenaBorderQComponent}         | @copybrief Quantum.BattleArenaBorderQComponent         |
| @crefd{Quantum}   | @cref{Quantum,BattleCollisionTriggerQComponent}    | @copybrief Quantum.BattleCollisionTriggerQComponent    |
|                                                                                                                               |||
| @crefd{Quantum}   | @cref{Quantum,BattleDiamondDataQComponent}         | @copybrief Quantum.BattleDiamondDataQComponent         |
|                                                                                                                               |||
| @crefd{Quantum}   | @cref{Quantum,BattleGoalQComponent}                | @copybrief Quantum.BattleGoalQComponent                |
|                                                                                                                               |||
| @crefd{Quantum}   | @cref{Quantum,BattlePlayerDataQComponent}          | @copybrief Quantum.BattlePlayerDataQComponent          |
| @crefd{Quantum}   | @cref{Quantum,BattlePlayerDataTemplateQComponent}  | @copybrief Quantum.BattlePlayerDataTemplateQComponent  |
| @crefd{Quantum}   | @cref{Quantum,BattlePlayerHitboxQComponent}        | @copybrief Quantum.BattlePlayerHitboxQComponent        |
|                                                                                                                               |||
| @crefd{Quantum}   | @cref{Quantum,BattleProjectileQComponent}          | @copybrief Quantum.BattleProjectileQComponent          |
| @crefd{Quantum}   | @cref{Quantum,BattleProjectileSpawnerQComponent}   | @copybrief Quantum.BattleProjectileSpawnerQComponent   |
|                                                                                                                               |||
| @crefd{Quantum}   | @cref{Quantum,BattleSoulWallQComponent}            | @copybrief Quantum.BattleSoulWallQComponent            |

<br/>

## Singletons {#page-simulation-singletons}

**Singleton** is a type of [Quantum Component🡵]. Only one instance of them can exist at a given time and they can be found without **EntityRef**.  
In **%Battle** **Singletons** have **"QSingleton"** suffix. [[Naming]](#index-naming)

|  Namespace        | Component                                          | Description                                            |
| :---------------- | :------------------------------------------------- | :----------------------------------------------------- |
| @crefd{Quantum}   | @cref{Quantum,BattleDiamondCounterQSingleton}      | @copybrief Quantum.BattleDiamondCounterQSingleton      |
| @crefd{Quantum}   | @cref{Quantum,BattleGameSessionQSingleton}         | @copybrief Quantum.BattleGameSessionQSingleton         |
| @crefd{Quantum}   | @cref{Quantum,BattlePlayerManagerDataQSingleton}   | @copybrief Quantum.BattlePlayerManagerDataQSingleton   |

<br/>

## Enums {#page-simulation-enums}

|  Namespace        | Enum                                               | Description                                            |
| :---------------- | :------------------------------------------------- | :----------------------------------------------------- |
| @crefd{Quantum}   | @cref{Quantum,BattleCollisionTriggerType}          | @copybrief Quantum.BattleCollisionTriggerType          |
| @crefd{Quantum}   | @cref{Quantum,BattleEmotionState}                  | @copybrief Quantum.BattleEmotionState                  |
| @crefd{Quantum}   | @cref{Quantum,BattleGameState}                     | @copybrief Quantum.BattleGameState                     |
| @crefd{Quantum}   | @cref{Quantum,BattleLightrayColor}                 | @copybrief Quantum.BattleLightrayColor                 |
| @crefd{Quantum}   | @cref{Quantum,BattleLightraySize}                  | @copybrief Quantum.BattleLightraySize                  |
| @crefd{Quantum}   | @cref{Quantum,BattleMovementInputType}             | @copybrief Quantum.BattleMovementInputType             |
| @crefd{Quantum}   | @cref{Quantum,BattlePlayerCharacterClass}          | @copybrief Quantum.BattlePlayerCharacterClass          |
| @crefd{Quantum}   | @cref{Quantum,BattlePlayerCharacterState}          | @copybrief Quantum.BattlePlayerCharacterState          |
| @crefd{Quantum}   | @cref{Quantum,BattlePlayerCollisionType}           | @copybrief Quantum.BattlePlayerCollisionType           |
| @crefd{Quantum}   | @cref{Quantum,BattlePlayerHitboxType}              | @copybrief Quantum.BattlePlayerHitboxType              |
| @crefd{Quantum}   | @cref{Quantum,BattlePlayerPlayState}               | @copybrief Quantum.BattlePlayerPlayState               |
| @crefd{Quantum}   | @cref{Quantum,BattlePlayerSlot}                    | @copybrief Quantum.BattlePlayerSlot                    |
| @crefd{Quantum}   | @cref{Quantum,BattleProjectileCollisionFlags}      | @copybrief Quantum.BattleProjectileCollisionFlags      |
| @crefd{Quantum}   | @cref{Quantum,BattleSoulWallRow}                   | @copybrief Quantum.BattleSoulWallRow                   |
| @crefd{Quantum}   | @cref{Quantum,BattleSoundFX}                       | @copybrief Quantum.BattleSoundFX                       |
| @crefd{Quantum}   | @cref{Quantum,BattleTeamNumber}                    | @copybrief Quantum.BattleTeamNumber                    |

<br/>

## Signals {#page-simulation-signals}

[Quantum Signals🡵] are **C# interfaces** that are generated from `.qtn` files.  
In **%Battle** all [Quantum Signals🡵] are located in BattleSignals.qtn file.

|  Namespace        | Interface                                           | Description                                             |
| :---------------- | :-------------------------------------------------- | :------------------------------------------------------ |
| @crefd{Quantum}   | @cref{Quantum,ISignalBattleOnDiamondHitPlayer}      | @copybrief Quantum.ISignalBattleOnDiamondHitPlayer      |
| @crefd{Quantum}   | @cref{Quantum,ISignalBattleOnDiamondHitArenaBorder} | @copybrief Quantum.ISignalBattleOnDiamondHitArenaBorder |
|                                                                                                                                 |||
| @crefd{Quantum}   | @cref{Quantum,ISignalBattleOnGameOver}              | @copybrief Quantum.ISignalBattleOnGameOver              |

<br/>

## Events {#page-simulation-events}

[Quantum Events🡵] are **C# classes** that are generated from `.qtn` files.  
In **%Battle** all [Quantum Events🡵] are located in BattleEvents.qtn file.

|  Namespace        | Class                                                       | Description                                                     |
| :---------------- | :---------------------------------------------------------- | :-------------------------------------------------------------- |
| @crefd{Quantum}   | @cref{Quantum,EventBattleViewWaitForPlayers}                | @copybrief Quantum.EventBattleViewWaitForPlayers                |
| @crefd{Quantum}   | @cref{Quantum,EventBattleViewPlayerConnected}               | @copybrief Quantum.EventBattleViewPlayerConnected               |
| @crefd{Quantum}   | @cref{Quantum,EventBattleViewAllPlayersConnected}           | @copybrief Quantum.EventBattleViewAllPlayersConnected           |
| @crefd{Quantum}   | @cref{Quantum,EventBattleViewInit}                          | @copybrief Quantum.EventBattleViewInit                          |
|                                                                                                                                                 |||
| @crefd{Quantum}   | @cref{Quantum,EventBattleViewActivate}                      | @copybrief Quantum.EventBattleViewActivate                      |
| @crefd{Quantum}   | @cref{Quantum,EventBattleViewGetReadyToPlay}                | @copybrief Quantum.EventBattleViewGetReadyToPlay                |
| @crefd{Quantum}   | @cref{Quantum,EventBattleViewGameStart}                     | @copybrief Quantum.EventBattleViewGameStart                     |
| @crefd{Quantum}   | @cref{Quantum,EventBattleViewGameOver}                      | @copybrief Quantum.EventBattleViewGameOver                      |
|                                                                                                                                                 |||
| @crefd{Quantum}   | @cref{Quantum,EventBattlePlayerViewInit}                    | @copybrief Quantum.EventBattlePlayerViewInit                    |
| @crefd{Quantum}   | @cref{Quantum,EventBattleSoulWallViewInit}                  | @copybrief Quantum.EventBattleSoulWallViewInit                  |
| @crefd{Quantum}   | @cref{Quantum,EventBattleStoneCharacterPieceViewInit}       | @copybrief Quantum.EventBattleStoneCharacterPieceViewInit       |
|                                                                                                                                                 |||
| @crefd{Quantum}   | @cref{Quantum,EventBattleViewSetRotationJoystickVisibility} | @copybrief Quantum.EventBattleViewSetRotationJoystickVisibility |
|                                                                                                                                                 |||
| @crefd{Quantum}   | @cref{Quantum,EventBattleChangeEmotionState}                | @copybrief Quantum.EventBattleChangeEmotionState                |
| @crefd{Quantum}   | @cref{Quantum,EventBattleProjectileChangeSpeed}             | @copybrief Quantum.EventBattleProjectileChangeSpeed             |
| @crefd{Quantum}   | @cref{Quantum,EventBattleProjectileChangeGlowStrength}      | @copybrief Quantum.EventBattleProjectileChangeGlowStrength      |
| @crefd{Quantum}   | @cref{Quantum,EventBattlePlaySoundFX}                       | @copybrief Quantum.EventBattlePlaySoundFX                       |
| @crefd{Quantum}   | @cref{Quantum,EventBattleLastRowWallDestroyed}              | @copybrief Quantum.EventBattleLastRowWallDestroyed              |
| @crefd{Quantum}   | @cref{Quantum,EventBattleDiamondLanded}                     | @copybrief Quantum.EventBattleDiamondLanded                     |
| @crefd{Quantum}   | @cref{Quantum,EventBattleCharacterTakeDamage}               | @copybrief Quantum.EventBattleCharacterTakeDamage               |
| @crefd{Quantum}   | @cref{Quantum,EventBattleShieldTakeDamage}                  | @copybrief Quantum.EventBattleShieldTakeDamage                  |
|                                                                                                                                                 |||
| @crefd{Quantum}   | @cref{Quantum,EventBattleDebugUpdateStatsOverlay}           | @copybrief Quantum.EventBattleDebugUpdateStatsOverlay           |
| @crefd{Quantum}   | @cref{Quantum,EventBattleDebugOnScreenMessage}              | @copybrief Quantum.EventBattleDebugOnScreenMessage              |

<br/>

## Configs {#page-simulation-configs}

In **%Battle** **Configs** have **"QConfig"** suffix. [[Naming]](#index-naming)

|  Namespace                                                  || Class                                        | Description                                      |
| :------------------------- | :------------------------------ | :------------------------------------------- | :----------------------------------------------- |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Game} | @cref{Battle.QSimulation.Game,BattleQConfig} | @copybrief Battle.QSimulation.Game.BattleQConfig |

<br/>

## Specs {#page-simulation-specs}

[Quantum Specs🡵] are **C# classes** that are used as data containers.  
In **%Battle** **Specs** have **"QSpec"** suffix. [[Naming]](#index-naming)

|  Namespace                                                        || Class                                                             | Description                                                      |
| :------------------------- | :------------------------------------ | :---------------------------------------------------------------- | :--------------------------------------------------------------- |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Diamond}    | @cref{Battle.QSimulation.Diamond,BattleDiamondQSpec}              | @copybrief Battle.QSimulation.Diamond.BattleDiamondQSpec         |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Game}       | @cref{Battle.QSimulation.Game,BattleArenaQSpec}                   | @copybrief Battle.QSimulation.Game.BattleArenaQSpec              |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Projectile} | @cref{Battle.QSimulation.Projectile,BattleProjectileQSpec}        | @copybrief Battle.QSimulation.Projectile.BattleProjectileQSpec   |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,SoulWall}   | @cref{Battle.QSimulation.SoulWall,BattleSoulWallQSpec}            | @copybrief Battle.QSimulation.SoulWall.BattleSoulWallQSpec       |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Player}     | @cref{Battle.QSimulation.SoulWall,BattlePlayerQSpec}              | @copybrief Battle.QSimulation.Player.BattlePlayerQSpec           |

<br/>

## Other {#page-simulation-other}

Miscellaneous classes and structs that don't belong in other categories.

|  Namespace                                                  || Class                                            | Description                                          |
| :------------------------- | :------------------------------ | :----------------------------------------------- | :--------------------------------------------------- |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Game} | @cref{Battle.QSimulation.Game,BattleAltzoneLink} | @copybrief Battle.QSimulation.Game.BattleAltzoneLink |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Game} | @cref{Battle.QSimulation.Game,BattleParameters}  | @copybrief Battle.QSimulation.Game.BattleParameters  |


<br/>

[Quantum Systems🡵]:    https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems
[Quantum Components🡵]: https://doc.photonengine.com/quantum/current/manual/quantum-ecs/dsl
[Quantum Component🡵]:  https://doc.photonengine.com/quantum/current/manual/quantum-ecs/dsl
[Quantum Specs🡵]:      https://doc.photonengine.com/quantum/current/manual/assets/assets-simulation
[Quantum Events🡵]:     https://doc.photonengine.com/quantum/current/manual/quantum-ecs/game-events
[Quantum Signals🡵]:    https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems#signals
