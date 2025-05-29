# View {#page-view}

## Namespaces {#page-view-namespaces}

|  Namespace                                         || Description                       |
| :------------------ | :---------------------------- | :-------------------------------- |
| @cref{Battle.View}                                 || @copybrief Battle.View            |
| @crefd{Battle.View} | @cref{Battle.View,Game}       | @copybrief Battle.View.Game       |
| @crefd{Battle.View} | @cref{Battle.View,Player}     | @copybrief Battle.View.Player     |
| @crefd{Battle.View} | @cref{Battle.View,SoulWall}   | @copybrief Battle.View.SoulWall   |
| @crefd{Battle.View} | @cref{Battle.View,Projectile} | @copybrief Battle.View.Projectile |
| @crefd{Battle.View} | @cref{Battle.View,UI}         | @copybrief Battle.View.UI         |
| @crefd{Battle.View} | @cref{Battle.View,Effect}     | @copybrief Battle.View.Effect     |
| @crefd{Battle.View} | @cref{Battle.View,Audio}      | @copybrief Battle.View.Audio      |


@bigtext{[[Namespace Summary]](#index-namespace-summary)}

<br/>

## Directories {#page-view-directories}

|  Path                                                                                                                                  ||| Description                                                                                       |
| :---------------------------------- | :--------------------------------------- | :------------------------------------------------------ | :------------------------------------------------------------------------------------------------ |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,View}                                                          || %Battle View Logic Directory.<br/>Contains non-deterministic Unity View/Visual logic for %Battle. |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,View} | @dirref{Altzone/Assets/QuantumUser/View,Battle/Scripts} | View %Battle C# Script                                                                            |
| @dirref{Altzone/Assets/QuantumUser} | @dirref{Altzone/Assets/QuantumUser,View} | @dirref{Altzone/Assets/QuantumUser/View,Generated}      | Generated View scripts                                                                            |

@bigtext{[[File Summary]](#index-file-summary)}

<br/>

## View Controllers {#page-view-controllers}

View Controllers are non-deterministic Unity View/Visual logic for %Battle. They are attached to entities and react to [Quantum Events🡵].  
View Controllers are used to change entities's sprites, animations, effects, etc.  
In %Battle View Controllers have ViewController suffix. [[Naming]](#index-naming)

|  Namespace                                          || Class                                                        | Description                                                      |
| :------------------ | :----------------------------- | :----------------------------------------------------------- | :--------------------------------------------------------------- |
| @crefd{Battle.View} | @crefd{Battle.View,Game}       | @cref{Battle.View.Game,BattleGameViewController}             | @copybrief Battle.View.Game.BattleGameViewController             |
| @crefd{Battle.View} | @crefd{Battle.View,Game}       | @cref{Battle.View.Game,BattleGridViewController}             | @copybrief Battle.View.Game.BattleGridViewController             |
|                                                                                                                                                                                     ||||
| @crefd{Battle.View} | @crefd{Battle.View,Player}     | @cref{Battle.View.Player,BattlePlayerViewController}         | @copybrief Battle.View.Player.BattlePlayerViewController         |
|                                                                                                                                                                                     ||||
| @crefd{Battle.View} | @crefd{Battle.View,SoulWall}   | @cref{Battle.View.SoulWall,BattleSoulWallViewController}     | @copybrief Battle.View.SoulWall.BattleSoulWallViewController     |
|                                                                                                                                                                                     ||||
| @crefd{Battle.View} | @crefd{Battle.View,Projectile} | @cref{Battle.View.Projectile,BattleProjectileViewController} | @copybrief Battle.View.Projectile.BattleProjectileViewController |
|                                                                                                                                                                                     ||||
| @crefd{Battle.View} | @crefd{Battle.View,Effect}     | @cref{Battle.View.Effect,BattleScreenEffectViewController}   | @copybrief Battle.View.Effect.BattleScreenEffectViewController   |
|                                                                                                                                                                                     ||||
| @crefd{Battle.View} | @crefd{Battle.View,Audio}      | @cref{Battle.View.Audio,BattleSoundFXViewController}         | @copybrief Battle.View.Audio.BattleSoundFXViewController         |

<br/>

## Other Classes {#page-view-other}

|  Namespace                                          || Class                                                        | Description                                                      |
| :------------------ | :----------------------------- | :----------------------------------------------------------- | :--------------------------------------------------------------- |
| @crefd{Battle.View} | @crefd{Battle.View,Game}       | @cref{Battle.View.Game,BattleCamera}                         | @copybrief Battle.View.Game.BattleCamera                         |
|                                                                                                                                                                                     ||||
| @crefd{Battle.View} | @crefd{Battle.View,Player}     | @cref{Battle.View.Player,BattlePlayerInput}                  | @copybrief Battle.View.Player.BattlePlayerInput                  |

<br/>

## UI {#page-view-ui}

|  Namespace                                          || Class                                                        | Description                                                      |
| :------------------ | :----------------------------- | :----------------------------------------------------------- | :--------------------------------------------------------------- |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiController}                     | @copybrief Battle.View.UI.BattleUiController                     |

<br/>

## UI Handler {#page-view-uihandler}

|  Namespace                                          || Class                                                        | Description                                                      |
| :------------------ | :----------------------------- | :----------------------------------------------------------- | :--------------------------------------------------------------- |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiAnnouncementHandler}            | @copybrief Battle.View.UI.BattleUiAnnouncementHandler            |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiDebugStatsOverlayHandler}       | @copybrief Battle.View.UI.BattleUiDebugStatsOverlayHandler       |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiDiamondsHandler}                | @copybrief Battle.View.UI.BattleUiDiamondsHandler                |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiGameOverHandler}                | @copybrief Battle.View.UI.BattleUiGameOverHandler                |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiGiveUpButtonHandler}            | @copybrief Battle.View.UI.BattleUiGiveUpButtonHandler            |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiPlayerInfoHandler}              | @copybrief Battle.View.UI.BattleUiPlayerInfoHandler              |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiTimerHandler}                   | @copybrief Battle.View.UI.BattleUiTimerHandler                   |

<br/>

## UI Component {#page-view-uicomponent}

|  Namespace                                          || Class                                                        | Description                                                      |
| :------------------ | :----------------------------- | :----------------------------------------------------------- | :--------------------------------------------------------------- |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiCharacterButtonComponent}       | @copybrief Battle.View.UI.BattleUiCharacterButtonComponent       |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiPlayerInfoComponent}            | @copybrief Battle.View.UI.BattleUiPlayerInfoComponent            |

<br/>

[Quantum Events🡵]:     https://doc.photonengine.com/quantum/current/manual/quantum-ecs/game-events
