/// @file BattleDebugUtils.cs
/// <summary>
/// Debug utility code.<br/>
/// Contains @cref{Battle.QSimulation,BattleInputDebugUtils} class which provides input debugging utilities.
/// </summary>

// Quantum usings
using Quantum;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation
{
    /// <summary>
    /// Provides input debugging utilities.
    /// </summary>
    public static unsafe class BattleInputDebugUtils
    {
        /// <summary>
        /// Struct containing input debug info data.<br/>
        /// Used as the return type of <see cref="GenerateDebugInfo"/>.
        /// </summary>
        public struct InputDebugInfo
        {
            /// <summary>
            /// Is the input not empty.<br/>
            /// Input is considered empty when player takes no action.
            /// </summary>
            public bool NotEmpty;

            /// <summary>Summary of the actions that the player wants to take as a string.</summary>
            public string Summary;

            /// <summary>A string representation of the full input struct.</summary>
            public string Struct;

            public InputDebugInfo(bool notEmpty, string summary, string @struct)
            {
                NotEmpty = notEmpty;
                Summary  = summary;
                Struct   = @struct;
            }
        }

        /// <summary>
        /// Generates info from player <paramref name="input"/> that is used for debugging.
        /// </summary>
        ///
        /// <param name="input">Pointer to player input</param>
        ///
        /// <returns><see cref="InputDebugInfo"/> data.</returns>
        public static InputDebugInfo GenerateDebugInfo(Input* input)
        {
            string inputDebugSummary = "";

            bool inputNotEmpty = false;

            if (input->MovementInput != BattleMovementInputType.None)
            {
                inputDebugSummary += "Move";
                inputNotEmpty = true;
            }
            if (input->RotationInput != false)
            {
                if (inputDebugSummary != "") inputDebugSummary += ", ";
                inputDebugSummary += "Rotate";
                inputNotEmpty = true;
            }

            if (inputNotEmpty)
            {
                return new InputDebugInfo(
                    true,
                    inputDebugSummary,
                    string.Format("{{\n" +
                                  "    MovementInput:                 {0},\n" +
                                  "    MovementDirectionIsNormalized: {1},\n" +
                                  "    MovementPositionTarget:        {2},\n" +
                                  "    MovementVector:                {3},\n" +
                                  "    RotationInput:                 {4},\n" +
                                  "    RotationValue:                 {5},\n" +
                                  "}}",
                                  input->MovementInput,
                                  input->MovementDirectionIsNormalized,
                                  input->MovementGridPosition.ConvertToString(),
                                  input->MovementVector,
                                  input->RotationInput,
                                  input->RotationValue
                    )
                );
            }
            else
            {
                return new InputDebugInfo(false, null, null);
            }
        }
    }
}
