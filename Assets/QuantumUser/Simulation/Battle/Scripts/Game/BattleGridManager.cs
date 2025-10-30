// System usings
using System.Runtime.CompilerServices;

// Quantum usings
using Quantum;
using Photon.Deterministic;

namespace Battle.QSimulation.Game
{
    public static class BattleGridManager
    {
        // fields are meant to be constant and should not be changed after init

        public static int Rows { get; private set; }
        public static int Columns { get; private set; }

        public static int TeamAlphaFieldStart { get; private set; }
        public static int TeamAlphaFieldEnd { get; private set; }
        public static int TeamBetaFieldStart { get; private set; }
        public static int TeamBetaFieldEnd { get; private set; }

        public static FP GridScaleFactor { get; private set; }

        public static void Init(BattleArenaQSpec battleArenaSpec)
        {
            Rows = battleArenaSpec.GridHeight;
            Columns = battleArenaSpec.GridWidth;

            TeamAlphaFieldStart = battleArenaSpec.SoulWallHeight;
            TeamAlphaFieldEnd = (battleArenaSpec.GridHeight - battleArenaSpec.MiddleAreaHeight) / 2 - 1;
            TeamBetaFieldStart = TeamAlphaFieldEnd + battleArenaSpec.MiddleAreaHeight + 1;
            TeamBetaFieldEnd = battleArenaSpec.GridHeight - 1 - battleArenaSpec.SoulWallHeight;

            GridScaleFactor = battleArenaSpec.WorldHeight / battleArenaSpec.GridHeight;

            s_gridOffset = new FPVector2(
                battleArenaSpec.GridWidth * GridScaleFactor * FP._0_50,
                battleArenaSpec.WorldHeight * FP._0_50
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int WorldXPositionToGridCol(FP x) { return (int)((x + s_gridOffset.X) / GridScaleFactor); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int WorldYPositionToGridRow(FP y) { return (int)((y + s_gridOffset.Y) / GridScaleFactor); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FP GridRowToWorldYPosition(int row) { return (((FP)row) + FP._0_50) * GridScaleFactor - s_gridOffset.Y; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FP GridColToWorldXPosition(int col) { return (((FP)col) + FP._0_50) * GridScaleFactor - s_gridOffset.X; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleGridPosition WorldPositionToGridPosition(FPVector2 wordPosition)
        {
            return new BattleGridPosition()
            {
                Row = WorldYPositionToGridRow(wordPosition.Y),
                Col = WorldXPositionToGridCol(wordPosition.X)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FPVector2 GridPositionToWorldPosition(BattleGridPosition gridPosition)
        {
            return new FPVector2(
                GridColToWorldXPosition(gridPosition.Col),
                GridRowToWorldYPosition(gridPosition.Row)
            );
        }

        private static FPVector2 s_gridOffset;
    }
}
