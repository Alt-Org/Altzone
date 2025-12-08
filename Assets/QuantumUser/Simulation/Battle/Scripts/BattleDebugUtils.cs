using Quantum;

using Battle.QSimulation.Game;


namespace Battle.QSimulation
{
    public static unsafe class BattleInputDebugUtils
    {
        public struct InputDebugInfo
        {
            public bool NotEmpty;
            public string Summary;
            public string Struct;

            public InputDebugInfo(bool notEmpty, string summary, string @struct)
            {
                NotEmpty = notEmpty;
                Summary  = summary;
                Struct   = @struct;
            }
        }

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
