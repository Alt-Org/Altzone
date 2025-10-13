using Battle.QSimulation.Game;
using Raid.QSimulation;

namespace Quantum
{
    public enum AzGameType
    {
        Battle,
        Raid
    }

    public partial class RuntimeConfig
    {
        public AzGameType AzGameType;

        public AssetRef<BattleQConfig> BattleConfig;
        public BattleParameters BattleParameters;

        public AssetRef<RaidQConfig> RaidConfig;
        public RaidParameters RaidParameters;
    }
}
