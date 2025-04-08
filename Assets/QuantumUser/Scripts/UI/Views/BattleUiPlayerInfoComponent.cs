using UnityEngine;

using TMPro;

namespace QuantumUser.Scripts.UI.Views
{
    /// <summary>
    /// Holds references to Battle Ui player info gameobjects.
    /// </summary>
    public class BattleUiPlayerInfoComponent : MonoBehaviour
    {
        public TMP_Text PlayerName;
        public BattleUiCharacterButtonComponent[] CharacterButtons;
    }
}

