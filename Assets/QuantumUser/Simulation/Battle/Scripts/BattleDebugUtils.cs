/// @file BattleDebugUtils.cs
/// <summary>
/// Contains @cref{Battle.QSimulation,BattleInputDebugUtils} class which handles input debug info.
/// </summary>

// Quantum usings
using Quantum;

// Battle QSimulation usings
using Battle.QSimulation.Game;

namespace Battle.QSimulation
{
    /// <summary>
    /// Handles input debug info.
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
            if (input->AbilityActivate != false)
            {
                if (inputDebugSummary != "") inputDebugSummary += ", ";
                inputDebugSummary += "Ability Activate";
                inputNotEmpty = true;
            }
            if (input->PlayerCharacterNumber > -1)
            {
                if (inputDebugSummary != "") inputDebugSummary += ", ";
                inputDebugSummary += "Character Swap";
                inputNotEmpty = true;
            }
            if (input->GiveUpInput != false)
            {
                if (inputDebugSummary != "") inputDebugSummary += ", ";
                inputDebugSummary += "Give Up";
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
                                  "    MovementPositionMove:          {3},\n" +
                                  "    MovementDirection:             {4},\n" +
                                  "    RotationInput:                 {5},\n" +
                                  "    RotationValue:                 {6},\n" +
                                  "    AbilityActivate:               {7},\n" +
                                  "    PlayerCharacterNumber:         {8},\n" +
                                  "    GiveUpInput:                   {9}\n" +
                                  "}}",
                                  input->MovementInput,
                                  input->MovementDirectionIsNormalized,
                                  input->MovementPositionTarget.ConvertToString(),
                                  input->MovementPositionMove,
                                  input->MovementDirection,
                                  input->RotationInput,
                                  input->RotationValue,
                                  input->AbilityActivate,
                                  input->PlayerCharacterNumber,
                                  input->GiveUpInput
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
