using System.Collections.Generic;

namespace Quantum
{
    public partial class RuntimePlayer
    {
        public const int BattleCharacterArraySize = 3;
        public BattleCharacterBase[] _characters = new BattleCharacterBase[BattleCharacterArraySize];
        public int playerPos;
    }
}
