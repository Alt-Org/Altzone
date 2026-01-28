/// @file BattlePlayerBotController.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerBotController} class which handles the bot AI logic and implements helper methods for handling bots.
/// </summary>

// System usings
using System.Collections.Generic;

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

            List<int> selectedBotCharacters = new List<int>();

            BattleCharacterBase[] botCharacters = new BattleCharacterBase[Constants.BATTLE_PLAYER_CHARACTER_COUNT];
            for (int i = 0; i < botCharacters.Length; i++)
            {            
                int selectedCharacter;
                do
                {
                    selectedCharacter = f.RNG->Next(0, playerBotSpec.BotCharacterSelection.Length);
                } while (selectedBotCharacters.Contains(selectedCharacter));

                selectedBotCharacters.Add(selectedCharacter);
                botCharacters[i] = playerBotSpec.BotCharacterSelection[selectedCharacter];             
            }
            selectedBotCharacters.Clear();
            return botCharacters;
        }

        /// <summary>
        /// Handles bot AI by predicting the projectile and generating the input for a bot.
        /// </summary>
        ///
        /// Bot behavior spec settings is defined in @cref{BattlePlayerBotQSpec}.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="isInPlay">Bool to check if bot is in play.</param>
        /// <param name="playerData">Pointer to player's BattlePlayerDataQComponent.</param>
        /// <param name="outBotInput">Pointer to where bot's %Quantum Input will be written.</param>
        public static void GetBotInput(Frame f, bool isInPlay, BattlePlayerDataQComponent* playerData, Input* outBotInput)
        {
            BattlePlayerBotQSpec playerBotSpec = BattleQConfig.GetPlayerBotSpec(f);

            BattleMovementInputType movementInput         = BattleMovementInputType.None;
            BattleGridPosition      predictedGridPosition = new() { Col = 0, Row = 0 };

            if (isInPlay)
            {
                if (playerData->MovementCooldownSec > FP._0)
                {
                    playerData->MovementCooldownSec -= f.DeltaTime;
                }
                else
                {
                    movementInput = BattleMovementInputType.PositionTarget;
                }
            }

            if (movementInput != BattleMovementInputType.None)
            {
                playerData->MovementCooldownSec = f.RNG->NextInclusive(playerBotSpec.MovementCooldownSecMin, playerBotSpec.MovementCooldownSecMax); ;
                ComponentFilter<BattleProjectileQComponent> projectiles = f.Filter<BattleProjectileQComponent>();
                if (projectiles.NextUnsafe(out EntityRef projectileEntity, out BattleProjectileQComponent* projectile))
                {
                    FPVector2 projectileDirection = projectile->Direction;
                    if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha ? projectileDirection.Y > 0 : projectileDirection.Y < 0) return;
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

            *outBotInput = new Input()
            {
                IsValid                       = true,
                MovementInput                 = movementInput,
                MovementDirectionIsNormalized = false,
                MovementPositionTarget        = predictedGridPosition,
                MovementPositionMove          = FPVector2.Zero,
                MovementDirection             = FPVector2.Zero,
                RotationInput                 = false,
                RotationValue                 = FP._0,
                PlayerCharacterNumber         = -1,
                GiveUpInput                   = false,
                AbilityActivate               = false
            };
        }
    }
}
