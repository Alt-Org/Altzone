namespace Quantum
{
    public partial class RuntimePlayer
    {
        public const int PlayerSlotCount = Constants.BATTLE_PLAYER_SLOT_COUNT;
        public const int CharacterCount = Constants.BATTLE_PLAYER_CHARACTER_COUNT;

        public static readonly BattlePlayerSlot[] PlayerSlots = { BattlePlayerSlot.Slot1, BattlePlayerSlot.Slot2, BattlePlayerSlot.Slot3, BattlePlayerSlot.Slot4 };

        public string UserID;
        public BattlePlayerSlot PlayerSlot;
        public BattleCharacterBase[] Characters = new BattleCharacterBase[CharacterCount];
    }
}
