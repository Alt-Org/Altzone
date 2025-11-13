/// @file Utils.cs
/// <summary>
/// Contains @cref{Battle.View,Utils} class which has utility methods for view.
/// </summary>
///
/// This script:<br/>
/// Static utility script.

using Quantum;

namespace Battle.View
{
    /// <summary>
    /// <span class="brief-h">Static utility script.</span><br/>
    /// Has utility methods for view.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Try get latest predicted %Quantum <a href="https://doc.photonengine.com/quantum/current/manual/frames">Frame@u-exlink</a>.
        /// </summary>
        ///
        /// <param name="frame">The latest predicted %Quantum <a href="https://doc.photonengine.com/quantum/current/manual/frames">Frame@u-exlink</a> or null.</param>
        ///
        /// <returns>True if getting the frame was successful and false if not.</returns>
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
