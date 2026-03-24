// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;
using Photon.Deterministic;
using System.Threading;

namespace Battle.QSimulation.Player
{
    public class BattlePlayerClassDesensitizer : BattlePlayerClassBase<BattlePlayerClassDesensitizerDataQComponent>
    {
        public override BattlePlayerCharacterClass Class { get; } = BattlePlayerCharacterClass.Desensitizer;

        public override unsafe void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            Debug.Log("Desensitizer spawned");

            
        }

        public override unsafe void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity, BattleSpecialInput* specialInput)
        {
            FP spawnDistance = FP._0_50;
            FP speed = FP._0_10;
            BattleDebugLogger.WarningFormat(f, nameof(BattlePlayerClassDesensitizer), "Joystick ( state: {0}, Direction: {1} )", specialInput->JoystickState, specialInput->JoystickValue);
            
            Transform2D* playerTransform = f.Unsafe.GetPointer<Transform2D>(playerEntity);
            BattlePlayerClassDesensitizerDataQComponent* classData = GetClassData(f, playerEntity);
            bool joystickDown = specialInput->JoystickState != BattleJoystickState.Up;
            
            // Update view
            if (joystickDown) f.Events.BattlePlayerClassDesensitizerAimIndicatorUpdate(playerEntity, playerData->Slot, Show: true, specialInput->JoystickValue);
            
            // Exit if no changes in joystick state
            if (joystickDown == classData->JoystickDownPrevious) goto Exit;
            
            if (joystickDown) 
            {
                // Handle joystick down
                classData->JoystickTimer = FrameTimer.FromSeconds(f, FP._0_50);
            }
            else
            {
                // Handle joystick up
                FPVector2 direction = classData->JoystickTimer.IsRunning(f) ? FPVector2.Up : classData->JoystickValuePrevious.Normalized;
                if (playerData->TeamNumber == BattleTeamNumber.TeamBeta) direction = FPVector2.Rotate(direction, FP.Rad_180);
                FPVector2 position = playerTransform->Position + direction * spawnDistance;
                BattlePlayerClassDesensitizerProjectileQSystem.Create(f, f.FindAsset(classData->ProjectileEntityPrototype), position, direction, speed);
                
                // Update view
                f.Events.BattlePlayerClassDesensitizerAimIndicatorUpdate(playerEntity, playerData->Slot, Show: false, FPVector2.Zero);
            }

            Exit:
            classData->JoystickDownPrevious = joystickDown;
            classData->JoystickValuePrevious = specialInput->JoystickValue;
        }
    }
}
