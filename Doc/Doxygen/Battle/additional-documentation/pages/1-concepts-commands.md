## Custom %Quantum Commands {#page-concepts-commands}

<br/>

### Custom %Quantum Commands {#page-concepts-commands}

@ref BattleCommands.cs contains @ref Battle.QSimulation.Game.BattleCommand "BattleCommand" class that should be used to make all custom %Quantum commands in battle.  
It also contains all the custom commands.

All new commands must be added to @ref CommandSetup.User.cs and @ref Battle.QSimulation.Game.BattleCommand.Type "Type" enum at the top of the @ref BattleCommands.cs file.  

See [Quantum Commands documentation🡵](https://doc.photonengine.com/quantum/current/manual/commands) for more info.

**C# code example**
```cs
public class BattleExampleQCommand : BattleCommand
{
    public override Type BattleCommandType => Type.Example;

    public override void Serialize(BitStream stream)
    {
        //...
    }
    // ...
}
```