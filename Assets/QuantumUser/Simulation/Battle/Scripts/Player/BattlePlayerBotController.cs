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
    public static unsafe class BattlePlayerBotController
    {
        public static BattleCharacterBase[] GetBotCharacters()
        {
            BattleCharacterBase[] botCharacters = new BattleCharacterBase[Constants.BATTLE_PLAYER_CHARACTER_COUNT];
            for (int i = 0; i < botCharacters.Length; i++)
            {
                botCharacters[i] = new()
                {
                    Id = 0,
                    Class = (int)BattlePlayerCharacterClass.None,
                    Stats = new()
                    {
                        Hp = 15,
                        Speed = 10,
                        CharacterSize = 4,
                        Attack = 5,
                        Defence = 250
                    }
                };
            }
            return botCharacters;
        }

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

                    predictedGridPosition = BattleGridManager.WorldPositionToGridPosition(predictedPosition);

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
                MovementInput                 = movementInput,
                MovementDirectionIsNormalized = false,
                MovementPositionTarget        = predictedGridPosition,
                MovementPositionMove          = FPVector2.Zero,
                MovementDirection             = FPVector2.Zero,
                RotationInput                 = false,
                RotationValue                 = FP._0,
                PlayerCharacterNumber         = -1,
                GiveUpInput                   = false
            };
        }
    }
}
