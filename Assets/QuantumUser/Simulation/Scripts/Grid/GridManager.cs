using System.Runtime.CompilerServices;

using Photon.Deterministic;

namespace Quantum
{
    public static class GridManager
    {
        public static FP GridScaleFactor { get; private set; }

        public static void Init(BattleArenaSpec battleArenaSpec)
        {
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

        private static FPVector2 s_gridOffset;
    }
}
