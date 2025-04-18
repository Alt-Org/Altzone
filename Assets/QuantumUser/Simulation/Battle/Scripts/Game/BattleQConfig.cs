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
        public BattleEmotionState ProjectileInitialEmotion;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleEmotionState GetProjectileInitialEmotion(Frame f) => f.RuntimeConfig.BattleParameters.ProjectileInitialEmotion;
    }
}
