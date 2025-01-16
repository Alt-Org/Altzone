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
- [Pelihahmojen Kuvaukset](https://docs.google.com/presentation/d/1LAQUyKSdMetxEPYzbhQWsciiImjf5fNKYR6QGETuVq8/edit#slide=id.g3237ce6b338_0_35) (
  Google Slides)

## Usage (public API)

`PlayerCharacters` contains getters for available `CharacterSpec`s.

### Characters

Gets all `CharacterSpec`s available for current configuration.

### GetCharacter (by id)

Gets `CharacterSpec` by its id.

## CharacterSpec semantic overview

`CharacterSpec` some attributes (properties) for its bookkeeping etc. features and rest is the content for the game
itself.

### Bookkeeping and infra

- Id, `CharacterSpec` by its id which is given externally
- IsApproved, flag to separate approved `CharacterSpec`s from those that are not to be used in production yet

### Content

`CharacterSpec` content defines the features that are available or visible in the game for the player (end user).

## CharacterSpec content by category

`CharacterSpec` content can be divided on 2 broad categories.

## General

### General attributes

General attributes can be textual content for player character backstory or some flavour texts.  
Most probably only textual content is required here.

### General graphical assets and prefabs

Everything else but Battle graphic assets belongs here.

## Battle

### Battle attributes

Battle attributes are those that are used in Battle game itself.  
These are typically integer or floating point values.

### Battle graphical assets and prefabs

## CharacterSpec change management and approval process

Changes to `CharacterSpec` should use [GitHub PR process](https://docs.github.com/en/pull-requests).

Changes to `CharacterSpec` should go trough following stages before approval:

- proposal for a change
- discussion is the change required and proper to implement
- evaluation how this change affects codebase, graphics pipeline or other important parts
- finally decision that the change can be implemented
- the change is approved and it is documented in `CharacterSpec` change history

## CharacterSpec change history

| Date       | Notes                                     |
|------------|-------------------------------------------|
| 2025-01-14 | Initial commit                            |
| 2025-01-16 | Add Battle attributes and #region markers |
