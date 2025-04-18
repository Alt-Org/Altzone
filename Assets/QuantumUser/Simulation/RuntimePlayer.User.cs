namespace Quantum
{
    public partial class RuntimePlayer
    {
        public const int PlayerSlotCount = Constants.BATTLE_PLAYER_SLOT_COUNT;
        public const int CharacterCount = Constants.BATTLE_PLAYER_CHARACTER_COUNT;

        public int PlayerSlot;
        public BattleCharacterBase[] Characters = new BattleCharacterBase[CharacterCount];
    }
}
