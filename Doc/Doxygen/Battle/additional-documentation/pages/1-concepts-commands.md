## Custom %Quantum Commands {#page-concepts-commands}

In %Battle we have a custom implementation of [Quantum Commands🡵](https://doc.photonengine.com/quantum/current/manual/commands)  
All [{BattleQCommands}](#page-concepts-commands-battle-qcommand) extend the [{BattleCommand}](#page-concepts-commands-battle-command-base) base class.  
@ref BattleCommands.cs contains all %Quantum commands in %Battle.  

### BattleCommand (base class) {#page-concepts-commands-battle-command-base}

@cref{Battle.QSimulation.Game,BattleCommand} contains @cref{Battle.QSimulation.Game.BattleCommand,Type} enum which contains all %Battle commands.  
[{BattleQCommands}](#page-concepts-commands-battle-qcommand) can be **Fetched** using @ref Battle.QSimulation.Game.BattleCommand.GetCommand "GetCommand" for handling the **command**.

### BattleQCommand {#page-concepts-commands-battle-qcommand}

To make a new command, add a new class in @ref BattleCommands.cs file. It should:  
- Inherit from @cref{Battle.QSimulation.Game,BattleCommand} class  
- Be named **%Battle(commandname)QCommand**  
- Have a <b>public override Type @ref %BattleCommandType => Type.(commandname)</b> property  
- Have a **public override void Serialize(Bitstream stream)** method
  - Method can be empty if not needed, but it is required to be implemented

The command does not necessarily have to do anything itself, since it can be done in the part of the code where receiving the command is polled.  

All new commands must be added to @ref CommandSetup.User.cs, and @ref Battle.QSimulation.Game.BattleCommand.Type "Type" enum at the top of the @ref BattleCommands.cs file.  

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