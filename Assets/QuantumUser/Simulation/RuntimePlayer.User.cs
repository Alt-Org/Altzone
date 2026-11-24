namespace Quantum
{
    public partial class RuntimePlayer
    {
        //{ Constants

        // Common
        public const int PlayerSlotCount   = Constants.BATTLE_PLAYER_SLOT_COUNT;
        public const int CharacterMaxCount = Constants.BATTLE_PLAYER_CHARACTER_MAX_COUNT;

        // Normal
        public const int NormalCharacterCount = Constants.BATTLE_NORMAL_PLAYER_CHARACTER_COUNT;

        // Flipper
        public const int FlipperCharacterCount = Constants.BATTLE_FLIPPER_PLAYER_CHARACTER_COUNT;

        //} Constants

        public static readonly BattlePlayerSlot[] PlayerSlots = { BattlePlayerSlot.Slot1, BattlePlayerSlot.Slot2, BattlePlayerSlot.Slot3, BattlePlayerSlot.Slot4 };

        public string UserID;
        public BattlePlayerSlot PlayerSlot;
        public BattleCharacterBase[] Characters = new BattleCharacterBase[CharacterMaxCount];
    }
}
