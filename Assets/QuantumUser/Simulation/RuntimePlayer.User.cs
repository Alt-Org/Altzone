namespace Quantum
{
    public partial class RuntimePlayer
    {
        public const int PlayerSlotCount = Constants.PLAYER_SLOT_COUNT;
        public const int CharacterCount = Constants.PLAYER_CHARACTER_COUNT;

        public int PlayerSlot;
        public BattleCharacterBase[] Characters = new BattleCharacterBase[CharacterCount];
    }
}
