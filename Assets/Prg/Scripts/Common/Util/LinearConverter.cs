using System;

namespace Prg.Scripts.Common.Util
{
    public interface ILinearConverter<T>
    {
        /// <summary>
        /// Converts and clamps input value using liner conversion formula.
        /// </summary>
        /// <param name="rawInput">unconstrained input value</param>
        /// <returns>clamped output value</returns>
        T mapValue(T rawInput);
    }

    public static class LinearConverter
    {
        public static ILinearConverter<float> Get(float minInput, float maxInput, float minOutput, float maxOutput)
        {
            if (!(maxInput > minInput))
            {
                throw new ArgumentException($"input values: min {minInput} >= max {maxInput}");
            }
            if (!(maxOutput > minOutput))
            {
                throw new ArgumentException($"output values: min {minOutput} >= max {maxOutput}");
            }
            return new FloatLinearConverter(minInput, maxInput, minOutput, maxOutput);
        }

        private class FloatLinearConverter : ILinearConverter<float>
        {
            private readonly float minInput;
            private readonly float maxInput;
            private readonly float minOutput;
            private readonly float maxOutput;

            public FloatLinearConverter(float minInput, float maxInput, float minOutput, float maxOutput)
            {
                this.minInput = minInput;
                this.maxInput = maxInput;
                this.minOutput = minOutput;
                this.maxOutput = maxOutput;
            }

            public float mapValue(float rawInput)
            {
                // formula to linearly rescale data values having observed min and max (input) into a new arbitrary range min' to max' (output):
                // newValue = (value-min) * (max'-min') / (max-min) + min'

                // See also: https://support.intuiface.com/hc/en-us/articles/360007179612-Linear-Converter

                // We clamp input values so that caller does not have to clamp output values!
                if (rawInput < minInput)
                {
                    return minOutput;
                }
                if (rawInput > maxInput)
                {
                    return maxOutput;
                }
                // TODO: this formula is generic that covers all cases, it could be optimized more...!
                var mappedOutput = (rawInput - minInput) * (maxOutput - minOutput) / (maxInput - minInput) + minOutput;
                return mappedOutput;
            }
        }
    }
}