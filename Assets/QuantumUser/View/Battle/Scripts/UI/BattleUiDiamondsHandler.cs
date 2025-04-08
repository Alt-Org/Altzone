using UnityEngine;
using TMPro;

using Altzone.Scripts.BattleUiShared;

namespace Battle.View.UI
{
    /// <summary>
    /// Handles setting collected diamond amount text.
    /// </summary>
    public class BattleUiDiamondsHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text _diamondText;

        public BattleUiMovableElement MovableUiElement;

        public void SetDiamondsText(int diamondAmount)
        {
            if (!MovableUiElement.gameObject.activeSelf) MovableUiElement.gameObject.SetActive(true);
            _diamondText.text = diamondAmount.ToString();
        }
    }
}

