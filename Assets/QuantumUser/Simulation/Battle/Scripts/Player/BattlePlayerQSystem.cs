/// @file BattlePlayerQSystem.cs
/// <summary>
/// Handles the quantum side of player logic.
/// </summary>
///
/// This system contains methods called by BattleCollisionQSystem that deal damage to players and shields, as well as sending input data forward for movement and character switching.

using UnityEngine.Scripting;
using Quantum;
using Photon.Deterministic;

using Battle.QSimulation.Projectile;
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// <span class="brief-h">Player <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Handles the quantum side of player logic.
    /// </summary>
    [Preserve]
    public unsafe class BattlePlayerQSystem : SystemMainThread
    {
        /// <summary>
        /// Calls BattlePlayerManager::SpawnPlayer for players that are in the game.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame</param>
        public static void SpawnPlayers(Frame f)
        {
            foreach (BattlePlayerManager.PlayerHandle playerHandle in BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f))
            {
                if (playerHandle.PlayState.IsNotInGame()) continue;

                BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, 0);
            }
        }

        /// <summary>
        /// Called by BattleCollisionQSystem. Applies damage to the player after checking if it is appropriate to do so.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame</param>
        /// <param name="projectile">Pointer reference to the projectile.</param>
        /// <param name="projectileEntity">The projectile entity.</param>
        /// <param name="playerHitbox">Pointer reference to the player hitbox that the projectile collided with.</param>
        /// <param name="playerHitboxEntity">The player hitbox entity.</param>
        public static void OnProjectileHitPlayerHitbox(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            if (projectile->IsHeld) return;
            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Player)) return;

            BattlePlayerDataQComponent* damagedPlayerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);
            FP damageTaken = projectile->Attack;

            BattlePlayerManager.PlayerHandle damagePlayerHandle = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, damagedPlayerData->Slot);
            int characterNumber = damagePlayerHandle.SelectedCharacterNumber;

            FP newHp = damagedPlayerData->CurrentHp - damageTaken;

            if (damageTaken > FP._0 && damagedPlayerData->CurrentHp > FP._0 && !damagedPlayerData->DamageCooldown.IsRunning(f))
            {
                damagedPlayerData->CurrentHp = newHp;

                damagedPlayerData->DamageCooldown = FrameTimer.FromSeconds(f, BattleQConfig.GetPlayerSpec(f).DamageCooldownSec);

                f.Events.BattleCharacterTakeDamage(playerHitbox->PlayerEntity, damagedPlayerData->TeamNumber, damagedPlayerData->Slot, characterNumber, newHp / damagedPlayerData->Stats.Hp);
            }

            if (damagedPlayerData->CurrentHp <= FP._0)
            {
                BattlePlayerManager.DespawnPlayer(f, damagedPlayerData->Slot, kill: true);
                damagePlayerHandle.SetOutOfPlayRespawning();
                damagePlayerHandle.RespawnTimer = FrameTimer.FromSeconds(f, BattleQConfig.GetPlayerSpec(f).AutoRespawnTimeSec);
            }

            BattlePlayerClassManager.OnProjectileHitPlayerHitbox(f, projectile, projectileEntity, playerHitbox, playerHitboxEntity);

            BattleProjectileQSystem.SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Player);
        }

        /// <summary>
        /// Called by BattleCollisionQSystem. Applies damage to the player's shield after checking if it is appropriate to do so.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame</param>
        /// <param name="projectile">Pointer reference to the projectile.</param>
        /// <param name="projectileEntity">The projectile entity.</param>
        /// <param name="playerHitbox">Pointer reference to the player's shield hitbox that the projectile collided with.</param>
        /// <param name="playerHitboxEntity">The player's shield hitbox entity.</param>
        public static void OnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            if (!playerHitbox->IsActive) return;
            if (projectile->IsHeld) return;
            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Player)) return;

            BattlePlayerDataQComponent* damagedPlayerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);
            FP damageTaken = projectile->Attack;

            BattleProjectileQSystem.SetAttack(f, projectile, damagedPlayerData->Stats.Attack);

            int characterNumber = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, damagedPlayerData->Slot).SelectedCharacterNumber;

            FP newDefence = damagedPlayerData->CurrentDefence - damageTaken;

            if (damageTaken > FP._0 && damagedPlayerData->CurrentDefence > FP._0 && !damagedPlayerData->DamageCooldown.IsRunning(f))
            {
                damagedPlayerData->CurrentDefence = newDefence;

                damagedPlayerData->DamageCooldown = FrameTimer.FromSeconds(f, BattleQConfig.GetPlayerSpec(f).DamageCooldownSec);

                f.Events.BattleShieldTakeDamage(playerHitbox->PlayerEntity, damagedPlayerData->TeamNumber, damagedPlayerData->Slot, characterNumber, newDefence);
            }

            BattlePlayerClassManager.OnProjectileHitPlayerShield(f, projectile, projectileEntity, playerHitbox, playerHitboxEntity);

            BattleProjectileQSystem.SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Player);
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
        /// Relays the appropriate input data to each player in the game
        /// </summary>
        ///
        /// <param name="f">Current simulation frame</param>
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

                if (input->PlayerCharacterNumber > -1 && playerHandle.AllowCharacterSwapping)
                {
                    BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, input->PlayerCharacterNumber);
                    continue;
                }

                if (playerHandle.PlayState.IsOutOfPlayRespawning() && !playerHandle.RespawnTimer.IsRunning(f) && playerHandle.AllowCharacterSwapping)
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

                if (playerHandle.PlayState.IsOutOfPlay()) continue;

                playerEntity = playerHandle.SelectedCharacterEntity;
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
