## Custom %Quantum Commands {#page-concepts-commands}

<br/>

### Custom %Quantum Commands {#page-concepts-commands}

@ref BattleCommands.cs contains @ref Battle.QSimulation.Game.BattleCommand "BattleCommand" class That should be used to make all custom %Quantum commands in battle.  
It also contains all the custom commands.

All new commands must be added to @ref CommandSetup.User.cs and @ref Battle.QSimulation.Game.BattleCommand.Type "Type" enum at the top of the file.

**C# code example**
```cs
public class BattleExampleQCommand : BattleCommand
{
    public override Type BattleCommandType => Type.Example;

    public override void Serialize(BitStream stream) { //... }
  // ...
}
```