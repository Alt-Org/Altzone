using Math = System.Math;
using Mathf = UnityEngine.Mathf;

namespace Prg.Scripts.Common.MathPlus
{
    /// <summary>
    /// A collection of extra Math operation.
    /// <para>This is meant to be used in addition to <c><see cref="System.Math"/></c> <a href="https://learn.microsoft.com/en-us/dotnet/api/system.math?view=net-8.0">(doc)</a> and <c><see cref="UnityEngine.Mathf"/></c> <a href="https://docs.unity3d.com/ScriptReference/Mathf.html">(doc)</a> not on its own.</para>
    /// </summary>
    public static class MathPlus
    {
        /// <summary>
        /// Remaps <paramref name="value"/> linearly from source range (<paramref name="min"/>, <paramref name="max"/>) to destination range (<paramref name="newMin"/>, <paramref name="newMax"/>).
        /// <para>@note This method does not clamp the value. (if you want to clamp the value you can use the <see cref="Prg.Scripts.Common.MathPlus.MathPlus.RemapAndClamp(float, float, float, float, float)">RemapAndClamp</see> method)</para>
        /// </summary>
        /// <param name="value">The value that you want to remap.</param>
        /// <param name="min">The min value of the source range.</param>
        /// <param name="max">The max value of the source range.</param>
        /// <param name="newMin">The min value of the destination range.</param>
        /// <param name="newMax">The max value of the destination range.</param>
        /// <returns>The value remapped to the destination range.</returns>
        public static float Remap(float value, float min, float max, float newMin, float newMax)
        {
            return (value - min) * ((max - min) / (newMax - newMin)) + newMin;
        }

        /// <summary>
        /// Remaps <paramref name="value"/> linearly from source range (<paramref name="min"/>, <paramref name="max"/>) to destination range (<paramref name="newMin"/>, <paramref name="newMax"/>).
        /// <para>@note This method does not clamp the value. (if you want to clamp the value you can use the <see cref="Prg.Scripts.Common.MathPlus.MathPlus.RemapAndClamp(double, double, double, double, double)">RemapAndClamp</see> method)</para>
        /// </summary>
        /// <param name="value">The value that you want to remap.</param>
        /// <param name="min">The min value of the source range.</param>
        /// <param name="max">The max value of the source range.</param>
        /// <param name="newMin">The min value of the destination range.</param>
        /// <param name="newMax">The max value of the destination range.</param>
        /// <returns>The value remapped to the destination range.</returns>
        public static double Remap(double value, double min, double max, double newMin, double newMax)
        {
            return (value - min) * ((max - min) / (newMax - newMin)) + newMin;
        }

        /// <summary>
        /// Remaps <paramref name="value"/> linearly from source range (<paramref name="min"/>, <paramref name="max"/>) to destination range (<paramref name="newMin"/>, <paramref name="newMax"/>) and clamps the remaped value to the destination range.
        /// <para>(if you don't want to clamp the value you can use the <see cref="Prg.Scripts.Common.MathPlus.MathPlus.Remap(float, float, float, float, float)">Remap</see> method)</para>
        /// </summary>
        /// <param name="value">The value that you want to remap.</param>
        /// <param name="min">The min value of the source range.</param>
        /// <param name="max">The max value of the source range.</param>
        /// <param name="newMin">The min value of the destination range.</param>
        /// <param name="newMax">The max value of the destination range.</param>
        /// <returns>The value remapped and clamped to the destination range.</returns>
        public static float RemapAndClamp(float value, float min, float max, float newMin, float newMax)
        {
            return Mathf.Clamp(Remap(value, min, max, newMin, newMax), newMin, newMax);
        }

        /// <summary>
        /// Remaps <paramref name="value"/> linearly from source range (<paramref name="min"/>, <paramref name="max"/>) to destination range (<paramref name="newMin"/>, <paramref name="newMax"/>) and clamps the remaped value to the destination range.
        /// <para>(if you don't want to clamp the value you can use the <see cref="Prg.Scripts.Common.MathPlus.MathPlus.Remap(double, double, double, double, double)">Remap</see> method)</para>
        /// </summary>
        /// <param name="value">The value that you want to remap.</param>
        /// <param name="min">The min value of the source range.</param>
        /// <param name="max">The max value of the source range.</param>
        /// <param name="newMin">The min value of the destination range.</param>
        /// <param name="newMax">The max value of the destination range.</param>
        /// <returns>The value remapped and clamped to the destination range.</returns>
        public static double RemapAndClamp(double value, double min, double max, double newMin, double newMax)
        {
            return Math.Clamp(Remap(value, min, max, newMin, newMax), newMin, newMax);
        }
    }
}
