# View {#page-view}

## Namespaces {#page-view-namespaces}

|  Namespace                                         || Description                       |
| :------------------ | :---------------------------- | :-------------------------------- |
| @cref{Battle.View}                                 || @copybrief Battle.View            |
| @crefd{Battle.View} | @cref{Battle.View,Game}       | @copybrief Battle.View.Game       |
| @crefd{Battle.View} | @cref{Battle.View,Player}     | @copybrief Battle.View.Player     |
| @crefd{Battle.View} | @cref{Battle.View,SoulWall}   | @copybrief Battle.View.SoulWall   |
| @crefd{Battle.View} | @cref{Battle.View,Projectile} | @copybrief Battle.View.Projectile |
| @crefd{Battle.View} | @cref{Battle.View,Diamond}    | @copybrief Battle.View.Diamond    |
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

**View Controllers** are non-deterministic **Unity View/Visual** logic for **%Battle**.  
**There are 2 types** of **View Controllers**, **Global View Controllers** and **View Controllers** that are attached to **%Quantum Entities**.  
**Global View Controllers**'s methods are called by **GameViewController** and **entity** attached **View Controllers** react to [Quantum Events🡵].  
**View Controllers** are used to change **entities**'s sprites, animations, effects, etc.  
In **%Battle** **View Controllers** have **"ViewController"** suffix. [[Naming]](#index-naming)

|  Namespace                                          || Class                                                              | Description                                                            |
| :------------------ | :----------------------------- | :----------------------------------------------------------------- | :--------------------------------------------------------------------- |
| @crefd{Battle.View} | @crefd{Battle.View,Game}       | @cref{Battle.View.Game,BattleGameViewController}                   | @copybrief Battle.View.Game.BattleGameViewController                   |
| @crefd{Battle.View} | @crefd{Battle.View,Game}       | @cref{Battle.View.Game,BattleGridViewController}                   | @copybrief Battle.View.Game.BattleGridViewController                   |
|                                                                                                                                                                                                 ||||
| @crefd{Battle.View} | @crefd{Battle.View,Player}     | @cref{Battle.View.Player,BattlePlayerViewController}               | @copybrief Battle.View.Player.BattlePlayerViewController               |
| @crefd{Battle.View} | @crefd{Battle.View,Player}     | @cref{Battle.View.Player,BattlePlayerClassBaseViewController}      | @copybrief Battle.View.Player.BattlePlayerClassBaseViewController      |
| @crefd{Battle.View} | @crefd{Battle.View,Player}     | @cref{Battle.View.Player,BattlePlayerClassNoneViewController}      | @copybrief Battle.View.Player.BattlePlayerClassNoneViewController      |
| @crefd{Battle.View} | @crefd{Battle.View,Player}     | @cref{Battle.View.Player,BattlePlayerClassProjectorViewController} | @copybrief Battle.View.Player.BattlePlayerClassProjectorViewController |
|                                                                                                                                                                                                 ||||
| @crefd{Battle.View} | @crefd{Battle.View,SoulWall}   | @cref{Battle.View.SoulWall,BattleSoulWallViewController}           | @copybrief Battle.View.SoulWall.BattleSoulWallViewController           |
|                                                                                                                                                                                                 ||||
| @crefd{Battle.View} | @crefd{Battle.View,Projectile} | @cref{Battle.View.Projectile,BattleProjectileViewController}       | @copybrief Battle.View.Projectile.BattleProjectileViewController       |
|                                                                                                                                                                                                 ||||
| @crefd{Battle.View} | @crefd{Battle.View,Diamond}    | @cref{Battle.View.Diamond,BattleDiamondViewController}             | @copybrief Battle.View.Diamond.BattleDiamondViewController             |
|                                                                                                                                                                                                 ||||
| @crefd{Battle.View} | @crefd{Battle.View,Effect}     | @cref{Battle.View.Effect,BattleLightrayEffectViewController}       | @copybrief Battle.View.Effect.BattleLightrayEffectViewController       |
| @crefd{Battle.View} | @crefd{Battle.View,Effect}     | @cref{Battle.View.Effect,BattleScreenEffectViewController}         | @copybrief Battle.View.Effect.BattleScreenEffectViewController         |
|                                                                                                                                                                                                 ||||
| @crefd{Battle.View} | @crefd{Battle.View,Audio}      | @cref{Battle.View.Audio,BattleAudioViewController}                 | @copybrief Battle.View.Audio.BattleAudioViewController                 |
|                                                                                                                                                                                                 ||||
| @crefd{Battle.View}                                 || @cref{Battle.View,BattleStoneCharacterViewController}              | @copybrief Battle.View.BattleStoneCharacterViewController              |

<br/>

## Other Classes {#page-view-other}
Miscellaneous classes that don't belong in other categories.

|  Namespace                                          || Class                                                        | Description                                                      |
| :------------------ | :----------------------------- | :----------------------------------------------------------- | :--------------------------------------------------------------- |
| @crefd{Battle.View} | @crefd{Battle.View,Game}       | @cref{Battle.View.Game,BattleCamera}                         | @copybrief Battle.View.Game.BattleCamera                         |
| @crefd{Battle.View} | @crefd{Battle.View,Game}       | @cref{Battle.View.Game,BattleCameraTest}                     | @copybrief Battle.View.Game.BattleCameraTest                     |
|                                                                                                                                                                                     ||||
| @crefd{Battle.View} | @crefd{Battle.View,Player}     | @cref{Battle.View.Player,BattlePlayerInput}                  | @copybrief Battle.View.Player.BattlePlayerInput                  |
| None                                                || @cref{OnPointerDownButton}                                   | @copybrief OnPointerDownButton                                   |
|                                                                                                                                                                                     ||||
| @crefd{Battle.View}                                 || @cref{Battle.View,Utils}                                     | @copybrief Battle.View.Utils                                     |

<br/>

## UI {#page-view-ui}

**UI Scripts** handle **%Battle** **UI** functionality.  
In **%Battle** the **UI scripts** have a **"Ui"** prefix following the **"Battle"** prefix. [[Naming]](#index-naming)

|  Namespace                                          || Class                                                        | Description                                                      |
| :------------------ | :----------------------------- | :----------------------------------------------------------- | :--------------------------------------------------------------- |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiController}                     | @copybrief Battle.View.UI.BattleUiController                     |

<br/>

## UI Handler {#page-view-uihandler}

**UI Handlers** are scripts which handle **UI** functionality.  
They are attached to the **top level parent GameObject** of the @ref BattleUi.prefab and there is only ever one instance of a **UI Handler script**.  
The **handler scripts** also add listeners to call **UiInput** methods in @cref{Battle.View.Game,BattleGameViewController} when the local player gives an UI input.  
In **%Battle** the **UI Handlers** have a **"Handler"** suffix in addition to the **"Ui"** prefix. [[Naming]](#index-naming)

**BattleUiShared** prefabs can be found in the @dirref{Altzone,Assets/Altzone/Resources/Prefabs/BattleUiShared} directory, and the **Scripts** are in the @dirref{Altzone,Assets/Altzone/Scripts/BattleUiShared} directory.  
Every one of the **BattleUiShared** prefabs has either a @cref{Altzone.Scripts.BattleUiShared,BattleUiMovableElement}, @cref{Altzone.Scripts.BattleUiShared,BattleUiMovableJoystickElement} or @cref{Altzone.Scripts.BattleUiShared,BattleUiMultiOrientationElement} script, which allow setting the saved @cref{Altzone.Scripts.BattleUiShared,BattleUiMovableElementData}.  
@cref{Altzone.Scripts.BattleUiShared,BattleUiMovableElementData} holds the anchor and orientation information for each **BattleUiShared** prefab. It is serialized and deserialized in **SettingsCarrier** using **GetBattleUiMovableElementData** and **SetBattleUiMovableElementData** methods.  

|  Namespace                                          || Class                                                        | Description                                                      |
| :------------------ | :----------------------------- | :----------------------------------------------------------- | :--------------------------------------------------------------- |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiAnnouncementHandler}            | @copybrief Battle.View.UI.BattleUiAnnouncementHandler            |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiDiamondsHandler}                | @copybrief Battle.View.UI.BattleUiDiamondsHandler                |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiGameOverHandler}                | @copybrief Battle.View.UI.BattleUiGameOverHandler                |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiGiveUpButtonHandler}            | @copybrief Battle.View.UI.BattleUiGiveUpButtonHandler            |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiJoystickHandler}                | @copybrief Battle.View.UI.BattleUiJoystickHandler                |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiPlayerInfoHandler}              | @copybrief Battle.View.UI.BattleUiPlayerInfoHandler              |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiTimerHandler}                   | @copybrief Battle.View.UI.BattleUiTimerHandler                   |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiDebugOverlayHandler}            | @copybrief Battle.View.UI.BattleUiDebugOverlayHandler            |

<br/>

## UI Component {#page-view-uicomponent}

**UI Components** are helper scripts which can have multiple instances in **BattleUi** prefab's child **GameObjects**.  
In **%Battle** the **UI Components** have a **"Component"** suffix in addition to the **"Ui"** prefix. [[Naming]](#index-naming)

The **UI Component** scripts exist to make it easier to handle @dirref{Altzone,Assets/Altzone/Resources/Prefabs,BattleUiShared} prefabs which have several instances, for example @ref BattleUiPlayerInfo.prefab.  
The different instances of the duplicated prefab each have their own **GameObjects** which need to be accessed to set visual information or attach listeners.  

|  Namespace                                          || Class                                                        | Description                                                      |
| :------------------ | :----------------------------- | :----------------------------------------------------------- | :--------------------------------------------------------------- |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiCharacterButtonComponent}       | @copybrief Battle.View.UI.BattleUiCharacterButtonComponent       |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiJoystickComponent}              | @copybrief Battle.View.UI.BattleUiJoystickComponent              |
| @crefd{Battle.View} | @crefd{Battle.View,UI}         | @cref{Battle.View.UI,BattleUiPlayerInfoComponent}            | @copybrief Battle.View.UI.BattleUiPlayerInfoComponent            |

<br/>

[Quantum Events🡵]: https://doc.photonengine.com/quantum/current/manual/quantum-ecs/game-events
