/// @file BattlePlayerQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which handles the quantum side of player logic.
/// </summary>

// Unity usings
using UnityEngine.Scripting;

// Quantum usings
using Quantum;
using Input = Quantum.Input;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;
using Battle.QSimulation.Projectile;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// <span class="brief-h">Player <a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System@u-exlink</a> @systemslink</span><br/>
    /// Handles the quantum side of player logic.
    /// </summary>
    ///
    /// This system contains methods called by BattleCollisionQSystem that deal damage to players and shields, as well as sending input data forward for movement and character switching.
    [Preserve]
    public unsafe class BattlePlayerQSystem : SystemMainThread
    {
        /// <summary>
        /// Initializes this classes BattleDebugLogger instance.<br/>
        /// This method is exclusively for debug logging purposes.
        /// </summary>
        public static void Init()
        {
            s_debugLogger = BattleDebugLogger.Create<BattlePlayerQSystem>();
        }

        /// <summary>
        /// Calls BattlePlayerManager::SpawnPlayer for players that are in the game.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public static void SpawnPlayers(Frame f)
        {
            foreach (BattlePlayerManager.PlayerHandle playerHandle in BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f))
            {
                if (playerHandle.PlayState.IsNotInGame()) continue;

                BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, 0);
            }
        }

        /// <summary>
        /// Handles logic when a player abandons the game.
        /// </summary>
        ///
        /// Updates give up state and calls <see cref="BattlePlayerQSystem.HandleGiveUpLogic">HandleGiveUpLogic</see> method which handles the rest of the logic.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle of the player who abandoned.</param>
        public static void HandlePlayerAbandoned(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            playerHandle.PlayerGiveUpState = true;
            HandleGiveUpLogic(f, playerHandle);
        }

        /// <summary>
        /// Called by BattleCollisionQSystem. Applies damage to the player after checking if it is appropriate to do so.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="playerCollisionData">Collision data related to the player character.</param>
        public static void OnProjectileHitPlayerCharacter(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerCharacterCollisionData* playerCollisionData)
        {
            // Temp disabled
            BattleProjectileQSystem.SetCollisionFlag(f, projectileCollisionData->Projectile, BattleProjectileCollisionFlags.Player);
            return;

            if (projectileCollisionData->Projectile->IsHeld) return;

            BattlePlayerDataQComponent* damagedPlayerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerCollisionData->PlayerCharacterHitbox->PlayerEntity);
            FP damageTaken = projectileCollisionData->Projectile->Attack;

            BattlePlayerManager.PlayerHandle damagePlayerHandle = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, damagedPlayerData->Slot);
            int characterNumber = damagePlayerHandle.SelectedCharacterNumber;

            FP newHp = damagedPlayerData->CurrentHp - damageTaken;

            if (damageTaken > FP._0 && damagedPlayerData->CurrentHp > FP._0 && !damagedPlayerData->DamageCooldown.IsRunning(f))
            {
                damagedPlayerData->CurrentHp = newHp;

                damagedPlayerData->DamageCooldown = FrameTimer.FromSeconds(f, BattleQConfig.GetPlayerSpec(f).DamageCooldownSec);

                f.Events.BattleCharacterTakeDamage(playerCollisionData->PlayerCharacterHitbox->PlayerEntity, damagedPlayerData->TeamNumber, damagedPlayerData->Slot, characterNumber, newHp / damagedPlayerData->Stats.Hp);
            }

            if (damagedPlayerData->CurrentHp <= FP._0)
            {
                BattlePlayerManager.DespawnPlayer(f, damagedPlayerData->Slot, kill: true);
                damagePlayerHandle.SetOutOfPlayRespawning();
                damagePlayerHandle.RespawnTimer = FrameTimer.FromSeconds(f, BattleQConfig.GetPlayerSpec(f).AutoRespawnTimeSec);
            }

            BattleProjectileQSystem.SetCollisionFlag(f, projectileCollisionData->Projectile, BattleProjectileCollisionFlags.Player);
        }

        /// <summary>
        /// Called by BattleCollisionQSystem. Applies damage to the player's shield after checking if it is appropriate to do so.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        public static void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerShieldCollisionData* shieldCollisionData)
        {
            if (!shieldCollisionData->PlayerShieldHitbox->IsActive) return;
            if (projectileCollisionData->Projectile->IsHeld) return;

            BattlePlayerDataQComponent* damagedPlayerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(shieldCollisionData->PlayerShieldHitbox->PlayerEntity);
            FP damageTaken = projectileCollisionData->Projectile->Attack;

            BattleProjectileQSystem.SetAttack(f, projectileCollisionData->Projectile, damagedPlayerData->Stats.Attack);

            int characterNumber = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, damagedPlayerData->Slot).SelectedCharacterNumber;

            FP newDefence = damagedPlayerData->CurrentDefence - damageTaken;

            if (damageTaken > FP._0 && damagedPlayerData->CurrentDefence > FP._0 && !damagedPlayerData->DamageCooldown.IsRunning(f))
            {
                damagedPlayerData->CurrentDefence = newDefence;

                damagedPlayerData->DamageCooldown = FrameTimer.FromSeconds(f, BattleQConfig.GetPlayerSpec(f).DamageCooldownSec);

                f.Events.BattleShieldTakeDamage(shieldCollisionData->PlayerShieldHitbox->PlayerEntity, damagedPlayerData->TeamNumber, damagedPlayerData->Slot, characterNumber, newDefence);
            }

            BattleProjectileQSystem.SetCollisionFlag(f, projectileCollisionData->Projectile, BattleProjectileCollisionFlags.Player);
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
        /// Relays the appropriate input data to each player in the game
        /// </summary>
        ///
        /// Update method has been split into subprocesses.<br/>
        /// see @cref{BattlePlayerQSystem,GetInput}<br/>
        /// see @cref{BattlePlayerQSystem,HandleGiveUpInput}<br/>
        /// see @cref{BattlePlayerQSystem,HandleCharacterSwapping}<br/>
        /// see @cref{BattlePlayerQSystem,HandleOutOfPlay}<br/>
        /// see @cref{BattlePlayerQSystem,HandleInPlay}
        ///
        /// <param name="f">Current simulation frame</param>
        public override void Update(Frame f)
        {
            Input* input;
            Input stackInputStorage;

            EntityRef playerEntity = EntityRef.None;
            BattlePlayerDataQComponent* playerData = null;
            Transform2D* playerTransform = null;

            BattlePlayerManager.PlayerHandle[] playerHandleArray = BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f);

            for (int playerNumber = 0; playerNumber < playerHandleArray.Length; playerNumber++)
            {
                BattlePlayerManager.PlayerHandle playerHandle = playerHandleArray[playerNumber];
                if (playerHandle.PlayState.IsNotInGame()) continue;
                if (playerHandle.PlayState.IsOutOfPlayFinal()) continue;

                if (playerHandle.PlayState.IsInPlay())
                {
                    playerEntity = playerHandle.SelectedCharacterEntity;
                    playerData = f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerEntity);
                    playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
                }

                input = GetInput(f, playerHandle, playerData, &stackInputStorage);

                if (HandleGiveUpInput(f, input, playerHandle)) continue;
                if (HandleCharacterSwapping(f, input, playerHandle)) continue;
                if (HandleOutOfPlay(f, playerHandle)) continue;

                HandleInPlay(f, input, playerHandle, playerData, playerEntity, playerTransform);
            }
        }

        /// <summary>This classes BattleDebugLogger instance.</summary>
        private static BattleDebugLogger s_debugLogger;

        /// <summary>
        /// Private helper method for retrieving the correct input (bot, abandoned, active player).<br/>
        /// Subprocess of the <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        /// <param name="playerData">Player data component.</param>
        /// <param name="stackInputStorage">Temporary input storage for bots and abandoned players.</param>
        ///
        /// <returns>Pointer to the player's input.</returns>
        private Input* GetInput(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, Input* stackInputStorage)
        {
            Input* input = stackInputStorage;

            bool isValid = false;

            if (playerHandle.IsBot)
            {
                BattlePlayerBotController.GetBotInput(f, playerHandle.PlayState.IsInPlay(), playerData, input);
                isValid = input->IsValid;
            }
            else if (!playerHandle.IsAbandoned)
            {
                input = f.GetPlayerInput(playerHandle.PlayerRef);
                isValid = input->IsValid;
            }

            if (!isValid)
            {
                input = stackInputStorage;
                *stackInputStorage = new Input
                {
                    MovementInput                 = BattleMovementInputType.None,
                    MovementDirectionIsNormalized = false,
                    MovementPositionTarget        = new BattleGridPosition { Col = 0, Row = 0 },
                    MovementPositionMove          = FPVector2.Zero,
                    MovementDirection             = FPVector2.Zero,
                    RotationInput                 = false,
                    RotationValue                 = FP._0,
                    PlayerCharacterNumber         = -1,
                    GiveUpInput                   = false
                };
            }

            return input;
        }

        /// <summary>
        /// Private helper method for handling when a player gives up or abandons the match.
        /// </summary>
        ///
        /// Used by <see cref="BattlePlayerQSystem.HandleGiveUpInput">HandleGiveUpInput</see> and <see cref="BattlePlayerQSystem.HandlePlayerAbandoned">HandlePlayerAbandoned</see>.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        private static bool HandleGiveUpLogic(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            BattlePlayerSlot slot = playerHandle.Slot;
            BattleTeamNumber team = BattlePlayerManager.PlayerHandle.GetTeamNumber(playerHandle.Slot);

            if (!playerHandle.PlayerGiveUpState)
            {
                f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.GiveUpVoteCancel);
                return false;
            }

            BattlePlayerManager.PlayerHandle teammateHandle = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, slot);
            if (!teammateHandle.PlayState.IsNotInGame())
            {
                if (!playerHandle.IsAbandoned)
                {
                    f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.GiveUpVote);
                }
                else
                {
                    f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.Abandoned);
                }
                if (!teammateHandle.PlayerGiveUpState) return false;
            }
            else
            {
                f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.GiveUpNow);
            }

            BattleTeamNumber winningTeam = team switch
            {
                BattleTeamNumber.TeamAlpha => BattleTeamNumber.TeamBeta,
                BattleTeamNumber.TeamBeta => BattleTeamNumber.TeamAlpha,

                _ => BattleTeamNumber.NoTeam
            };

            BattleGameControlQSystem.OnGameOver(f, winningTeam);
            return true;
        }

        /// <summary>
        /// Private helper method for handling player input for giving up during the game play.<br/>
        /// Subprocess of the <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// Updates give up state and calls <see cref="BattlePlayerQSystem.HandleGiveUpLogic">HandleGiveUpLogic</see> method which handles the rest of the logic.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="input">Pointer to the player's input data.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        ///
        /// <returns>True if the give up input was processed.</returns>
        private bool HandleGiveUpInput(Frame f, Input* input, BattlePlayerManager.PlayerHandle playerHandle)
        {
            if (!input->GiveUpInput) return false;

            playerHandle.PlayerGiveUpState = !playerHandle.PlayerGiveUpState;

            s_debugLogger.LogFormat(f, "({0}) Give up input received, new state: {1}", playerHandle.Slot, playerHandle.PlayerGiveUpState);

            return HandleGiveUpLogic(f, playerHandle);
        }

        /// <summary>
        /// Private helper method for handling character swapping.<br/>
        /// Subprocess of the <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="input">Pointer to the player's input data.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        ///
        /// <returns>True if character swapped.</returns>
        private bool HandleCharacterSwapping(Frame f, Input* input, BattlePlayerManager.PlayerHandle playerHandle)
        {
            if (input->PlayerCharacterNumber < 0) return false;

            s_debugLogger.LogFormat(f, "({0}) Character swap input received", playerHandle.Slot);

            if (!playerHandle.AllowCharacterSwapping)
            {
                s_debugLogger.LogFormat(f, "({0}) Character swap input rejected, as AllowCharacterSwapping == false", playerHandle.Slot);
                return false;
            }

            s_debugLogger.LogFormat(f, "({0}) Swapping to character number: {1}", playerHandle.Slot, input->PlayerCharacterNumber);

            BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, input->PlayerCharacterNumber);

            return true;
        }

        /// <summary>
        /// Private helper method for handling player logic when out of play or any of its substates.<br/>
        /// Subprocess of the <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        ///
        /// <returns>True if the player is out of play or out of play respawn.</returns>
        private bool HandleOutOfPlay(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            // handle auto respawning
            if (playerHandle.PlayState.IsOutOfPlayRespawning() && !playerHandle.RespawnTimer.IsRunning(f) && playerHandle.AllowCharacterSwapping)
            {
                int i;

                // try to spawn next character
                for (i = 0; i < Constants.BATTLE_PLAYER_CHARACTER_COUNT; i++)
                {
                    if (playerHandle.GetCharacterState(i) == BattlePlayerCharacterState.Alive)
                    {
                        s_debugLogger.LogFormat(f,"({0}) Auto spawning character number: {1}", playerHandle.Slot, i);

                        BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, i);
                        break;
                    }
                }

                // handle out of characters
                if (i == Constants.BATTLE_PLAYER_CHARACTER_COUNT)
                {
                    s_debugLogger.LogFormat(f, "({0}) Player is out of characters!", playerHandle.Slot);

                    playerHandle.SetOutOfPlayFinal();
                }

                return true;
            }

            if (playerHandle.PlayState.IsOutOfPlay()) return true;

            return false;
        }

        /// <summary>
        /// Private helper method for handling player logic and updates when state is in play.<br/>
        /// Subprocess of the <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="input">Pointer to the player's input data.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        /// <param name="playerData">Pointer to the player's data component</param>
        /// <param name="playerEntity">Reference to the player's entity</param>
        /// <param name="playerTransform">Pointer to the player's transform component.</param>
        private void HandleInPlay(Frame f, Input* input, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity, Transform2D* playerTransform)
        {
            if (playerData->CurrentDefence <= FP._0)
            {
                s_debugLogger.LogFormat(f, "({0}) Current characters shield destroyed!", playerHandle.Slot);

                f.Unsafe.GetPointer<BattlePlayerHitboxQComponent>(playerData->HitboxShieldEntity)->IsActive = false;
            }

            BattlePlayerClassManager.OnUpdate(f, playerHandle, playerData, playerEntity);
            BattlePlayerMovementController.UpdateMovement(f, playerData, playerTransform, input);
        }
    }
}
