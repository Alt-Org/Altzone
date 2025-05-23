using System;
using System.Runtime.CompilerServices;

using Quantum;

using Battle.QSimulation.Projectile;
using Battle.QSimulation.SoulWall;
using Battle.QSimulation.Diamond;

namespace Battle.QSimulation.Game
{
    public class BattleQConfig : AssetObject
    {
        public AssetRef<BattleArenaQSpec>      BattleArenaSpec;
        public AssetRef<BattleSoulWallQSpec>   BattleSoulWallSpec;
        public AssetRef<BattleProjectileQSpec> BattleProjectileSpec;
        public AssetRef<BattleDiamondQSpec>    BattleDiamondSpec;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleArenaQSpec GetArenaSpec(Frame f)
        {
            return f.FindAsset(f.FindAsset(f.RuntimeConfig.BattleConfig).BattleArenaSpec);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleSoulWallQSpec GetSoulWallSpec(Frame f)
        {
            return f.FindAsset(f.FindAsset(f.RuntimeConfig.BattleConfig).BattleSoulWallSpec);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleProjectileQSpec GetProjectileSpec(Frame f)
        {
            return f.FindAsset(f.FindAsset(f.RuntimeConfig.BattleConfig).BattleProjectileSpec);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleDiamondQSpec GetDiamondSpec(Frame f)
        {
            return f.FindAsset(f.FindAsset(f.RuntimeConfig.BattleConfig).BattleDiamondSpec);
        }
    }

    [Serializable]
    public struct BattleParameters
    {
        public enum PlayerType
        {
            None = 0,
            Player = 1,
            Bot = 2,
        }

        public PlayerType[] PlayerSlotTypes;
        public string[] PlayerSlotUserIDs;

        public int PlayerCount;

        public BattleEmotionState ProjectileInitialEmotion;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PlayerType[] GetPlayerSlotTypes(Frame f) => f.RuntimeConfig.BattleParameters.PlayerSlotTypes;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] GetPlayerSlotUserIDs(Frame f) => f.RuntimeConfig.BattleParameters.PlayerSlotUserIDs;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPlayerCount(Frame f) => f.RuntimeConfig.BattleParameters.PlayerCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleEmotionState GetProjectileInitialEmotion(Frame f) => f.RuntimeConfig.BattleParameters.ProjectileInitialEmotion;
    }
}
