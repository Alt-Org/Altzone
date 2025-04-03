using Altzone.Scripts.BattleUi;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumUser.Scripts.UI.Views
{
    /// <summary>
    /// Handles Battle Ui player info functionality.
    /// </summary>
    public class GameUiPlayerInfoHandler : MonoBehaviour
    {
        [Header("Character slot 1")]
        [SerializeField] private Button _characterSelectButton1;
        [SerializeField] private Image _damageFill1;
        [SerializeField] private Image _shieldFill1;

        [Header("Character slot 2")]
        [SerializeField] private Button _characterSelectButton2;
        [SerializeField] private Image _damageFill2;
        [SerializeField] private Image _shieldFill2;

        [Header("Character slot 3")]
        [SerializeField] private Button _characterSelectButton3;
        [SerializeField] private Image _damageFill3;
        [SerializeField] private Image _shieldFill3;

        [Header("Movable UI")]
        public BattleUiElement MovableUiElement;

    }
}

