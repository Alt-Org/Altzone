# Simulation {#page-simulation}

## Namespaces {#page-simulation-namespaces}

|  Namespace                                                       || Description                              |
| :------------------------- | :----------------------------------- | :--------------------------------------- |
| @cref{Battle.QSimulation}                                        || @copybrief Battle.QSimulation            |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,Diamond}    | @copybrief Battle.QSimulation.Diamond    |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,Game}       | @copybrief Battle.QSimulation.Game       |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,Player}     | @copybrief Battle.QSimulation.Player     |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,SoulWall}   | @copybrief Battle.QSimulation.SoulWall   |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,Projectile} | @copybrief Battle.QSimulation.Projectile |
| @crefd{Battle.QSimulation} | @cref{Battle.QSimulation,Goal}       | @copybrief Battle.QSimulation.Goal       |

[[Namespace Summary]](#index-namespace-summary)

## Directories {#page-simulation-directories}

|  Path                                                                                                                                              ||| Description                                                                                      |
| :---------------------------------- | :--------------------------------------------- | :------------------------------------------------------------ | :----------------------------------------------------------------------------------------------- |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation}                                                                || Game Simulation Logic Directory.<br/>Contains deterministic %Quantum Simulation logic and state. |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation} | @dirref{Altzone/Assets/QuantumUser/Simulation,Battle/Scripts} | Simulation %Battle C# Script                                                                     |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation} | Battle/Qtn/                                                   | Simulation %Battle QTN files                                                                     |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,Simulation} | @dirref{Altzone/Assets/QuantumUser/Simulation,Generated}      | Generated Simulation scripts                                                                     |

[[File Summary]](#index-file-summary)

## Systems {#page-simulation-systems}

[Quantum Systems🡵] are C# classes that handle game logic.
In %Battle [Quantum Systems🡵] have QSystem suffix. [[Naming]](#index-naming)

|  Namespace                                                        || Class                                                               | Description                                                             |
| :------------------------- | :------------------------------------ | :------------------------------------------------------------------ | :---------------------------------------------------------------------- |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Diamond}    | @cref{Battle.QSimulation.Diamond,BattleDiamondQSystem}              | @copybrief Battle.QSimulation.Diamond.BattleDiamondQSystem                 |
|                                                                                                                                                                                                                 ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Game}       | @cref{Battle.QSimulation.Game,BattleGameControlQSystem}             | @copybrief Battle.QSimulation.Game.BattleGameControlQSystem             |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Game}       | @cref{Battle.QSimulation.Game,BattleCollisionQSystem}               | @copybrief Battle.QSimulation.Game.BattleCollisionQSystem               |
|                                                                                                                                                                                                                 ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Player}     | @cref{Battle.QSimulation.Player,BattlePlayerJoinQSystem}            | @copybrief Battle.QSimulation.Player.BattlePlayerJoinQSystem            |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Player}     | @cref{Battle.QSimulation.Player,BattlePlayerMovementQSystem}        | @copybrief Battle.QSimulation.Player.BattlePlayerMovementQSystem        |
|                                                                                                                                                                                                                 ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,SoulWall}   | @cref{Battle.QSimulation.SoulWall,BattleSoulWallQSystem}            | @copybrief Battle.QSimulation.SoulWall.BattleSoulWallQSystem            |
|                                                                                                                                                                                                                 ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Projectile} | @cref{Battle.QSimulation.Projectile,BattleProjectileSpawnerQSystem} | @copybrief Battle.QSimulation.Projectile.BattleProjectileSpawnerQSystem |
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Projectile} | @cref{Battle.QSimulation.Projectile,BattleProjectileQSystem}        | @copybrief Battle.QSimulation.Projectile.BattleProjectileQSystem        |
|                                                                                                                                                                                                                 ||||
| @crefd{Battle.QSimulation} | @crefd{Battle.QSimulation,Goal}       | @cref{Battle.QSimulation.Goal,BattleGoalQSystem}                    | @copybrief Battle.QSimulation.Goal.BattleGoalQSystem                    |

## Components {#page-simulatio-components}

[Quantum Components🡵] are C# structs that are generated from qtn files which can be attached to %Quantum Entities.
In %Battle [Quantum Components🡵] have QComponent suffix. [[Naming]](#index-naming)

[Quantum Systems🡵]:    https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems
[Quantum Components🡵]: https://doc.photonengine.com/quantum/current/manual/quantum-ecs/dsl
