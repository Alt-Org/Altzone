using Math = System.Math;
using UnityEngine;

namespace Prg.Scripts.Common.MathPlus
{
    public static class MathPlus
    {
        /// <summary>
        /// <para>Remaps <c>value</c> linearly from source range (<c>min</c>, <c>max</c>) to destination range (<c>newMin</c>, <c>newMax</c>).</para>
        /// <para>Note that this method does not clamp the value. (if you want to clamp the value you can use the <c>RemapAndClamp</c> method)</para>
        /// </summary>
        /// <param name="value">The value that you want to remap.</param>
        /// <param name="min">The min value of the source range.</param>
        /// <param name="max">The max value of the source range.</param>
        /// <param name="newMin">The min value of the destination range.</param>
        /// <param name="newMax">The max value of the destination range.</param>
        /// <returns>The value remaped to the destination range.</returns>
        public static float Remap(float value, float min, float max, float newMin, float newMax)
        {
            return (value - min) * ((max - min) / (newMax - newMin)) + newMin;
        }

        /// <summary>
        /// <para>Remaps <c>value</c> linearly from source range (<c>min</c>, <c>max</c>) to destination range (<c>newMin</c>, <c>newMax</c>).</para>
        /// <para>Note that this method does not clamp the value. (if you want to clamp the value you can use the <c>RemapAndClamp</c> method)</para>
        /// </summary>
        /// <param name="value">The value that you want to remap.</param>
        /// <param name="min">The min value of the source range.</param>
        /// <param name="max">The max value of the source range.</param>
        /// <param name="newMin">The min value of the destination range.</param>
        /// <param name="newMax">The max value of the destination range.</param>
        /// <returns>The value remaped to the destination range.</returns>
        public static double Remap(double value, double min, double max, double newMin, double newMax)
        {
            return (value - min) * ((max - min) / (newMax - newMin)) + newMin;
        }

        /// <summary>
        /// <para>Remaps <c>value</c> linearly from source range (<c>min</c>, <c>max</c>) to destination range (<c>newMin</c>, <c>newMax</c>) and clamps the remaped value to the destination.</para>
        /// <para>(if you don't want to clamp the value you can use the <c>Remap</c> method)</para>
        /// </summary>
        /// <param name="value">The value that you want to remap.</param>
        /// <param name="min">The min value of the source range.</param>
        /// <param name="max">The max value of the source range.</param>
        /// <param name="newMin">The min value of the destination range.</param>
        /// <param name="newMax">The max value of the destination range.</param>
        /// <returns>The value remaped and clamped to the destination range.</returns>
        public static float RemapAndClamp(float value, float min, float max, float newMin, float newMax)
        {
            return Mathf.Clamp(Remap(value, min, max, newMin, newMax), newMin, newMax);
        }

        /// <summary>
        /// <para>Remaps <c>value</c> linearly from source range (<c>min</c>, <c>max</c>) to destination range (<c>newMin</c>, <c>newMax</c>) and clamps the remaped value to the destination.</para>
        /// <para>(if you don't want to clamp the value you can use the <c>Remap</c> method)</para>
        /// </summary>
        /// <param name="value">The value that you want to remap.</param>
        /// <param name="min">The min value of the source range.</param>
        /// <param name="max">The max value of the source range.</param>
        /// <param name="newMin">The min value of the destination range.</param>
        /// <param name="newMax">The max value of the destination range.</param>
        /// <returns>The value remaped and clamped to the destination range.</returns>
        public static double RemapAndClamp(double value, double min, double max, double newMin, double newMax)
        {
            return Math.Clamp(Remap(value, min, max, newMin, newMax), newMin, newMax);
        }
    }
}
