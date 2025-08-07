/// @file BattlePlayerQSystem.cs
/// <summary>
/// Handles the quantum side of player logic.
/// </summary>
///
/// This system reacts to the ProjectileHitPlayerHitbox event to deal damage to players, as well as sending input data forward for movement and character switching.

using UnityEngine.Scripting;

using Quantum;
using Photon.Deterministic;

using Battle.QSimulation.Projectile;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// <span class="brief-h">Player <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum SystemSignalsOnly@u-exlink</a> @systemslink</span><br/>
    /// Handles the quantum side of player logic.
    /// </summary>
    [Preserve]
    public unsafe class BattlePlayerQSystem : SystemMainThread, ISignalBattleOnProjectileHitPlayerHitbox
    {
        /// <summary>
        /// Calls BattlePlayerManager::SpawnPlayer for players that are in the game.
        /// </summary>
        /// <param name="f">Current simulation frame</param>
        public static void SpawnPlayers(Frame f)
        {
            foreach (BattlePlayerManager.PlayerHandle playerHandle in BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f))
            {
                if (playerHandle.PlayState == BattlePlayerPlayState.NotInGame) continue;

                BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, 0);
            }
        }

        /// <summary>
        /// <span class="brief-h"><a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems" > Quantum System Signal method@u-exlink</a>
        /// that gets called when <see cref="Quantum.ISignalBattleOnProjectileHitPlayerHitbox">ISignalBattleOnProjectileHitPlayerHitbox</see> is sent.</span><br/>
        /// Deals damage to the player hit if their damage cooldown is not active and despawns the character if its Hp reaches 0.
        /// @warning
        /// This method should only be called by Quantum.
        /// </summary>
        /// <param name="f">Current simulation frame</param>
        /// <param name="projectile">Pointer reference to the projectile.</param>
        /// <param name="projectileEntity">The projectile entity.</param>
        /// <param name="playerHitbox">Pointer reference to the player hitbox that the projectile collided with.</param>
        /// <param name="playerHitboxEntity">The player hitbox entity.</param>
        public unsafe void BattleOnProjectileHitPlayerHitbox(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            if (BattleProjectileQSystem.IsCollisionFlagSet(f, projectile, BattleProjectileCollisionFlags.Player)) return;

            BattlePlayerDataQComponent* _damagedPlayerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity);
            FP _damageTaken = FP._1;

            int characterNumber = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, _damagedPlayerData->Slot).SelectedCharacterNumber;

            FP newHp = _damagedPlayerData->CurrentHp - _damageTaken;

            if (_damageTaken > FP._0 && _damagedPlayerData->CurrentHp > 0 && !_damagedPlayerData->DamageCooldown.IsRunning(f))
            {
                _damagedPlayerData->CurrentHp = newHp;
                _damageTaken = FP._0;

                _damagedPlayerData->DamageCooldown = FrameTimer.FromSeconds(f, 1);

                f.Events.BattleCharacterTakeDamage(playerHitbox->PlayerEntity, _damagedPlayerData->TeamNumber, _damagedPlayerData->Slot, characterNumber, newHp / _damagedPlayerData->Stats.Hp);
            }

            if (_damagedPlayerData->CurrentHp <= FP._0)
            {
                BattlePlayerManager.DespawnPlayer(f, _damagedPlayerData->Slot);
            }

            BattleProjectileQSystem.SetCollisionFlag(f, projectile, BattleProjectileCollisionFlags.Player);
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
        /// Relays the appropriate input data to each player in the game
        /// </summary>
        /// <param name="f">Current simulation frame</param>
        public override void Update(Frame f)
        {
            Input* input;

            EntityRef playerEntity;
            BattlePlayerDataQComponent* playerData;
            Transform2D* playerTransform;

            foreach (BattlePlayerManager.PlayerHandle playerHandle in BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f))
            {
                if (playerHandle.PlayState == BattlePlayerPlayState.NotInGame) continue;

                input = f.GetPlayerInput(playerHandle.PlayerRef);

                if (input->PlayerCharacterNumber > -1)
                {
                    BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, input->PlayerCharacterNumber);
                    continue;
                }

                if (playerHandle.PlayState == BattlePlayerPlayState.OutOfPlay) continue;

                playerEntity = playerHandle.SelectedCharacter;
                playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerEntity);
                playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);

                BattlePlayerMovementController.UpdateMovement(f, playerData, playerTransform, input);
            }
        }
    }
}
