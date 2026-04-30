## Custom %Quantum Commands {#page-concepts-commands}

In %Battle we have a custom implementation of [Quantum Commands🡵](https://doc.photonengine.com/quantum/current/manual/commands)  
All [{BattleQCommands}](#page-concepts-commands-battle-qcommand) extend the [{BattleCommand}](#page-concepts-commands-battle-command-base) base class.  
@ref BattleCommands.cs contains all %Quantum commands in %Battle.  
All new commands must be added to @ref CommandSetup.User.cs, and @ref Battle.QSimulation.Game.BattleCommand.Type "Type" enum at the top of the @ref BattleCommands.cs file.  

### BattleCommand (base class) {#page-concepts-commands-battle-command-base}

@cref{Battle.QSimulation.Game,BattleCommand} contains @cref{Battle.QSimulation.Game.BattleCommand,Type} enum which contains all %Battle commands.  
[{BattleQCommands}](#page-concepts-commands-battle-qcommand) and their type can be retrieved using @ref Battle.QSimulation.Game.BattleCommand.GetCommand "GetCommand"

[{BattleQCommands}](#page-concepts-commands-battle-qcommand) can be **Fetched** using @ref Battle.QSimulation.Game.BattleCommand.GetCommand "GetCommand" for handling the **command**.

Use @ref Battle.QSimulation.Game.BattleCommand.GetCommand "GetCommand" to get data about the command.

### BattleQCommand {#page-concepts-commands-battle-qcommand}

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