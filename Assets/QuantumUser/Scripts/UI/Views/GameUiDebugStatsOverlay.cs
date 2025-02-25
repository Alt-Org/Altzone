using Quantum;
using TMPro;
using UnityEngine;

namespace QuantumUser.Scripts.UI.Views
{
    public class GameUiDebugStatsOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject _view;
        [SerializeField] private TMP_Text _impactForceValueText;

        public void SetShow(bool show)
        {
            _view.SetActive(show);
        }

        public void SetStats(BattleCharacterBase character)
        {
            _impactForceValueText.text = character.Attack.ToString();
        }
    }
}

