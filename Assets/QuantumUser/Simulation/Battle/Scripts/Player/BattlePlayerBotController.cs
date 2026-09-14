/// @file BattlePlayerBotController.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerBotController} class which handles the bot AI logic and implements helper methods for handling bots.
/// </summary>

// System usings
using System.Linq;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Input = Quantum.Input;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// Handles the bot logic and implements helper methods for handling bots.
    /// </summary>
    ///
    /// [{Player Overview}](#page-concepts-player-overview)<br/>
    /// [{Player Simulation Code Overview}](#page-concepts-player-simulation-overview)
    ///
    /// Bot AI is handled by generating input in @cref{GetBotInput} that can be processed by the player like any other Input.
    /// Bot behavior and other spec settings are defined in @cref{BattlePlayerBotQSpec}.
    public static unsafe class BattlePlayerBotController
    {
        public struct BotInputData
        {
            public Input Input;
            public BattleCommand.Type CommandType;
            public BattleCommand CommandData;
        }

        /// <summary>
        /// Helper method to get characters for a bot.
        /// </summary>
        ///
        /// Characters are retrieved from @cref{BattlePlayerBotQSpec}.
        ///
        /// <param name="f">Current simulation frame.</param>
        ///
        /// <returns>Array of characters for a bot.</returns>
        public static BattleCharacterBase[] GetBotCharacters(Frame f)
        {
            BattlePlayerBotQSpec playerBotSpec = BattleQConfig.GetPlayerBotSpec(f);

            int[] selectedBotCharacters = new int[Constants.BATTLE_PLAYER_CHARACTER_COUNT];

            BattleCharacterBase[] botCharacters = new BattleCharacterBase[Constants.BATTLE_PLAYER_CHARACTER_COUNT];
            for (int i = 0; i < botCharacters.Length; i++)
            {
                int selectedCharacter;
                do
                {
                    selectedCharacter = f.RNG->Next(0, playerBotSpec.BotCharacterSelection.Length);
                } while (selectedBotCharacters.Contains(selectedCharacter));

                selectedBotCharacters[i] = selectedCharacter;
                botCharacters[i]         = playerBotSpec.BotCharacterSelection[selectedCharacter];
            }
            return botCharacters;
        }

        /// <summary>
        /// Handles bot AI logic and generates <see cref="Quantum.Input">Quantum Input</see>
        /// and/or <see cref="Battle.QSimulation.Game.BattleCommand">Battle Command</see>.
        /// </summary>
        ///
        /// Bot behavior:
        /// - Selects a random character on random intervals or if none is selected.
        /// - Character movement: Moves intentionally or "misclicks" based on @cref{Battle.QSimulation.Player,BattlePlayerBotQSpec.MissClickChance}.
        ///   - Intentional movement: Predicts and intercepts the projectile, with some inaccuracy based on @cref{Battle.QSimulation.Player,BattlePlayerBotQSpec.Inaccuracy}.
        ///   - Misclick: Performs a random movement.
        /// - Gives up when teammate gives up.
        ///
        /// Bot behavior spec settings is defined in @cref{BattlePlayerBotQSpec}.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">The player handle of the bot.</param>
        ///
        /// <returns><see cref="BotInputData"/> struct, which contains the generated inputs.</returns>
        public static BotInputData GetBotInput(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            BotInputData botInputData = new();

            BattlePlayerBotQSpec playerBotSpec = BattleQConfig.GetPlayerBotSpec(f);

            BattleMovementInputType movementInput = BattleMovementInputType.None;
            BattleGridPosition predictedGridPosition = new() { Col = 0, Row = 0 };

            bool hasCharacter = false;

            BattlePlayerEntityRef playerEntity;
            BattlePlayerDataQComponent* playerData = null;

            if (playerHandle.SelectedCharacterNumber != -1)
            {
                playerEntity = playerHandle.GetSelectedCharacterEntityRef(f);
                playerData = playerEntity.GetDataQComponent(f);

                hasCharacter = true;
            }

            //{ non-character logic

            // behavior: gives up when teammate gives up
            if (BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, playerHandle.Slot).GiveUpState && !playerHandle.GiveUpState)
            {
                botInputData.CommandType = BattleCommand.Type.GiveUp;
                botInputData.CommandData = new BattleGiveUpQCommand();
                return botInputData;
            }

            // behavior: selects a random character on random intervals or if none is selected
            if (hasCharacter && playerData->BotCharacterSwapTimerSec > FP._0)
            {
                playerData->BotCharacterSwapTimerSec -= f.DeltaTime;
            }
            else
            {
                int nextCharacter = hasCharacter
                    ? (playerHandle.SelectedCharacterNumber + f.RNG->NextInclusive(1, Constants.BATTLE_PLAYER_CHARACTER_COUNT - 1)) % Constants.BATTLE_PLAYER_CHARACTER_COUNT
                    : f.RNG->NextInclusive(0, Constants.BATTLE_PLAYER_CHARACTER_COUNT - 1);

                playerHandle.GetCharacterEntityRef(f, nextCharacter).GetDataQComponent(f)->BotCharacterSwapTimerSec =
                    f.RNG->NextInclusive(playerBotSpec.CharacterSwapTimeSecMin, playerBotSpec.CharacterSwapTimeSecMax);

                botInputData.CommandType = BattleCommand.Type.SwapCharacter;
                botInputData.CommandData = new BattleCharacterSwapQCommand
                {
                    CharacterNumber = nextCharacter
                };
                return botInputData;
            }

            //} non-character logic

            //{ character logic
            // behavior character movement: moves intentionally or "misclicks" based on BattlePlayerBotQSpec.MissClickChance.
            // - intentional movement: predicts and intercepts the projectile, with some inaccuracy based on BattlePlayerBotQSpec.Inaccuracy.
            // - misclick: performs a random movement.

            if (playerData->BotMovementCooldownSec > FP._0)
            {
                playerData->BotMovementCooldownSec -= f.DeltaTime;
            }
            else
            {
                movementInput = BattleMovementInputType.PositionTarget;
            }

            if (movementInput != BattleMovementInputType.None)
            {
                playerData->BotMovementCooldownSec = f.RNG->NextInclusive(playerBotSpec.MovementCooldownSecMin, playerBotSpec.MovementCooldownSecMax);
                ComponentFilter<BattleProjectileQComponent> projectiles = f.Filter<BattleProjectileQComponent>();
                if (projectiles.NextUnsafe(out EntityRef projectileEntity, out BattleProjectileQComponent* projectile))
                {
                    FPVector2 projectileDirection = projectile->Direction;
                    if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha ? projectileDirection.Y > 0 : projectileDirection.Y < 0) return botInputData;
                    FPVector2 projectilePosition = projectile->Position;
                    FP predictionTimeSec = playerBotSpec.LookAheadTimeSec;

                    FPVector2 predictedPosition = projectilePosition + projectileDirection * (projectile->Speed * predictionTimeSec);

                    FP missClick = f.RNG->Next();
                    if (missClick >= playerBotSpec.MissClickChance)
                    {
                        FP randomAngle = f.RNG->Next(0, FP.Pi * 2);
                        FP randomRadius = f.RNG->NextInclusive(0, playerBotSpec.Inaccuracy);

                        FPVector2 randomnessOffset = FPVector2.Rotate(FPVector2.Up, randomAngle) * randomRadius;
                        predictedPosition += randomnessOffset;

                        predictedGridPosition = BattleGridManager.WorldPositionToGridPosition(predictedPosition);
                    }
                    else
                    {
                        int playfieldStart;
                        int playfieldEnd;

                        if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha)
                        {
                            playfieldStart = BattleGridManager.TeamAlphaFieldStart;
                            playfieldEnd = BattleGridManager.TeamAlphaFieldEnd;
                        }
                        else
                        {
                            playfieldStart = BattleGridManager.TeamBetaFieldStart;
                            playfieldEnd = BattleGridManager.TeamBetaFieldEnd;
                        }

                        predictedGridPosition = new()
                        {
                            Row = f.RNG->NextInclusive(playfieldEnd, playfieldStart),
                            Col = f.RNG->NextInclusive(0, BattleGridManager.Columns - 1)
                        };
                    }

                    // clamp the TargetPosition inside sidebounds
                    predictedGridPosition.Col = Mathf.Clamp(predictedGridPosition.Col, 0, BattleGridManager.Columns - 1);

                    if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha)
                    {
                        predictedGridPosition.Row = Mathf.Clamp(
                            predictedGridPosition.Row,
                            BattleGridManager.TeamAlphaFieldStart + playerData->GridExtendBottom,
                            BattleGridManager.TeamAlphaFieldEnd - playerData->GridExtendTop
                        );
                    }
                    else
                    {
                        predictedGridPosition.Row = Mathf.Clamp(
                            predictedGridPosition.Row,
                            BattleGridManager.TeamBetaFieldStart + playerData->GridExtendBottom,
                            BattleGridManager.TeamBetaFieldEnd - playerData->GridExtendTop
                        );
                    }
                }
                else
                {
                    movementInput = BattleMovementInputType.None;
                }
            }

            botInputData.Input = new Input()
            {
                IsValid                       = true,
                MovementInput                 = movementInput,
                MovementDirectionIsNormalized = false,
                MovementGridPosition          = predictedGridPosition,
                MovementVector                = FPVector2.Zero,
                RotationInput                 = false,
                RotationValue                 = FP._0
            };

            return botInputData;

            //} character logic
        }
    }
}
