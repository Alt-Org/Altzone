using System.Runtime.CompilerServices;

using Photon.Deterministic;

namespace Quantum
{
    public static class GridManager
    {
        public static FP GridScaleFactor { get; private set; }

        public static void Init(BattleArenaSpec battleArenaSpec)
        {
            s_battleArenaSpec = battleArenaSpec;
            GridScaleFactor = s_battleArenaSpec.WorldHeight / s_battleArenaSpec.GridHeight;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int WorldXPositionToGridCol(FP x) { return (int)((x + s_battleArenaSpec.WorldWidth  * FP._0_50) / GridScaleFactor); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int WorldYPositionToGridRow(FP y) { return (int)((y + s_battleArenaSpec.WorldHeight * FP._0_50) / GridScaleFactor); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FP GridRowToWorldYPosition(int row) { return (((FP)row) + FP._0_50) * GridScaleFactor - s_battleArenaSpec.WorldHeight * FP._0_50; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FP GridColToWorldXPosition(int col) { return (((FP)col) + FP._0_50) * GridScaleFactor - s_battleArenaSpec.WorldWidth  * FP._0_50; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GridPosition WorldPositionToGridPosition(FPVector2 wordPosition)
        {
            return new GridPosition()
            {
                Row = WorldYPositionToGridRow(wordPosition.Y),
                Col = WorldXPositionToGridCol(wordPosition.X)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FPVector2 GridPositionToWorldPosition(GridPosition gridPosition)
        {
            return new FPVector2(
                GridColToWorldXPosition(gridPosition.Col),
                GridRowToWorldYPosition(gridPosition.Row)
            );
        }

        private static BattleArenaSpec s_battleArenaSpec;
    }
}
