using UnityEngine;

using Quantum;
using Input = Quantum.Input;
using Photon.Deterministic;

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
            BattleMovementInputType movementInput         = BattleMovementInputType.None;
            BattleGridPosition      predictedGridPosition = new() { Col = 0, Row = 0 };

            if (isInPlay)
            {
                if (playerData->MovementCooldown < InterceptConstants.CooldownThreshold)
                {
                    FP randnum = f.RNG->Next(0, InterceptConstants.CooldownRandAddMax);
                    playerData->MovementCooldown += randnum;
                }
                else
                {
                    movementInput = BattleMovementInputType.Position;
                }
            }

            if (movementInput != BattleMovementInputType.None)
            {
                playerData->MovementCooldown = 0;
                ComponentFilter<BattleProjectileQComponent> projectiles = f.Filter<BattleProjectileQComponent>();
                bool found = projectiles.NextUnsafe(out EntityRef projectileEntity, out BattleProjectileQComponent* projectile);
                if (!found) return;
                FPVector2 projectileDirection = projectile->Direction;
                if (playerData->TeamNumber == BattleTeamNumber.TeamAlpha ? projectileDirection.Y > 0 : projectileDirection.Y < 0) return;
                FPVector2 projectilePosition = projectile->Position;
                FP predictionTime = InterceptConstants.LookAheadTime;

                FPVector2 predictedPosition = projectilePosition + projectileDirection * projectile->Speed * predictionTime;

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

                // clamp the TargetPosition inside teams playfield for betateam
                else
                {
                    predictedGridPosition.Row = Mathf.Clamp(
                        predictedGridPosition.Row,
                        BattleGridManager.TeamBetaFieldStart + playerData->GridExtendBottom,
                        BattleGridManager.TeamBetaFieldEnd - playerData->GridExtendTop
                    );
                }
            }

            *outBotInput = new Input()
            {
                MovementInput                 = movementInput,
                MovementDirectionIsNormalized = false,
                MovementPosition              = predictedGridPosition,
                MovementDirection             = FPVector2.Zero,
                RotationInput                 = false,
                RotationValue                 = FP._0,
                PlayerCharacterNumber         = -1
            };
        }

        private static class InterceptConstants
        {
            public static readonly FP CooldownThreshold = FP._0_33;
            public static readonly FP CooldownRandAddMax = FP._0_02;
            public static readonly FP LookAheadTime = FP._0_50;
        }
    }
}
