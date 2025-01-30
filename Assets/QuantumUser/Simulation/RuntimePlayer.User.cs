using System.Collections.Generic;

namespace Quantum
{
    public partial class RuntimePlayer
    {
        public const int CharacterCount = 3;

        public int PlayerPosition;
        public BattleCharacterBase[] Characters = new BattleCharacterBase[CharacterCount];
    }
}
