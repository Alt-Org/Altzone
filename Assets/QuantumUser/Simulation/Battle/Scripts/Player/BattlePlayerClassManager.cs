using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Quantum;
using UnityEngine;

namespace Battle.QSimulation.Player
{
    public abstract unsafe class BattlePlayerClassBase<T> : IBattlePlayerClass where T : unmanaged, IComponent
    {
        public abstract BattlePlayerCharacterClass Class { get; }

        public virtual unsafe void OnCreate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }
        public virtual unsafe void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }
        public virtual unsafe void OnDespawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }

        public virtual unsafe void OnProjectileHitPlayerHitbox(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity) { }
        public virtual unsafe void OnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity) { }
        public virtual unsafe void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity) { }

        protected T* GetClassData(Frame f, EntityRef playerEntity)
        {
            T* value = f.Unsafe.GetPointer<T>(playerEntity);
            if (value == null)
            {
                Debug.LogErrorFormat("[PlayerCharacterClass] Class data for {0} could not be found!", playerEntity);
            }
            return value;
        }
    }

    public unsafe interface IBattlePlayerClass
    {
        public BattlePlayerCharacterClass Class { get; }

        public void OnCreate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity);
        public void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity);
        public void OnDespawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity);

        public void OnProjectileHitPlayerHitbox(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity);
        public void OnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity);
        public void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity);
    }

    public unsafe static class BattlePlayerClassManager
    {
        public static void LoadClass(BattlePlayerCharacterClass characterClass)
        {
            switch (characterClass)
            {
                case BattlePlayerCharacterClass.Desensitizer:
                    if (s_classArray[ClassIndexDesensitizer] != null) break;
                    s_classArray[ClassIndexDesensitizer] = new BattlePlayerClassDesensitizer();
                    break;

                case BattlePlayerCharacterClass.Trickster:
                    if (s_classArray[ClassIndexTrickster] != null) break;
                    //s_classArray[ClassIndexDesensitizer] = new BattlePlayerClassTrickster();
                    break;

                case BattlePlayerCharacterClass.Obedient:
                    if (s_classArray[ClassIndexObedient] != null) break;
                    //s_classArray[ClassIndexDesensitizer] = new BattlePlayerClassObedient();
                    break;

                case BattlePlayerCharacterClass.Projector:
                    if (s_classArray[ClassIndexProjector] != null) break;
                    //s_classArray[ClassIndexDesensitizer] = new BattlePlayerClassProjector();
                    break;

                case BattlePlayerCharacterClass.Retroflector:
                    if (s_classArray[ClassIndexRetroflector] != null) break;
                    //s_classArray[ClassIndexDesensitizer] = new BattlePlayerClassRetroflector();
                    break;

                case BattlePlayerCharacterClass.Confluent:
                    if (s_classArray[ClassIndexConfluent] != null) break;
                    //s_classArray[ClassIndexDesensitizer] = new BattlePlayerClassConfluent();
                    break;

                case BattlePlayerCharacterClass.Intellectualizer:
                    if (s_classArray[ClassIndexIntellectualizer] != null) break;
                    //s_classArray[ClassIndexDesensitizer] = new BattlePlayerClassIntellectualizer();
                    break;

                default:
                    break;
            }
        }

        public static void OnCreate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            IBattlePlayerClass playerClass = GetClass(playerData->CharacterClass);

            if (playerClass == null)
            {
                Debug.LogWarningFormat("[PlayerClassManager] The {0} class could not be initialized!", playerData->CharacterClass);
                return;
            }

            playerClass.OnCreate(f, playerHandle, playerData, playerEntity);
        }

        public static void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            IBattlePlayerClass playerClass = GetClass(playerData->CharacterClass);

            if (playerClass == null) return;

            playerClass.OnSpawn(f, playerHandle, playerData, playerEntity);
        }

        public static void OnDespawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            IBattlePlayerClass playerClass = GetClass(playerData->CharacterClass);

            if (playerClass == null) return;

            playerClass.OnDespawn(f, playerHandle, playerData, playerEntity);
        }

        public static void OnProjectileHitPlayerHitbox(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            IBattlePlayerClass playerClass = GetClass(f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity)->CharacterClass);

            if (playerClass == null) return;

            playerClass.OnProjectileHitPlayerHitbox(f, projectile, projectileEntity, playerHitbox, playerHitboxEntity);
        }

        public static void OnProjectileHitPlayerShield(Frame f, BattleProjectileQComponent* projectile, EntityRef projectileEntity, BattlePlayerHitboxQComponent* playerHitbox, EntityRef playerHitboxEntity)
        {
            IBattlePlayerClass playerClass = GetClass(f.Unsafe.GetPointer<BattlePlayerDataQComponent>(playerHitbox->PlayerEntity)->CharacterClass);

            if (playerClass == null) return;

            playerClass.OnProjectileHitPlayerShield(f, projectile, projectileEntity, playerHitbox, playerHitboxEntity);
        }

        public static void OnUpdate(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            IBattlePlayerClass playerClass = GetClass(playerData->CharacterClass);

            if (playerClass == null) return;

            playerClass.OnUpdate(f, playerHandle, playerData, playerEntity);
        }

        private const int ClassIndexDesensitizer = 0;
        private const int ClassIndexTrickster = 1;
        private const int ClassIndexObedient = 2;
        private const int ClassIndexProjector = 3;
        private const int ClassIndexRetroflector = 4;
        private const int ClassIndexConfluent = 5;
        private const int ClassIndexIntellectualizer = 6;
        private const int ClassCount = 7;

        private static IBattlePlayerClass[] s_classArray = new IBattlePlayerClass[ClassCount];

        private static Dictionary<BattlePlayerCharacterClass, bool> s_errorMessagesSent = new();

        private static IBattlePlayerClass GetClass(BattlePlayerCharacterClass characterClass)
        {
            int classIndex = characterClass switch
            {
                BattlePlayerCharacterClass.Desensitizer     => ClassIndexDesensitizer,
                //BattlePlayerCharacterClass.Trickster        => ClassIndexTrickster,
                //BattlePlayerCharacterClass.Obedient         => ClassIndexObedient,
                //BattlePlayerCharacterClass.Projector        => ClassIndexProjector,
                //BattlePlayerCharacterClass.Retroflector     => ClassIndexRetroflector,
                //BattlePlayerCharacterClass.Confluent        => ClassIndexConfluent,
                //BattlePlayerCharacterClass.Intellectualizer => ClassIndexIntellectualizer,

                _ => -1,
            };

            if (classIndex == -1)
            {
                if (!s_errorMessagesSent.ContainsKey(characterClass))
                {
                    s_errorMessagesSent.Add(characterClass, false);
                }

                if (!s_errorMessagesSent[characterClass])
                {
                    Debug.LogWarningFormat("[PlayerClassManager] The {0} class is not currently implemented!", characterClass);
                    s_errorMessagesSent[characterClass] = true;
                }
                
                return null;
            }

            IBattlePlayerClass classObj = s_classArray[classIndex];

            if (classObj == null)
            {
                Debug.LogErrorFormat("[PlayerClassManager] The {0} class is not in the class array!", characterClass);
            }

            if (classObj.Class != characterClass)
            {
                Debug.LogErrorFormat("[PlayerClassManager] Mapping of character classes is incorrect! Expected {0}, got {1}", characterClass, classObj.Class);
            }

            return classObj;
        }
    }
}
