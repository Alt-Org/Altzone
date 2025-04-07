using Battle.QSimulation.Game;
using Battle.QSimulation.SoulWall;
using Battle.QSimulation.Projectile;

namespace Quantum
{
    public partial class RuntimeConfig : AssetObject
    {
        // Battle Spec
        public AssetRef<BattleArenaQSpec>      BattleArenaSpec;
        public AssetRef<BattleSoulWallQSpec>   BattleSoulWallSpec;
        public AssetRef<BattleProjectileQSpec> BattleProjectileSpec;

        // Battle Parameters
        public BattleEmotionState BattleInitialProjectileEmotion;

        public void BattleSetParameters(BattleEmotionState InitialProjectileEmotion)
        {
            BattleInitialProjectileEmotion = InitialProjectileEmotion;
        }
    }
}
