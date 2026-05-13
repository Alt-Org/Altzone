// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

namespace Battle.QSimulation.Player
{
    public class BattlePlayerClass100 : BattlePlayerClassBase<BattlePlayerClass100DataQComponent>
    {
        public override BattlePlayerCharacterClass Class { get; } = BattlePlayerCharacterClass.Class100;

        public override unsafe void OnSpawn(Frame f, BattlePlayerManager.PlayerHandle playerHandle, BattlePlayerDataQComponent* playerData, EntityRef playerEntity)
        {
            Debug.Log("Desensitizer spawned");

            //BattlePlayerClass100DataQComponent* classData = GetClassData(f, playerEntity);
        }
    }
}
