using Battle.QSimulation.Game;

namespace Quantum
{
    public enum AzGameType
    {
        Battle
    }

    public partial class RuntimeConfig
    {
        public AzGameType AzGameType;

        public AssetRef<BattleQConfig> BattleConfig;
        public BattleParameters BattleParameters;
    }
}
