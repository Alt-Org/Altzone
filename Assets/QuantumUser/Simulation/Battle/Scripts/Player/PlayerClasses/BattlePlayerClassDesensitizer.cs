// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

namespace Battle.QSimulation.Player
{
    public class BattlePlayerClassDesensitizer : BattlePlayerClassBase<BattlePlayerClassDesensitizerDataQComponent>
    {
        public override BattlePlayerCharacterClass Class { get; } = BattlePlayerCharacterClass.Desensitizer;

        public override unsafe void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            Debug.Log("Desensitizer spawned");

            //BattlePlayerClassDesensitizerDataQComponent* classData = GetClassData(f, playerEntity);
        }
    }
}
