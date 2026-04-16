# BattleSpriteSheet {#page-concepts-battle-sprite-sheet}

<br/>

## SpriteSheetMap {#page-concepts-battle-sprite-sheet-sprite-sheet-map}

A SpriteSheetMap is a struct that needs to implement the @cref{Battle.View,IBattleSpriteSheetMap} interface, wrap an Enum and give methods to handle said Enum.  
Methods that need to be implemented are at least conversions between the SpriteSheetMap and the wrapped Enum, and a conversion from int to SpriteSheetMap.  
The @cref{Battle.View,IBattleSpriteSheetMap} is an interface that includes instance related methods that allow a SpriteSheetMap to be used via polymorphism.  
The point of this structure is to be able to 
