# %View Registry {#page-concepts-view-registry}

<br/>

## View Registry {#page-concepts-view-registry-view-registry}

The @cref{Battle.View,BattleViewRegistry} handles mapping **%Quantum entities** to any **Objects** related to the entities.<br/>
The point of this class is to link **%Quantum entities** to each other on the view side of Quantum.<br/>
**Objects** are mapped to a given **%Quantum entity** using @cref{Battle.View.BattleViewRegistry,Register}.<br/>
**Objects** are retrieved using @cref{Battle.View.BattleViewRegistry,GetObject<T>} or @cref{Battle.View.BattleViewRegistry,GetObjects<T>} depending on if multiple objects need to be retrieved or not.<br/>
If an **Object** needs another **Object** related to a different **%Quantum entity** to function and it isnt known if the object has been registered to this system yet or not, the @cref{Battle.View.BattleViewRegistry,WhenRegistered} method needs to be used. <br/>