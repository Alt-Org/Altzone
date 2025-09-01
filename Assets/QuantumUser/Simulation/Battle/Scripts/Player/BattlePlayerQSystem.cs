using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

using Battle.QSimulation.Projectile;

namespace Battle.QSimulation.Player
{
    [Preserve]
    public unsafe class BattlePlayerQSystem : SystemMainThread
    {
        public static void SpawnPlayers(Frame f)
        {
            foreach (BattlePlayerManager.PlayerHandle playerHandle in BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f))
            {
                if (playerHandle.PlayState.IsNotInGame()) continue;

                BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, 0);
            }
        }

        public static void OnProjectileHitPlayerHitbox(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Player)) return;

            BattlePlayerDataQComponent* damagedPlayerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);
            FP damageTaken = projectile->Attack;

            BattlePlayerManager.PlayerHandle damagePlayerHandle = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, damagedPlayerData->Slot);
            int characterNumber = damagePlayerHandle.SelectedCharacterNumber;

            FP newHp = damagedPlayerData->CurrentHp - damageTaken;

            if (damageTaken > FP._0 && damagedPlayerData->CurrentHp > FP._0 && !damagedPlayerData->DamageCooldown.IsRunning(f))
            {
                damagedPlayerData->CurrentHp = newHp;

                damagedPlayerData->DamageCooldown = FrameTimer.FromSeconds(f, FP._1);

                f.Events.BattleCharacterTakeDamage(playerHitbox->PlayerEntity, damagedPlayerData->TeamNumber, damagedPlayerData->Slot, characterNumber, newHp / damagedPlayerData->Stats.Hp);
            }

            if (damagedPlayerData->CurrentHp <= FP._0)
            {
                BattlePlayerManager.DespawnPlayer(f, damagedPlayerData->Slot, kill: true);
                damagePlayerHandle.SetOutOfPlayRespawning();
                damagePlayerHandle.RespawnTimer = FrameTimer.FromSeconds(f, FP._0_50);
            }

            BattlePlayerClassManager.OnProjectileHitPlayerHitbox(f, projectile, projectileEntity, playerHitbox, playerHitboxEntity);

            BattleProjectileQSystem.SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Player);
        }

        public static void OnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            if (!playerHitbox->IsActive) return;
            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Player)) return;

            BattlePlayerDataQComponent* damagedPlayerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);
            FP damageTaken = projectile->Attack;

            BattleProjectileQSystem.SetAttack(f, projectile, damagedPlayerData->Stats.Attack);

            int characterNumber = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, damagedPlayerData->Slot).SelectedCharacterNumber;

            FP newDefence = damagedPlayerData->CurrentDefence - damageTaken;

            if (damageTaken > FP._0 && damagedPlayerData->CurrentDefence > FP._0 && !damagedPlayerData->DamageCooldown.IsRunning(f))
            {
                damagedPlayerData->CurrentDefence = newDefence;

                damagedPlayerData->DamageCooldown = FrameTimer.FromSeconds(f, FP._1);

                f.Events.BattleShieldTakeDamage(playerHitbox->PlayerEntity, damagedPlayerData->TeamNumber, damagedPlayerData->Slot, characterNumber, newDefence);
            }

            BattlePlayerClassManager.OnProjectileHitPlayerShield(f, projectile, projectileEntity, playerHitbox, playerHitboxEntity);

            BattleProjectileQSystem.SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Player);
        }

        public override void Update(Frame f)
        {
            Input* input;

            EntityRef playerEntity;
            BattlePlayerDataQComponent* playerData;
            Transform2D* playerTransform;

            BattlePlayerManager.PlayerHandle[] playerHandleArray = BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f);

            for (int playerNumber = 0; playerNumber < playerHandleArray.Length; playerNumber++)
            {
                BattlePlayerManager.PlayerHandle playerHandle = playerHandleArray[playerNumber];
                if (playerHandle.PlayState.IsNotInGame()) continue;
                if (playerHandle.PlayState.IsOutOfPlayFinal()) continue;

                input = f.GetPlayerInput(playerHandle.PlayerRef);

                if (input->PlayerCharacterNumber > -1)
                {
                    BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, input->PlayerCharacterNumber);
                    continue;
                }

                if (playerHandle.PlayState.IsOutOfPlayRespawning())
                {
                    if (!playerHandle.RespawnTimer.IsRunning(f))
                    {
                        int i;
                        for (i = 0; i < Constants.BATTLE_PLAYER_CHARACTER_COUNT; i++)
                        {
                            if (playerHandle.GetCharacterState(i) == BattlePlayerCharacterState.Alive)
                            {
                                BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, i);
                                break;
                            }
                        }
                        if (i == Constants.BATTLE_PLAYER_CHARACTER_COUNT)
                        {
                            playerHandle.SetOutOfPlayFinal();
                        }
                    }
                }

                if (playerHandle.PlayState.IsOutOfPlay()) continue;

                playerEntity = playerHandle.SelectedCharacter;
                playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerEntity);
                playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);

                if (playerData->CurrentDefence <= FP._0)
                {
                    f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(playerData->HitboxShieldEntity)->IsActive = false;
                }

                BattlePlayerClassManager.OnUpdate(f, playerHandle, playerData, playerEntity);
                BattlePlayerMovementController.UpdateMovement(f, playerData, playerTransform, input);
            }
        }
    }
}
