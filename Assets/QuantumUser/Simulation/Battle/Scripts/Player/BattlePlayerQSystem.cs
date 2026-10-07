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
        /// Calls <see cref="BattlePlayerManager.SpawnPlayerCharacter">BattlePlayerManager.SpawnPlayer</see> for players that are in the game.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public static void SpawnPlayers(Frame f)
        {
            foreach (BattlePlayerData.Ref playerData in BattlePlayerManager.GetPlayerDataIterator(f))
            {
                if (playerData.PlayState.IsNotInGame()) continue;

                BattlePlayerManager.SpawnPlayerCharacter(f, playerData.Slot, 0, select: true);
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
        public static void HandlePlayerAbandoned(Frame f, BattlePlayerData.Ref playerData)
        {
            playerData.Low_Level.StatePlayerGiveUp = true;

            BattleTeamNumber giveUpTeam = HandleGiveUpLogic(f, playerData);
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
        public static void OnProjectileHitPlayerCharacter(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData)
        {
            if (projectileCollisionData->Projectile.Data->IsHeld) return;

            // get spec
            BattlePlayerHandle playerHandle = BattlePlayerHandle.Create(f, projectileCollisionData->OtherEntityRef);
            BattlePlayerCharacterShieldHandle shieldHandle = playerHandle.GetLoadedCharacterAttachedShield(f);
            BattlePlayerQSpec playerSpec = BattleQConfig.GetPlayerSpec(f);

            if (playerHandle.LoadedCharacterData->StateStunCooldown.IsRunning(f) || playerHandle.LoadedCharacterData->StateShieldHitCooldown.IsRunning(f)) goto Exit;

            if (playerHandle.LoadedCharacterData->StateDefenceValue > 0)
            {
                // handle stun

                playerHandle.LoadedCharacterData->StateMovementEnabled = false;
                playerHandle.LoadedCharacterData->StateRotationEnabled = false;
                playerHandle.LoadedCharacterData->StateStunCooldown    = FrameTimer.FromSeconds(f, playerSpec.StunDurationSec);

                SoundEffectTypeCommon soundEffectType = projectileCollisionData->ProjectileEmotionCurrent switch
                {
                    BattleEmotionState.Aggression => SoundEffectTypeCommon.HitCharacterAggression,
                    BattleEmotionState.Joy        => SoundEffectTypeCommon.HitCharacterJoy,
                    BattleEmotionState.Love       => SoundEffectTypeCommon.HitCharacterLove,
                    BattleEmotionState.Playful    => SoundEffectTypeCommon.HitCharacterPlayful,
                    BattleEmotionState.Sadness    => SoundEffectTypeCommon.HitCharacterSadness,

                    _ => throw new System.NotImplementedException()
                };
                HandleSFXCommon(f, playerHandle.PlayerData.Slot, soundEffectType, SoundEffectTarget.All);

                f.Events.BattleCharacterHit(
                    playerHandle.LoadedCharacterEntityRef,
                    playerHandle.PlayerData.Team,
                    playerHandle.PlayerData.Slot,
                    playerHandle.LoadedCharacterData->Number,
                    shieldHandle.ShieldData->ShieldNumber,
                    playerSpec.StunDurationSec,
                    projectileCollisionData->ProjectileEmotionCurrent
                );
            }
            else
            {
                // handle death

                int characterNumber = playerHandle.LoadedCharacterData->Number;

                BattlePlayerManager.DespawnPlayerCharacter(f, playerHandle, characterNumber, kill: true);
                if (playerHandle.PlayerData.PlayState.IsOutOfPlay())
                {
                    playerHandle.PlayerData.SetOutOfPlayRespawning();
                    playerHandle.PlayerData.Low_Level.RespawnTimer = FrameTimer.FromSeconds(f, playerSpec.AutoRespawnTimeSec);
                }

                HandleSFXCommon(f, playerHandle.PlayerData.Slot, SoundEffectTypeCommon.Death, SoundEffectTarget.All);
                f.Events.BattleCharacterDeath(playerHandle.PlayerData.Slot, characterNumber);
            }

        Exit:
            projectileCollisionData->Projectile.SetCollisionFlag(f, BattleProjectileCollisionFlags.Player);
        }

        /// <summary>
        /// Called by BattleCollisionQSystem. Applies damage to the player's shield after checking if it is appropriate to do so.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame</param>
        /// <param name="projectileCollisionData">Collision data related to the projectile.</param>
        /// <param name="shieldCollisionData">Collision data related to the player shield.</param>
        public static void OnProjectileHitPlayerShield(Frame f, BattleCollisionQSystem.ProjectileCollisionData* projectileCollisionData)
        {
            // checks
            if (projectileCollisionData->Projectile.Data->IsHeld) return;

            //{ hit

            BattlePlayerHandle                playerHandle = BattlePlayerHandle.Create(f, projectileCollisionData->OtherEntityRef);
            BattlePlayerCharacterShieldHandle shieldHandle = BattlePlayerCharacterShieldHandle.Create(f, projectileCollisionData->OtherEntityRef);

            int  characterNumber     = playerHandle.LoadedCharacterData->Number;
            bool defenceUpdateVisual = false;
            FP   defencePercentage   = -1;

            if (shieldHandle.ShieldData->ShieldHitCooldown.IsRunning(f)) goto ExitNoHit;

            HandleSFXCommon(f, playerHandle.PlayerData.Slot, SoundEffectTypeCommon.HitShield, SoundEffectTarget.Player);

            //} hit

            //{ hit attach

            if (!shieldHandle.ShieldData->IsAttached) goto ExitHit;

            FP damageTaken = projectileCollisionData->Projectile.Data->Attack;

            projectileCollisionData->Projectile.SetAttack(f, playerHandle.LoadedCharacterData->Stats.Attack);

            if (damageTaken <= FP._0) goto ExitNoHit;

            playerHandle.LoadedCharacterData->StateDefenceValue = playerHandle.LoadedCharacterData->StateDefenceValue - damageTaken;

            defenceUpdateVisual = true;
            defencePercentage = playerHandle.LoadedCharacterData->StateDefenceValue / playerHandle.LoadedCharacterData->Stats.Defence;

            if (playerHandle.LoadedCharacterData->StateDefenceValue <= 0)
            {
                s_debugLogger.LogFormat(f, "({0}) Current characters shield destroyed!", playerHandle.PlayerData.Slot);

                BattlePlayerShieldManager.RemoveShield(f, playerHandle, characterNumber, shieldHandle.ShieldData->ShieldNumber);
            }

            //} hit attach

        ExitHit:
            FP damageCooldownSec                       = BattleQConfig.GetPlayerSpec(f).DamageCooldownSec;
            shieldHandle.ShieldData->ShieldHitCooldown = FrameTimer.FromSeconds(f, damageCooldownSec);
            if (playerHandle.LoadedCharacterHasShieldAttached)
            {
                playerHandle.LoadedCharacterData->StateShieldHitCooldown = FrameTimer.FromSeconds(f, damageCooldownSec);
            }

            f.Events.BattleShieldHit(
                shieldHandle.ShieldEntityRef,
                playerHandle.PlayerData.Team,
                playerHandle.PlayerData.Slot,
                characterNumber,
                shieldHandle.ShieldData->ShieldNumber,
                defenceUpdateVisual,
                defencePercentage
            );
        ExitNoHit:
            projectileCollisionData->Projectile.SetCollisionFlag(f, BattleProjectileCollisionFlags.Player);
        }

        /// <summary>
        /// Calls <see cref="BattlePlayerClassManager.OnGameStart">BattlePlayerClassManager.OnGameStart</see> for every player's every character.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        public static void OnGameStart(Frame f)
        {
            foreach (BattlePlayerData.Ref playerData in BattlePlayerManager.GetPlayerDataIterator(f))
            {
                if (playerData.PlayState.IsNotInGame()) continue;
                BattlePlayerHandle playerHandle = BattlePlayerHandle.Create(playerData);

                for (int characterNumber = 0; characterNumber < Constants.BATTLE_PLAYER_CHARACTER_COUNT; characterNumber++)
                {
                    bool selected = characterNumber == playerData.SelectedCharacterNumber;
                    playerHandle.LoadCharacter(f, characterNumber);

                    BattlePlayerClassManager.OnGameStart(f, playerHandle, selected);
                }
            }
        }

        /// <summary>
        /// <span class="brief-h"><a href="https://doc.photonengine.com/quantum/current/manual/quantum-ecs/systems">Quantum System Update method@u-exlink</a> gets called every frame.</span><br/>
        /// Relays the appropriate input data to each player in the game
        /// </summary>
        ///
        /// Update method has been split into subprocesses:
        /// - @cref{BattlePlayerQSystem,GetInput}
        /// - @cref{BattlePlayerQSystem,HandleNonCharacterUpdate}
        ///   - @cref{BattlePlayerQSystem,HandleGiveUp}
        ///   - @cref{BattlePlayerQSystem,HandleCharacterSwapping}
        /// - @cref{BattlePlayerQSystem,HandleCharacterUpdate}
        ///   - @cref{BattlePlayerQSystem,AbilityActivate}
        ///
        /// The Update method and it's subprocesses use @cref{Battle.QSimulation.Player.BattlePlayerQSystem,UpdateData}
        /// to store and share all necessary data used by the update logic.
        ///
        /// <param name="f">Current simulation frame</param>
        public override void Update(Frame f)
        {
            BattleGameSessionQSingleton* singleton = f.Unsafe.GetPointerSingleton<BattleGameSessionQSingleton>();
            if (singleton->State != BattleGameState.Playing) return;

            UpdateData updateData = new();
            Input stackInputStorage;

            foreach (BattlePlayerData.Ref playerData in BattlePlayerManager.GetPlayerDataIterator(f))
            {
                if (playerData.PlayState.IsNotInGame()) continue;

                updateData.SetPlayer(BattlePlayerHandle.Create(playerData));

                GetInput(f, updateData, &stackInputStorage);

                // non-character logic
                HandleNonCharacterUpdate(f, updateData);

                //{ character logic

                for (int characterNumber = 0; characterNumber < Constants.BATTLE_PLAYER_CHARACTER_COUNT; characterNumber++)
                {
                    updateData.PlayerHandle.LoadCharacter(f, characterNumber);

                    BattlePlayerCharacterState characterState = updateData.PlayerHandle.LoadedCharacterData->State;
                    if (characterState is BattlePlayerCharacterState.OutOfPlay or BattlePlayerCharacterState.OutOfPlayDead) continue;

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

        /// <summary>
        /// Class containing all necessary data for player <see cref="BattlePlayerQSystem.Update">Update</see> logic.
        /// </summary>
        ///
        /// @anchor BattlePlayerQSystem-UpdateData-DetailedDescription
        ///
        /// Created at the start of the @cref{Battle.QSimulation.Player.BattlePlayerQSystem,Update} method.
        ///
        /// The data is split into 3 categories:
        /// - @ref BattlePlayerQSystem-UpdateData-CommonData "Common Data"<br/>
        /// Common data related to the update logic.
        /// - @ref BattlePlayerQSystem-UpdateData-CurrentPlayer "Current Player Properties"<br/>
        /// Data related to the current player that is being processed.<br/>
        /// Is set for each player that is being processed using
        /// @cref{Battle.QSimulation.Player.BattlePlayerQSystem.UpdateData,SetPlayer}.
        /// - @ref BattlePlayerQSystem-UpdateData-CurrentPlayerCharacter "Current Player Character Properties"<br/>
        /// Data related to the current player character that is being processed.<br/>
        /// Is loaded for each player character that is being processed using
        /// @cref{Battle.QSimulation.Player.BattlePlayerQSystem.UpdateData,LoadPlayerCharacter}.
        private class UpdateData
        {
            /// @anchor BattlePlayerQSystem-UpdateData-CommonData
            /// @name Common Data
            /// Common data related to the update logic. @ref BattlePlayerQSystem-UpdateData-DetailedDescription "More..."
            /// @{

            /// <summary>
            /// Used to keep track of the team that wants to give up.
            /// </summary>
            ///
            /// Part of @ref BattlePlayerQSystem-UpdateData-CommonData "Common Data"
            public BattleTeamNumber GiveUpTeam = BattleTeamNumber.NoTeam;
            /// @}

            /// @anchor BattlePlayerQSystem-UpdateData-CurrentPlayer
            /// @name Current Player Properties
            /// Data related to the current player that is being processed. @ref BattlePlayerQSystem-UpdateData-DetailedDescription "More..."
            /// @{

            /// <summary>
            /// PlayerHandle of the current player.
            /// </summary>
            ///
            /// Part of @ref BattlePlayerQSystem-UpdateData-CurrentPlayer "Current Player Properties"
            //public BattlePlayerManager.PlayerHandle PlayerHandle { get; private set; }

            public BattlePlayerHandle PlayerHandle;

            /// <summary>
            /// %Input data of the current player.<br/>
            /// Is not set by <see cref="Battle.QSimulation.Player.BattlePlayerQSystem.UpdateData.SetPlayer">SetPlayer</see>.
            /// Needs to be set separately using <see cref="Battle.QSimulation.Player.BattlePlayerQSystem.UpdateData.SetPlayerInput">SetPlayerInput</see>.
            /// </summary>
            ///
            /// This property needs to be set separately because the current player's @cref{Battle.QSimulation.Player.BattlePlayerQSystem,InputData}
            /// is fetched later in the @cref{Battle.QSimulation.Player.BattlePlayerQSystem,Update} logic.
            ///
            /// Part of @ref BattlePlayerQSystem-UpdateData-CurrentPlayer "Current Player Properties"
            public InputData PlayerInputData { get; private set; }

            /// <summary>
            /// Has the current player swapped character this frame or not.
            /// </summary>
            ///
            /// Part of @ref BattlePlayerQSystem-UpdateData-CurrentPlayer "Current Player Properties"
            public bool PlayerHasSwappedCharacter { get; set; }
            /// @}

            /// <summary>
            /// Sets the current player.
            /// </summary>
            ///
            /// See @ref BattlePlayerQSystem-UpdateData-DetailedDescription "Detailed Description" for more info.
            ///
            /// <param name="playerHandle">Player handle of the player being set.</param>
            public void SetPlayer(BattlePlayerHandle playerHandle)
            {
                PlayerHandle              = playerHandle;
                PlayerHasSwappedCharacter = false;
            }

            /// <summary>
            /// Sets the current player's input data.
            /// </summary>
            ///
            /// See @cref{Battle.QSimulation.Player.BattlePlayerQSystem.UpdateData,PlayerInputData} for more info.
            ///
            /// <param name="inputData">Input data being set.</param>
            public void SetPlayerInput(InputData inputData)
            {
                PlayerInputData = inputData;
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
        /// <param name="updateData">Reference to <see cref="BattlePlayerQSystem.UpdateData"/>.</param>
        /// <param name="stackInputStorage">Temporary input storage for bots and abandoned players.</param>
        private void GetInput(Frame f, UpdateData updateData, Input* stackInputStorage)
        {
            InputData inputData = new()
            {
                Input = stackInputStorage,
                CommandType = BattleCommand.Type.None,
                CommandData = null
            };

            bool isValid = false;

            if (updateData.PlayerHandle.PlayerData.IsBot)
            {
                BattlePlayerBotController.BotInputData botInput = BattlePlayerBotController.GetBotInput(f, updateData.PlayerHandle);
                *stackInputStorage    = botInput.Input;
                inputData.CommandType = botInput.CommandType;
                inputData.CommandData = botInput.CommandData;

                isValid = inputData.Input->IsValid;
            }
            else if (!updateData.PlayerHandle.PlayerData.IsAbandoned)
            {
                inputData.Input = f.GetPlayerInput(updateData.PlayerHandle.PlayerData.PRef);
                inputData.CommandType = BattleCommand.GetCommand(f, updateData.PlayerHandle.PlayerData.PRef, out inputData.CommandData);

                BattleInputDebugUtils.InputDebugInfo inputDebugInfo = BattleInputDebugUtils.GenerateDebugInfo(inputData.Input);

                if (inputDebugInfo.NotEmpty)
                {
                    s_debugLogger.LogFormat(f,
                                            "({0}) Received input ({1}) ({2})\n" +
                                            "struct: {3}",
                                            updateData.PlayerHandle.PlayerData.Slot,
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
        private static BattleTeamNumber HandleGiveUpLogic(Frame f, BattlePlayerData.Ref playerData)
        {
            BattlePlayerSlot slot = playerData.Slot;
            BattleTeamNumber team = playerData.Team;

            if (!playerData.StatePlayerGiveUp)
            {
                f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.GiveUpVoteCancel);
                return BattleTeamNumber.NoTeam;
            }

            BattlePlayerData.Ref teammateData = BattlePlayerManager.GetPlayerData(f, BattlePlayerManager.GetTeammateSlot(playerData.Slot));
            if (teammateData.PlayState.IsInGame())
            {
                if (!playerData.IsAbandoned)
                {
                    f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.GiveUpVote);
                }
                else
                {
                    f.Events.BattleGiveUpStateChange(team, slot, BattleGiveUpStateUpdate.Abandoned);
                }
                if (!teammateData.StatePlayerGiveUp) return BattleTeamNumber.NoTeam;
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
                    BattleTeamNumber teamNumber = BattlePlayerManager.GetPlayerData(f, slot).Team;
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
        /// <param name="updateData">Reference to <see cref="BattlePlayerQSystem.UpdateData"/>.</param>
        private void HandleGiveUp(Frame f, UpdateData updateData)
        {
            if (updateData.GiveUpTeam != BattleTeamNumber.NoTeam) return;

            BattlePlayerData.Ref playerData = updateData.PlayerHandle.PlayerData;
            playerData.Low_Level.StatePlayerGiveUp = !playerData.StatePlayerGiveUp;

            s_debugLogger.LogFormat(f, "({0}) Give up input received, new state: {1}", playerData.Slot, playerData.StatePlayerGiveUp);

            updateData.GiveUpTeam = HandleGiveUpLogic(f, playerData);
        }

        /// <summary>
        /// Private helper method for handling character swapping.<br/>
        /// Subprocess of <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="updateData">Reference to <see cref="BattlePlayerQSystem.UpdateData"/>.</param>
        /// <param name="playerCharacterNumber">Character number of the character being swapped to.</param>
        private void HandleCharacterSwapping(Frame f, UpdateData updateData, int playerCharacterNumber)
        {
            if (playerCharacterNumber == updateData.PlayerHandle.PlayerData.SelectedCharacterNumber) return;

            s_debugLogger.LogFormat(f, "({0}) Character swap input received", updateData.PlayerHandle.PlayerData.Slot);

            if (!updateData.PlayerHandle.PlayerData.StateAllowCharacterSwapping)
            {
                s_debugLogger.LogFormat(f, "({0}) Character swap input rejected, as AllowCharacterSwapping == false", updateData.PlayerHandle.PlayerData.Slot);
                return;
            }

            s_debugLogger.LogFormat(f, "({0}) Swapping to character number: {1}", updateData.PlayerHandle.PlayerData.Slot, playerCharacterNumber);

            bool select = true;
            BattlePlayerManager.SpawnPlayerCharacter(f, updateData.PlayerHandle.PlayerData.Slot, playerCharacterNumber, select);
            updateData.PlayerHasSwappedCharacter = select;
        }

        /// <summary>
        /// Private helper method for handling non character update.<br/>
        /// Subprocess of <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="updateData">Reference to <see cref="BattlePlayerQSystem.UpdateData"/>.</param>
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
            if (updateData.PlayerHandle.PlayerData.PlayState.IsOutOfPlayRespawning() && !updateData.PlayerHandle.PlayerData.RespawnTimer.IsRunning(f) && updateData.PlayerHandle.PlayerData.StateAllowCharacterSwapping)
            {
                int i;

                // try to spawn next character
                for (i = 0; i < Constants.BATTLE_PLAYER_CHARACTER_COUNT; i++)
                {
                    if (updateData.PlayerHandle.LoadedCharacterData->State != BattlePlayerCharacterState.OutOfPlayDead)
                    {
                        s_debugLogger.LogFormat(f, "({0}) Auto spawning character number: {1}", updateData.PlayerHandle.PlayerData.Slot, i);

                        BattlePlayerManager.SpawnPlayerCharacter(f, updateData.PlayerHandle.PlayerData.Slot, i, select: true);
                        break;
                    }
                }

                // handle out of characters
                if (i == Constants.BATTLE_PLAYER_CHARACTER_COUNT)
                {
                    s_debugLogger.LogFormat(f, "({0}) Player is out of characters!", updateData.PlayerHandle.PlayerData.Slot);

                    updateData.PlayerHandle.PlayerData.SetOutOfPlayFinal();
                }
            }
        }

        /// <summary>
        /// Private helper method for handling character update.<br/>
        /// Subprocess of <see cref="BattlePlayerQSystem.Update">Update</see> method.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="updateData">Reference to <see cref="BattlePlayerQSystem.UpdateData"/>.</param>
        /// <param name="selected"></param>
        private void HandleCharacterUpdate(Frame f, UpdateData updateData, bool selected)
        {
            if (updateData.PlayerHasSwappedCharacter) selected = false;

            updateData.PlayerHandle.LoadedCharacterData->ViewMovementVector = FPVector2.Zero;

            bool updateMovement = true;

            Input* input = updateData.PlayerInputData.Input;

            if (!updateData.PlayerHandle.LoadedCharacterData->StateStunCooldown.IsRunning(f))
            {
                updateData.PlayerHandle.LoadedCharacterData->StateMovementEnabled = !updateData.PlayerHandle.LoadedCharacterData->AttributeDisableMovement;
                updateData.PlayerHandle.LoadedCharacterData->StateRotationEnabled = !updateData.PlayerHandle.LoadedCharacterData->AttributeDisableRotation;
            }

            BattlePlayerClassManager.OnUpdate(f, updateData.PlayerHandle, &input->Special);

            if (!selected) return;

            switch (updateData.PlayerInputData.CommandType)
            {
                case BattleCommand.Type.ActivateAbility:
                    updateData.PlayerHandle.LoadedCharacterData->AbilityActivateBufferSec = FrameTimer.FromSeconds(f, FP._0_50);
                    break;
            }

            if (!updateData.PlayerHandle.LoadedCharacterData->AbilityCooldownSec.IsRunning(f) && updateData.PlayerHandle.LoadedCharacterData->AbilityActivateBufferSec.IsRunning(f))
            {
                AbilityActivate(f, updateData.PlayerHandle);
                updateMovement = false;
            }

            if (updateMovement) BattlePlayerMovementController.UpdateMovement(f, updateData.PlayerHandle, input);
        }

        private void AbilityActivate(Frame f, BattlePlayerHandle playerHandle)
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

            playerHandle.LoadedCharacterData->AbilityCooldownSec = FrameTimer.FromSeconds(f, FP._3);
        }
    }
}
