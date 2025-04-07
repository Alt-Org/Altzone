using Quantum;

namespace Battle.View
{
    public static class Utils
    {
        public static bool TryGetQuantumFrame(out Frame frame)
        {
            frame = null;

            if (QuantumRunner.Default == null) return false;
            if (QuantumRunner.Default.Game == null) return false;

            frame = QuantumRunner.Default.Game.Frames.Predicted;

            if (frame == default) return false;

            return true;
        }
    }
}
