/// @file BattlePlayerClass100Test.cs
/// <summary>
/// Contains @cref{Battle.QSimulation.Player,BattlePlayerClass100Test} class which handles player character class logic for the 100/Desensitizer class.
/// </summary>

// Quantum usings
using Quantum;
using Photon.Deterministic;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation.Player
{
    /// <summary>
    /// %Player character class logic for the 100/Desensitizer class.
    /// </summary>
    ///
    /// @bigtext{See [{PlayerClass}](#page-concepts-player-simulation-class-playerclass) for more info.}<br/>
    /// @bigtext{See [{Player Character Classes}](#page-concepts-player-characters-classes) for more info.}<br/>
    /// @bigtext{See [{Player Class 100}](#page-concepts-player-class-100) for more info.}
    public class BattlePlayerClass100Test : BattlePlayerClassBase<BattlePlayerClass100DataQComponent>
    {
        /// <summary>
        /// Gets the character class associated with this Class.<br/>
        /// Always returns <see cref="Quantum.BattlePlayerCharacterClass.Class100"></see>.
        /// </summary>
        public override BattlePlayerCharacterClass Class { get; } = BattlePlayerCharacterClass.Class100;

        /// <summary>
        /// Called when the player is spawned.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle for the player.</param>
        /// <param name="playerData">Pointer to player data.</param>
        /// <param name="playerEntity">Entity reference for the player.</param>
        public override unsafe void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            f.Events.BattleSpecialJoystickVisibilityChange(playerData->Slot, true);
        }

        /// <summary>
        /// Called when the player is despawned.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle for the player.</param>
        /// <param name="playerData">Pointer to player data.</param>
        /// <param name="playerEntity">Entity reference for the player.</param>
        public override unsafe void OnDespawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            f.Events.BattleSpecialJoystickVisibilityChange(playerData->Slot, false);
        }

        /// <summary>
        /// Called every frame to update the player.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle for the player.</param>
        /// <param name="playerData">Pointer to player data.</param>
        /// <param name="playerEntity">Entity reference for the player.</param>
        /// <param name="specialInput">Pointer to special input (unused)</param>
        public override unsafe void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, BattlePlayerEntityRef playerEntity, BattleSpecialInput* specialInput)
        /// <summary>
        /// Called when the game starts to start the placement timer.
        /// </summary>
        ///
        /// <param name="f">Current simulation frame.</param>
        /// <param name="playerHandle">Handle for the player.</param>
        /// <param name="playerData">Pointer to player data.</param>
        /// <param name="playerEntity">Entity reference to the player.</param>
        public override unsafe void OnGameStart(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            BattlePlayerClass100QSpec spec = BattleQConfig.GetBattlePlayerClass100Spec(f);

            BattlePlayerClass100DataQComponent* classData = GetClassData(f, playerEntity);

            classData->PlacementTimer = FrameTimer.FromSeconds(f, spec.PlacementTimeDurationSec);
        }
        {
            BattlePlayerClass100QSpec spec = BattleQConfig.GetBattlePlayerClass100Spec(f);
            //BattleDebugLogger.WarningFormat(f, nameof(BattlePlayerClass100), "Joystick ( state: {0}, Direction: {1} )", specialInput->JoystickState, specialInput->JoystickValue);

            Transform2D*                        playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
            BattlePlayerClass100DataQComponent* classData       = GetClassData(f, playerEntity);

            bool joystickDown         = specialInput->JoystickState != BattleJoystickState.Up;
            bool projectileOnCooldown = classData->CooldownTimer.IsRunning(f);

            // Update view
            if (joystickDown && !projectileOnCooldown) f.Events.BattlePlayerClass100AimIndicatorUpdate(playerEntity, playerData->Slot, Show: true, specialInput->JoystickValue);

            // Exit if no changes in joystick state
            if (joystickDown == classData->JoystickDownPrevious) goto Exit;

            if (joystickDown)
            {
                // Handle joystick down
                classData->JoystickTimer = FrameTimer.FromSeconds(f, spec.JoystickTapDurationMax);
            }
            else
            {
                // Handle joystick up

                // Update view
                f.Events.BattlePlayerClass100AimIndicatorUpdate(playerEntity, playerData->Slot, Show: false, FPVector2.Zero);

                // exit if projectile ability is on cooldown
                if (projectileOnCooldown) goto Exit;

                bool isJoystickTap = classData->JoystickTimer.IsRunning(f) && classData->JoystickValuePrevious.Magnitude < spec.JoystickTapDistanceMax;

                FPVector2 direction = isJoystickTap ? FPVector2.Up : classData->JoystickValuePrevious.Normalized;

                if (playerData->TeamNumber == BattleTeamNumber.TeamBeta) direction = FPVector2.Rotate(direction, FP.Rad_180);
                FPVector2 position = playerTransform->Position + direction * spec.ProjectileSpawnDistance;
                BattlePlayerClass100ProjectileQSystem.Create(f, f.FindAsset(spec.ProjectileEntityPrototype), position, direction, spec.ProjectileSpeed);

                // start projectile cooldown
                classData->CooldownTimer = FrameTimer.FromSeconds(f, spec.ProjectileSpawnCooldown);
            }

        Exit:
            classData->JoystickDownPrevious = joystickDown;
            classData->JoystickValuePrevious = specialInput->JoystickValue;
        }
    }
}
