using UnityEngine;
using TMPro;

namespace Battle.View.UI
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
