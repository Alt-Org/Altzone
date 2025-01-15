# CharacterSpec

This file contains instructions for `CharacterSpec` (_ScriptableObject_) usage, change and approval process.

`CharacterSpec` is the single source of truth for player character for the Altzone UNITY game.  
Changes to `CharacterSpec.cs` file must be done trough change and approval processes described here.

## Related files

Package `Altzone.Scripts.Config.ScriptableObjects` contains two _ScriptableObject_ classes for this:
- CharacterSpec, data, bindings, references, attributes, etc. for single player character _instance_, and
- PlayerCharacters, list of `CharacterSpec`s available for the game at runtime.
- `Resources/PlayerCharacters.asset` for the bookkeeping.
- `Resources/Characters` folder for actual `CharacterSpec` instances.

## Usage (public API)

`PlayerCharacters` contains getters for available `CharacterSpec`s.

### Characters

Gets all `CharacterSpec`s available for current configuration.

### GetCharacter (by id)

Gets `CharacterSpec` by its id.

## CharacterSpec semantic overview

`CharacterSpec` some attributes (properties) for its bookkeeping etc. features and rest is the content for the game itself.

### Bookkeeping and infra

- Id, `CharacterSpec` by its id which is given externally
- IsApproved, flag to separate approved `CharacterSpec`s from those that are not to be used in production yet

### Content

`CharacterSpec` content defines the features that are available or visible in the game for the player (end user).

## CharacterSpec content by category

_tbd_

## CharacterSpec change management process

_tbd_

## CharacterSpec approval process

_tbd_
