# Sprite Sheet Handling {#page-concepts-battle-sprite-sheet}

The **%Battle Sprite Sheets** allow for the handling of full **Spritesheets** as just one unit. They also allow defining a **Spritesheet's** structure
using a [{BattleSpriteSheetMap}](#page-concepts-battle-sprite-sheet-sprite-sheet-map).  
The serializable [{BattleSpriteSheet}](#page-concepts-battle-sprite-sheet-sprite-sheet) struct handles storing a **Spritesheet** simply as an array. It also has a custom property drawer.

<br/>

## BattleSpriteSheet (struct) {#page-concepts-battle-sprite-sheet-sprite-sheet}

The serializable @cref{Battle.View,BattleSpriteSheet} struct contains a spritesheet as an **Sprite Array**.  
The @cref{Battle.View,BattleSpriteSheet} can be validated using a [{BattleSpriteSheetMap}](#page-concepts-battle-sprite-sheet-sprite-sheet-map).  
A specific sprite can be fetched from the **Sprite Array** using the @clink{GetSprite:Battle.View.BattleSpriteSheet.GetSprite<T>(T)}
method with a specified [{BattleSpriteSheetMap}](#page-concepts-battle-sprite-sheet-sprite-sheet-map).

The @cref{Battle.View,BattleSpriteSheet} also has a **CustomPropertyDrawer** (@cref{Battle.View,BattleSpriteSheetDrawer}) for loading and displaying the **Spritesheet** in the inspector.  
Using a sprite from the **Spritesheet** as a reference, the @cref{Battle.View,BattleSpriteSheetDrawer} loads and sorts the full **Spritesheet** into the **Sprite Array**.

<br/>

## BattleSpriteSheetMap {#page-concepts-battle-sprite-sheet-sprite-sheet-map}

A **%SpriteSheetMap** is a struct that defines the structure of a [{BattleSpriteSheet}](#page-concepts-battle-sprite-sheet-sprite-sheet) by:
- Wrapping an Enum that defines a [{BattleSpriteSheet}](#page-concepts-battle-sprite-sheet-sprite-sheet)'s structure, giving each sprite index a name.
- Defining the expected sprite **Count** as a constant.
- Implementing the @cref{Battle.View,IBattleSpriteSheetMap} interface that defines methods that allow instances of a **%SpriteSheetMap** to be used by [{BattleSpriteSheet}](#page-concepts-battle-sprite-sheet-sprite-sheet) via polymorphism.
- Implementing methods for handling the **%SpriteSheetMap**.
    - Implicit cast operators for converting a **%SpriteSheetMap** into the wrapped **Enum** and vice versa,
    as well as a `public static %SpriteSheetMap FromInt(int index)` method for converting int to **%SpriteSheetMap**.
    - A `public static bool Validate(BattleSpriteSheet spriteSheet)` method for validating the given
    [{BattleSpriteSheet}](#page-concepts-battle-sprite-sheet-sprite-sheet) using the expected sprite **Count**.

A **%SpriteSheetMap** can be used statically for checking if a [{BattleSpriteSheet}](#page-concepts-battle-sprite-sheet-sprite-sheet) is valid,
meaning that it has the correct number of sprites according to the **%SpriteSheetMap**.  
An instance of a **%SpriteSheetMap** works as an **Enum/MapValue** for referencing individual sprites in a [{BattleSpriteSheet}](#page-concepts-battle-sprite-sheet-sprite-sheet).