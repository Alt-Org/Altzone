using Battle.QSimulation.Game;
using Battle.QSimulation.SoulWall;
using Battle.QSimulation.Projectile;

namespace Quantum
{
    public partial class RuntimeConfig
    {
        // Battle Spec
        public AssetRef<BattleArenaQSpec>      BattleArenaSpec;
        public AssetRef<BattleSoulWallQSpec>   BattleSoulWallSpec;
        public AssetRef<BattleProjectileQSpec> BattleProjectileSpec;

        // Battle Parameters
        public BattleEmotionState BattleInitialProjectileEmotion;
    }
}
