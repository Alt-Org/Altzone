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
        /// Calls <see cref="BattlePlayerManager.SpawnPlayer">BattlePlayerManager.SpawnPlayer</see> for players that are in the game.
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

            BattleTeamNumber giveUpTeam = HandleGiveUpLogic(f, playerHandle);
            if (giveUpTeam != BattleTeamNumber.NoTeam)
            {
                BattleGameControlQSystem.OnGameOverGiveUp(f, giveUpTeam);
            }
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

            if (damagedPlayerData->StunCooldown.IsRunning(f) || damagedPlayerData->ShieldHitCooldown.IsRunning(f)) goto Exit;

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

                int selectedCharacterNumber = playerHandle.SelectedCharacterNumber;

                BattlePlayerManager.DespawnPlayer(f, damagedPlayerData->Slot, kill: true);
                playerHandle.SetOutOfPlayRespawning();
                playerHandle.RespawnTimer = FrameTimer.FromSeconds(f, playerSpec.AutoRespawnTimeSec);

                HandleSFXCommon(f, damagedPlayerData->Slot, SoundEffectTypeCommon.Death, SoundEffectTarget.All);
                f.Events.BattleCharacterDeath(damagedPlayerData->Slot, selectedCharacterNumber);
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
            if (damagedPlayerData->AttachedShield != EntityRef.None)
            {
                damagedPlayerData->ShieldHitCooldown = FrameTimer.FromSeconds(f, damageCooldownSec);
            }

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
        /// Calls <see cref="BattlePlayerClassManager.OnGameStart">BattlePlayerClassManager.OnGameStart</see> for every player's selected character.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public static void OnGameStart(Frame f)
        {
            foreach (BattlePlayerManager.PlayerHandle playerHandle in BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f))
            {
                if (playerHandle.PlayState.IsNotInGame()) continue;

                BattlePlayerEntityRef entityRef        = playerHandle.GetSelectedCharacterEntityRef(f);
                BattlePlayerDataQComponent* playerData = entityRef.GetDataQComponent(f);

                BattlePlayerClassManager.OnGameStart(f, playerHandle, playerData, entityRef);
            }
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

            UpdateData updateData = new();
            Input stackInputStorage;

            BattlePlayerManager.PlayerHandle[] playerHandleArray = BattlePlayerManager.PlayerHandle.GetPlayerHandleArray(f);

            for (int playerNumber = 0; playerNumber < playerHandleArray.Length; playerNumber++)
            {
                updateData.SetPlayer(playerHandleArray[playerNumber]);
                if (updateData.PlayerHandle.PlayState.IsNotInGame()) continue;

                GetInput(f, updateData, &stackInputStorage);

                //{ non-character logic

                HandleNonCharacterUpdate(f, updateData);

                //} non-character logic

                //{ character logic

                for (int i = 0; i < Constants.BATTLE_PLAYER_CHARACTER_COUNT; i++)
                {
                    BattlePlayerCharacterState characterState = updateData.PlayerHandle.GetCharacterState(i);
                    if (characterState is BattlePlayerCharacterState.OutOfPlay or BattlePlayerCharacterState.OutOfPlayDead) continue;

                    updateData.LoadPlayerCharacter(f, updateData.PlayerHandle.GetCharacterEntityRef(f, i));
                    HandleCharacterUpdate(f, updateData, selected: characterState is BattlePlayerCharacterState.InPlaySelected);
                }

                //} character logic
            }

            if (updateData.GiveUpTeam != BattleTeamNumber.NoTeam)
            {
                BattleGameControlQSystem.OnGameOverGiveUp(f, updateData.GiveUpTeam);
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

        private class UpdateData
        {
            public BattleTeamNumber GiveUpTeam = BattleTeamNumber.NoTeam;

            public BattlePlayerManager.PlayerHandle PlayerHandle { get; private set; }
            public InputData    PlayerInputData { get; private set; }

            public BattlePlayerEntityRef       PlayerCharacterEntityRef { get; private set; }
            public BattlePlayerDataQComponent* PlayerCharacterData { get; private set; }
            public Transform2D*                PlayerCharacterTransform { get; private set; }

            public void SetPlayer(BattlePlayerManager.PlayerHandle playerHandle)
            {
                PlayerHandle = playerHandle;
            }

            public void SetPlayerInput(InputData inputData)
            {
                PlayerInputData = inputData;
            }

            public void LoadPlayerCharacter(Frame f, BattlePlayerEntityRef playerEntityRef)
            {
                PlayerCharacterEntityRef = playerEntityRef;
                PlayerCharacterData = playerEntityRef.GetDataQComponent(f);
                PlayerCharacterTransform = playerEntityRef.GetTransform(f);
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
        /// <param name="stackInputStorage">Temporary input storage for bots and abandoned players.</param>
        ///
        /// <returns>Pointer to the player's input.</returns>
        private void GetInput(Frame f, UpdateData updateData, Input* stackInputStorage)
        {
            InputData inputData = new()
            {
                Input = stackInputStorage,
                CommandType = BattleCommand.Type.None,
                CommandData = null
            };

            bool isValid = false;

            if (updateData.PlayerHandle.IsBot)
            {
                BattlePlayerBotController.GetBotInput(f, updateData.PlayerHandle, inputData.Input, &inputData.CommandType, inputData.CommandData);
                isValid = inputData.Input->IsValid;
            }
            else if (!updateData.PlayerHandle.IsAbandoned)
            {
                inputData.Input = f.GetPlayerInput(updateData.PlayerHandle.PlayerRef);
                inputData.CommandType = BattleCommand.GetCommand(f, updateData.PlayerHandle.PlayerRef, out inputData.CommandData);

                BattleInputDebugUtils.InputDebugInfo inputDebugInfo = BattleInputDebugUtils.GenerateDebugInfo(inputData.Input);

                if (inputDebugInfo.NotEmpty)
                {
                    s_debugLogger.LogFormat(f,
                                            "({0}) Received input ({1}) ({2})\n" +
                                            "struct: {3}",
                                            updateData.PlayerHandle.Slot,
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

            updateData.SetPlayerInput(inputData);
        }

        /// <summary>
        /// Private helper method for handling when a player wants to give up or has abandoned the match.
        /// </summary>
        ///
        /// Used by <see cref="BattlePlayerQSystem.HandleGiveUp">HandleGiveUp</see> and <see cref="BattlePlayerQSystem.HandlePlayerAbandoned">HandlePlayerAbandoned</see>.
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        ///
        /// <returns>True if all players on a team have given up.</returns>
        private static BattleTeamNumber HandleGiveUpLogic(Frame f, BattlePlayerManager.PlayerHandle playerHandle)
        {
            BattlePlayerSlot slot = playerHandle.Slot;
            BattleTeamNumber team = BattlePlayerManager.PlayerHandle.GetTeamNumber(playerHandle.Slot);

            if (!playerHandle.GiveUpState)
            {
                f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.GiveUpVoteCancel);
                return BattleTeamNumber.NoTeam;
            }

            BattlePlayerManager.PlayerHandle teammateHandle = BattlePlayerManager.PlayerHandle.GetTeammateHandle(f, slot);
            if (teammateHandle.PlayState.IsInGame())
            {
                if (!playerHandle.IsAbandoned)
                {
                    f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.GiveUpVote);
                }
                else
                {
                    f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.Abandoned);
                }
                if (!teammateHandle.GiveUpState) return BattleTeamNumber.NoTeam;
            }
            else
            {
                f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.GiveUpNow);
            }

            return team;
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
        private void HandleGiveUp(Frame f, UpdateData updateData)
        {
            if (updateData.GiveUpTeam != BattleTeamNumber.NoTeam) return;

            BattlePlayerManager.PlayerHandle playerHandle = updateData.PlayerHandle;
            playerHandle.GiveUpState = !playerHandle.GiveUpState;

            s_debugLogger.LogFormat(f, "({0}) Give up input received, new state: {1}", playerHandle.Slot, playerHandle.GiveUpState);

            updateData.GiveUpTeam = HandleGiveUpLogic(f, playerHandle);
        }

        /// <summary>
        /// Private helper method for handling character swapping.<br/>
        /// Subprocess of <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle of the player.</param>
        /// <param name="playerCharacterNumber">Character number of the character being swapped to.</param>
        ///
        /// <returns>True if character was swapped.</returns>
        private bool HandleCharacterSwapping(Frame f, UpdateData updateData, int playerCharacterNumber)
        {
            if (playerCharacterNumber == updateData.PlayerHandle.SelectedCharacterNumber) return false;

            s_debugLogger.LogFormat(f, "({0}) Character swap input received", updateData.PlayerHandle.Slot);

            if (!updateData.PlayerHandle.AllowCharacterSwapping)
            {
                s_debugLogger.LogFormat(f, "({0}) Character swap input rejected, as AllowCharacterSwapping == false", updateData.PlayerHandle.Slot);
                return false;
            }

            s_debugLogger.LogFormat(f, "({0}) Swapping to character number: {1}", updateData.PlayerHandle.Slot, playerCharacterNumber);

            BattlePlayerManager.SpawnPlayer(f, updateData.PlayerHandle.Slot, playerCharacterNumber, select: true);
            return true;
        }

        private void HandleCharacterUpdate(Frame f, UpdateData updateData, bool selected)
        {
            updateData.PlayerCharacterData->ViewMovementVector = FPVector2.Zero;

            bool updateMovement = true;

            Input* input = updateData.PlayerInputData.Input;

            if (!updateData.PlayerCharacterData->StunCooldown.IsRunning(f))
            {
                updateData.PlayerCharacterData->MovementEnabled = !updateData.PlayerCharacterData->DisableMovement;
                updateData.PlayerCharacterData->RotationEnabled = !updateData.PlayerCharacterData->DisableRotation;
            }

            BattlePlayerClassManager.OnUpdate(f, updateData.PlayerHandle, updateData.PlayerCharacterData, updateData.PlayerCharacterEntityRef, &input->Special);

            if (!selected) return;

            switch (updateData.PlayerInputData.CommandType)
            {
                case BattleCommand.Type.ActivateAbility:
                    updateData.PlayerCharacterData->AbilityActivateBufferSec = FrameTimer.FromSeconds(f, FP._0_50);
                    break;
            }

            if (!updateData.PlayerCharacterData->AbilityCooldownSec.IsRunning(f) && updateData.PlayerCharacterData->AbilityActivateBufferSec.IsRunning(f))
            {
                AbilityActivate(f, updateData.PlayerCharacterData, updateData.PlayerCharacterTransform);
                updateMovement = false;
            }

            if (updateMovement) BattlePlayerMovementController.UpdateMovement(f, updateData.PlayerCharacterData, updateData.PlayerCharacterEntityRef, updateData.PlayerCharacterTransform, input);
        }

        private void HandleNonCharacterUpdate(Frame f, UpdateData updateData)
        {
            switch (updateData.PlayerInputData.CommandType)
            {
                case BattleCommand.Type.GiveUp:
                    HandleGiveUp(f, updateData);
                    break;

                case BattleCommand.Type.SwapCharacter:
                    BattleCharacterSwapQCommand swapCharacterData = (BattleCharacterSwapQCommand)updateData.PlayerInputData.CommandData;
                    HandleCharacterSwapping(f, updateData, swapCharacterData.CharacterNumber);
                    break;
            }

            // handle auto respawning
            if (updateData.PlayerHandle.PlayState.IsOutOfPlayRespawning() && !updateData.PlayerHandle.RespawnTimer.IsRunning(f) && updateData.PlayerHandle.AllowCharacterSwapping)
            {
                int i;

                // try to spawn next character
                for (i = 0; i < Constants.BATTLE_PLAYER_CHARACTER_COUNT; i++)
                {
                    if (updateData.PlayerHandle.GetCharacterState(i) != BattlePlayerCharacterState.OutOfPlayDead)
                    {
                        s_debugLogger.LogFormat(f, "({0}) Auto spawning character number: {1}", updateData.PlayerHandle.Slot, i);

                        BattlePlayerManager.SpawnPlayer(f, updateData.PlayerHandle.Slot, i, select: true);
                        break;
                    }
                }

                // handle out of characters
                if (i == Constants.BATTLE_PLAYER_CHARACTER_COUNT)
                {
                    s_debugLogger.LogFormat(f, "({0}) Player is out of characters!", updateData.PlayerHandle.Slot);

                    updateData.PlayerHandle.SetOutOfPlayFinal();
                }
            }
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
