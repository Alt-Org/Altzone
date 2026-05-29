/// @file BattlePlayerQSystem.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerQSystem} [Quantum System](https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems) which handles the quantum side of player logic.
/// </summary>

// Unity usings
using UnityEngine.Scripting;

// Quantum usings
using Quantum;
using Photon.Deterministic;
using Input = Quantum.Input;

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
    /// [{Player Overview}](#page-concepts-player-overview)<br/>
    /// [{Player Simulation Code Overview}](#page-concepts-player-simulation-overview)
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
            playerHandle.GiveUpState = true;
            HandleGiveUpLogic(f, playerHandle);
        }

        /// <summary>
        /// Called by BattleCollisionQSystem. Stuns the player after checking if it is appropriate to do so and kills the player if he gets hit without a shield.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="playerCollisionData">Collision data related to the player character.</param>
        public static void OnProjectileHitPlayerCharacter(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData, BattleCollisionQSystem.PlayerCharacterCollisionData* playerCollisionData)
        {
            if (projectileCollisionData->Projectile->IsHeld) return;

            // get spec
            BattlePlayerQSpec playerSpec = BattleQConfig.GetPlayerSpec(f);

            // get references
            BattlePlayerEntityRef            damagedPlayerEntityRef = (BattlePlayerEntityRef)playerCollisionData->PlayerCharacterHitbox->ParentEntityRef;
            BattlePlayerDataQComponent*      damagedPlayerData      = damagedPlayerEntityRef.GetDataQComponent(f);
            BattlePlayerManager.PlayerHandle playerHandle           = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, damagedPlayerData->Slot);

            if (damagedPlayerData->StunCooldown.IsRunning(f) || (damagedPlayerData->ShieldHitCooldown.IsRunning(f)) && damagedPlayerData->AttachedShield != EntityRef.None) goto Exit;

            if (damagedPlayerData->CurrentDefence > 0)
            {
                // handle stun

                damagedPlayerData->MovementEnabled = false;
                damagedPlayerData->RotationEnabled = false;
                damagedPlayerData->StunCooldown    = FrameTimer.FromSeconds(f, playerSpec.StunDurationSec);

                SoundEffectTypeCommon soundEffectType = projectileCollisionData->ProjectileEmotionCurrent switch
                {
                    BattleEmotionState.Aggression => SoundEffectTypeCommon.HitCharacterAggression,
                    BattleEmotionState.Joy        => SoundEffectTypeCommon.HitCharacterJoy,
                    BattleEmotionState.Love       => SoundEffectTypeCommon.HitCharacterLove,
                    BattleEmotionState.Playful    => SoundEffectTypeCommon.HitCharacterPlayful,
                    BattleEmotionState.Sadness    => SoundEffectTypeCommon.HitCharacterSadness,

                    _ => throw new System.NotImplementedException()
                };
                HandleSFXCommon(f, damagedPlayerData->Slot, soundEffectType, SoundEffectTarget.All);

                f.Events.BattleCharacterHit(
                    damagedPlayerEntityRef,
                    damagedPlayerData->TeamNumber,
                    damagedPlayerData->Slot,
                    playerHandle.SelectedCharacterNumber,
                    damagedPlayerData->AttachedShieldNumber,
                    playerSpec.StunDurationSec,
                    projectileCollisionData->ProjectileEmotionCurrent
                );
            }
            else
            {
                // handle death

                BattlePlayerManager.DespawnPlayer(f, damagedPlayerData->Slot, kill: true);
                playerHandle.SetOutOfPlayRespawning();
                playerHandle.RespawnTimer = FrameTimer.FromSeconds(f, playerSpec.AutoRespawnTimeSec);

                HandleSFXCommon(f, damagedPlayerData->Slot, SoundEffectTypeCommon.Death, SoundEffectTarget.All);
                f.Events.BattleCharacterDeath(damagedPlayerData->Slot, playerHandle.SelectedCharacterNumber);
            }

        Exit:
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
            // checks
            if (projectileCollisionData->Projectile->IsHeld) return;

            //{ hit

            BattlePlayerShieldDataQComponent* playerShieldData  = ((BattlePlayerShieldEntityRef)shieldCollisionData->PlayerShieldHitbox->ParentEntityRef).GetDataQComponent(f);
            BattlePlayerDataQComponent*       damagedPlayerData = playerShieldData->PlayerEntityRef.GetDataQComponent(f);

            int  characterNumber     = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, damagedPlayerData->Slot).SelectedCharacterNumber;
            bool defenceUpdateVisual = false;
            FP   defencePercentage   = -1;

            if (playerShieldData->ShieldHitCooldown.IsRunning(f)) goto ExitNoHit;

            HandleSFXCommon(f, damagedPlayerData->Slot, SoundEffectTypeCommon.HitShield, SoundEffectTarget.Player);

            //} hit

            //{ hit attach

            if (!playerShieldData->IsAttached) goto ExitHit;

            FP damageTaken = projectileCollisionData->Projectile->Attack;

            BattleProjectileQSystem.SetAttack(f, projectileCollisionData->Projectile, damagedPlayerData->Stats.Attack);

            if (damageTaken <= FP._0) goto ExitNoHit;

            damagedPlayerData->CurrentDefence = damagedPlayerData->CurrentDefence - damageTaken;

            defenceUpdateVisual = true;
            defencePercentage = damagedPlayerData->CurrentDefence / damagedPlayerData->Stats.Defence;

            if (damagedPlayerData->CurrentDefence <= 0)
            {
                s_debugLogger.LogFormat(f, "({0}) Current characters shield destroyed!", damagedPlayerData->Slot);

                BattlePlayerShieldManager.RemoveShield(f, damagedPlayerData->Slot, characterNumber, playerShieldData->ShieldNumber);
            }

            //} hit attach

        ExitHit:
            FP damageCooldownSec                 = BattleQConfig.GetPlayerSpec(f).DamageCooldownSec;
            playerShieldData->ShieldHitCooldown  = FrameTimer.FromSeconds(f, damageCooldownSec);
            damagedPlayerData->ShieldHitCooldown = FrameTimer.FromSeconds(f, damageCooldownSec);

            f.Events.BattleShieldHit(
                shieldCollisionData->PlayerShieldHitbox->ParentEntityRef,
                damagedPlayerData->TeamNumber,
                damagedPlayerData->Slot,
                characterNumber,
                damagedPlayerData->AttachedShieldNumber,
                defenceUpdateVisual,
                defencePercentage
            );
        ExitNoHit:
            BattleProjectileQSystem.SetCollisionFlag(f, projectileCollisionData->Projectile, BattleProjectileCollisionFlags.Player);
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
        /// Relays the appropriate input data to each player in the game
        /// </summary>
        ///
        /// Update method has been split into subprocesses.<br/>
        /// see @cref{BattlePlayerQSystem,GetInput}<br/>
        /// see @cref{BattlePlayerQSystem,HandleGiveUp}<br/>
        /// see @cref{BattlePlayerQSystem,HandleCharacterSwapping}<br/>
        /// see @cref{BattlePlayerQSystem,HandleOutOfPlay}<br/>
        /// see @cref{BattlePlayerQSystem,HandleInPlay}
        ///
        /// <param name="f">Current simulation frame</param>
        public override void Update(Frame f)
        {
            BattleGameSessionQSingleton* singleton = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>();
            if (singleton->State != BattleGameState.Playing) return;

            InputData inputData;
            Input stackInputStorage;

            BattlePlayerEntityRef       playerEntity    = BattlePlayerEntityRef.None;
            BattlePlayerDataQComponent* playerData      = null;
            Transform2D*                playerTransform = null;

            BattlePlayerManager.PlayerHandle[] playerHandleArray = BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f);

            for (int playerNumber = 0; playerNumber < playerHandleArray.Length; playerNumber++)
            {
                BattlePlayerManager.PlayerHandle playerHandle = playerHandleArray[playerNumber];
                if (playerHandle.PlayState.IsNotInGame()) continue;

                inputData = GetInput(f, playerHandle, &stackInputStorage);

                //{ non-character logic

                switch (inputData.CommandType)
                {
                    case BattleCommand.Type.GiveUp:
                        if (HandleGiveUp(f, playerHandle)) continue;
                        break;

                    case BattleCommand.Type.SwapCharacter:
                        BattleCharacterSwapQCommand swapCharacterData = (BattleCharacterSwapQCommand)inputData.CommandData;
                        if (HandleCharacterSwapping(f, playerHandle, swapCharacterData.CharacterNumber)) continue;
                        break;
                }

                if (HandleOutOfPlay(f, playerHandle)) continue;

                //} non-character logic

                //{ character logic

                playerEntity    = playerHandle.GetSelectedCharacterEntityRef(f);
                playerData      = playerEntity.GetDataQComponent(f);
                playerTransform = playerEntity.GetTransform(f);

                switch (inputData.CommandType)
                {
                    case BattleCommand.Type.ActivateAbility:
                        playerData->AbilityActivateBufferSec = FrameTimer.FromSeconds(f, FP._0_50);
                        break;
                }

                HandleInPlay(f, inputData.Input, playerHandle, playerData, playerEntity, playerTransform);

                //} character logic
            }
        }

        /// <summary>Enum used to define common sound effect types</summary>
        ///
        /// Used by @cref{HandleSFXCommon} method.
        private enum SoundEffectTypeCommon
        {
            HitShield,
            Catchphrase,
            HitCharacterAggression,
            HitCharacterJoy,
            HitCharacterLove,
            HitCharacterPlayful,
            HitCharacterSadness,
            Death
        }

        /// <summary>Enum used to define character specific sound effect types</summary>
        ///
        /// Used by @cref{HandleSFXCharacter} method.
        private enum SoundEffectTypeCharacter
        {
            Catchphrase,
            HitCharacterAggression,
            HitCharacterJoy,
            HitCharacterLove,
            HitCharacterPlayful,
            HitCharacterSadness,
            Death
        }

        /// <summary>Enum used to define the target of a sound effect</summary>
        ///
        /// Used by @cref{HandleSFX} method.
        private enum SoundEffectTarget
        {
            /// <summary>Sound effect played for all players</summary>
            All,
            /// <summary>Sound effect played for local player's team</summary>
            Team,
            /// <summary>Sound effect played for local player</summary>
            Player
        }

        /// <summary>
        /// Struct containing input data from different input methods.
        /// </summary>
        private struct InputData
        {
        /// <summary>Quantum's default input struct</summary>
        public Input* Input;
        /// <summary>Type of the command</summary>
        public BattleCommand.Type CommandType;
        /// <summary>Data related to the command</summary>
        public BattleCommand CommandData;
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
        private InputData GetInput(Frame f, BattlePlayerManager.PlayerHandle playerHandle, Input* stackInputStorage)
        {
            InputData inputData = new()
            {
                Input = stackInputStorage,
                CommandType = BattleCommand.Type.None,
                CommandData = null
            };

            bool isValid = false;

            if (playerHandle.IsBot)
            {
                BattlePlayerBotController.GetBotInput(f, playerHandle, inputData.Input, &inputData.CommandType, inputData.CommandData);
                isValid = inputData.Input->IsValid;
            }
            else if (!playerHandle.IsAbandoned)
            {
                inputData.Input = f.GetPlayerInput(playerHandle.PlayerRef);
                inputData.CommandType = BattleCommand.GetCommand(f, playerHandle.PlayerRef, out inputData.CommandData);

                BattleInputDebugUtils.InputDebugInfo inputDebugInfo = BattleInputDebugUtils.GenerateDebugInfo(inputData.Input);

                if (inputDebugInfo.NotEmpty)
                {
                    s_debugLogger.LogFormat(f,
                                            "({0}) Received input ({1}) ({2})\n" +
                                            "struct: {3}",
                                            playerHandle.Slot,
                                            inputData.Input->DebugNumber,
                                            inputDebugInfo.Summary,
                                            inputDebugInfo.Struct
                    );
                }

                isValid = inputData.Input->IsValid;
            }

            if (!isValid)
            {
                inputData.Input = stackInputStorage;
                *stackInputStorage = new Input
                {
                    IsValid                       = true,
                    MovementInput                 = BattleMovementInputType.None,
                    MovementDirectionIsNormalized = false,
                    MovementGridPosition          = new BattleGridPosition { Col = 0, Row = 0 },
                    MovementVector                = FPVector2.Zero,
                    RotationInput                 = false,
                    RotationValue                 = FP._0,
                };
            }

            return inputData;
        }

        /// <summary>
        /// Private helper method for handling when a player wants to give up or has abandoned the match.
        /// </summary>
        ///
        /// Used by <see cref="BattlePlayerQSystem.HandleGiveUpInput">HandleGiveUpInput</see> and <see cref="BattlePlayerQSystem.HandlePlayerAbandoned">HandlePlayerAbandoned</see>.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        ///
        /// <returns>True if all players on a team have given up.</returns>
        private static bool HandleGiveUpLogic(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            BattlePlayerSlot slot = playerHandle.Slot;
            BattleTeamNumber team = BattlePlayerManager.PlayerHandle.GetTeamNumber(playerHandle.Slot);

            if (!playerHandle.GiveUpState)
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
                if (!teammateHandle.GiveUpState) return false;
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
        /// Private helper method for playing specified <paramref name="soundEffect"/> for specified sound effect <paramref name="target"/>.
        /// @note Only handles sending the correct event based on <paramref name="target"/>.
        /// Use <see cref="HandleSFXCommon">HandleSFXCommon</see> or <see cref="HandleSFXCharacter">HandleSFXCharacter</see> for playing an appropriate sound effect
        /// </summary>
        ///
        /// <param name="f">Current simulation frame</param>
        /// <param name="slot">Slot of the player who, or whose team, will hear the sound depening on the <paramref name="target"/></param>
        /// <param name="soundEffect">Sound effect to be played</param>
        /// <param name="target">Target that will hear the sound effect to be played</param>
        private static void HandleSFX(Frame f, BattlePlayerSlot slot, BattleSoundFX soundEffect, SoundEffectTarget target)
        {
            switch (target)
            {
                case SoundEffectTarget.All:
                    f.Events.BattlePlaySoundFxForAll(soundEffect);
                    break;
                case SoundEffectTarget.Team:
                    BattleTeamNumber teamNumber = BattlePlayerManager.PlayerHandle.GetTeamNumber(slot);
                    f.Events.BattlePlaySoundFxForTeam(teamNumber, soundEffect);
                    break;
                case SoundEffectTarget.Player:
                    f.Events.BattlePlaySoundFxForPlayer(slot, soundEffect);
                    break;
            }
        }

        /// <summary>
        /// Private helper method for playing the appropriate common sound effect based on sound effect <paramref name="type"/>
        /// </summary>
        ///
        /// Use @cref{HandleSFXCharacter} to play character specific sound effects.
        ///
        /// <param name="f">Current simulation frame</param>
        /// <param name="slot">Slot of the player who, or whose team, will hear the sound depening on the <paramref name="target"/></param>
        /// <param name="type">Type of sound effect to be played</param>
        /// <param name="target">Target that will hear the sound effect to be played</param>
        private static void HandleSFXCommon(Frame f, BattlePlayerSlot slot, SoundEffectTypeCommon type, SoundEffectTarget target)
        {
            BattleSoundFX soundEffect = (BattleSoundFX)(Constants.BATTLE_SOUND_FX_CHARACTER_COMMON_START + type);

            HandleSFX(f, slot, soundEffect, target);
        }

        /// <summary>
        /// Private helper method for playing the appropriate character specific sound effect based on <paramref name="characterID"/> and sound effect <paramref name="type"/>
        /// </summary>
        ///
        /// Use @cref{HandleSFXCommon} to play common sound effects.
        ///
        /// <param name="f">Current simulation frame</param>
        /// <param name="slot">Slot of the player who, or whose team, will hear the sound depening on the <paramref name="target"/></param>
        /// <param name="type">Type of sound effect to be played</param>
        /// <param name="characterID">ID of the current character in play</param>
        /// <param name="target">Target that will hear the sound effect to be played</param>
        private static void HandleSFXCharacter(Frame f, BattlePlayerSlot slot, SoundEffectTypeCharacter type, BattlePlayerCharacterID characterID, SoundEffectTarget target)
        {
            BattleSoundFX soundEffect = (BattleSoundFX)(Constants.BATTLE_SOUND_FX_CHARACTER_START + (int)characterID * Constants.BATTLE_SOUND_FX_CHARACTER_ID_MULTIPLIER) + (int)type;

            HandleSFX(f, slot, soundEffect, target);
        }

        /// <summary>
        /// Private helper method for handling player give up command.<br/>
        /// Subprocess of <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// Updates give up state and calls <see cref="BattlePlayerQSystem.HandleGiveUpLogic">HandleGiveUpLogic</see> method which handles the rest of the logic.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        ///
        /// <returns>True if all players on a team have given up.</returns>
        private bool HandleGiveUp(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            playerHandle.GiveUpState = !playerHandle.GiveUpState;

            s_debugLogger.LogFormat(f, "({0}) Give up input received, new state: {1}", playerHandle.Slot, playerHandle.GiveUpState);

            return HandleGiveUpLogic(f, playerHandle);
        }

        /// <summary>
        /// Private helper method for handling character swapping.<br/>
        /// Subprocess of <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        ///
        /// <returns>True if character was swapped.</returns>
        private bool HandleCharacterSwapping(Frame f, BattlePlayerManager.PlayerHandle playerHandle, int playerCharacterNumber)
        {
            if (playerCharacterNumber == playerHandle.SelectedCharacterNumber) return false;

            s_debugLogger.LogFormat(f, "({0}) Character swap input received", playerHandle.Slot);

            if (!playerHandle.AllowCharacterSwapping)
            {
                s_debugLogger.LogFormat(f, "({0}) Character swap input rejected, as AllowCharacterSwapping == false", playerHandle.Slot);
                return false;
            }

            s_debugLogger.LogFormat(f, "({0}) Swapping to character number: {1}", playerHandle.Slot, playerCharacterNumber);

            BattlePlayerManager.SpawnPlayer(f, playerHandle.Slot, playerCharacterNumber);
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
        private void HandleInPlay(Frame f, Input* input, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, BattlePlayerEntityRef playerEntity, Transform2D* playerTransform)
        {
            playerData->ViewMovementVector = FPVector2.Zero;

            bool updateMovement = true;

            if (!playerData->AbilityCooldownSec.IsRunning(f) && playerData->AbilityActivateBufferSec.IsRunning(f))
            {
                AbilityActivate(f, playerData, playerTransform);
                updateMovement = false;
            }

            if (!playerData->StunCooldown.IsRunning(f))
            {
                playerData->MovementEnabled = true;
                playerData->RotationEnabled = !playerData->DisableRotation;
            }

            BattlePlayerClassManager.OnUpdate(f, playerHandle, playerData, playerEntity, &input->Special);
            if (updateMovement) BattlePlayerMovementController.UpdateMovement(f, playerData, playerEntity, playerTransform, input);
        }

        private void AbilityActivate(Frame f, BattlePlayerDataQComponent* playerData, Transform2D* playerTransform)
        {
            //{ Ability test
            /*

            if (playerData->CharacterId == 601)
            {
                for (int i = 0; i < 4; i++)
                {
                    BattleSoulWallQSystem.CreateAbilitySoulWallTest(f, playerData->TeamNumber, playerTransform->Position + new FPVector2(f.RNG->NextInclusive(-1, 1), f.RNG->NextInclusive(-1, 1)).Normalized * 2);
                }

                BattlePlayerManager.PlayerHandle playerHandle = BattlePlayerManager.PlayerHandle.GetPlayerHandle(f, playerData->Slot);

                BattlePlayerManager.DespawnPlayer(f, playerData->Slot, kill: true);
                playerHandle.SetOutOfPlayRespawning();
                playerHandle.RespawnTimer = FrameTimer.FromSeconds(f, BattleQConfig.GetPlayerSpec(f).AutoRespawnTimeSec);
            }
            else
            {
                BattleSoulWallQSystem.CreateAbilitySoulWallTest(f, playerData->TeamNumber, playerTransform->Position);
            }

            */
            //} Ability test

            playerData->AbilityCooldownSec = FrameTimer.FromSeconds(f, FP._3);
        }
    }
}
