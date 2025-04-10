# Simulation {#page-simulation}

## Namespaces {#page-simulation-namespaces}

Battle.QSimulation

## Directories {#page-simulation-directories}

|  Path                                                     ||| Description                  |
| :-------------------------- | :---------- | :-------------- | :--------------------------- |
| Altzone/Assets/QuantumUser/ | Simulation/ |                 | Simulation code              |
| Altzone/Assets/QuantumUser/ | Simulation/ | Battle/Scripts/ | Simulation %Battle C# Script |
| Altzone/Assets/QuantumUser/ | Simulation/ | Battle/QTN/     | Simulation %Battle QTN files |
| Altzone/Assets/QuantumUser/ | Simulation/ | Generated/      | Generated Simulation scripts |

## Systems {#page-simulation-systems}

[Quantum Systems](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) are c# classes that handle game logic.  
In %Battle [Quantum Systems](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) have QSystem sufix. [[Naming]](#index-naming)

- Battle.QSimulation.Game
  - [BattleGameControlQSystem](#Battle.QSimulation.Game.BattleGameControlQSystem)
  - [BattleCollisionQSystem](#Battle.QSimulation.Game.BattleCollisionQSystem)
- Battle.QSimulation.Player
  - [BattlePlayerJoinQSystem](#Battle.QSimulation.Player.BattlePlayerJoinQSystem) 
  - [BattlePlayerMovementQSystem](#Battle.QSimulation.Player.BattlePlayerMovementQSystem)
- Battle.QSimulation.SoulWall
  - [BattleSoulWallQSystem](#Battle.QSimulation.SoulWall.BattleSoulWallQSystem)
- Battle.QSimulation.Projectile
  - [BattleProjectileSpawnerQSystem](#Battle.QSimulation.Projectile.BattleProjectileSpawnerQSystem)
  - [BattleProjectileQSystem](#Battle.QSimulation.Projectile.BattleProjectileQSystem)
- Battle.QSimulation.Goal
  - [BattleGoalQSystem](#Battle.QSimulation.Goal.BattleGoalQSystem)

## Components {#page-simulatio-components}

[Quantum Components](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/dsl) are c# structs that are generated from qtn files which can be attached to Quantum Entities.  
In %Battle [Quantum Components](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/dsl) have QComponent sufix. [[Naming]](#index-naming)
