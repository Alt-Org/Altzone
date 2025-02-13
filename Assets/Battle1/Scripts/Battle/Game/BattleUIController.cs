using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;

namespace Battle1.Scripts.Battle.Game
{
    internal class BattleUIController : MonoBehaviour
    {

        [SerializeField, Tooltip("0: Speed\n1: Resistance\n2: Attack\n3: HP")] TMP_Text[] _diamondsAlpha;
        [SerializeField, Tooltip("0: Speed\n1: Resistance\n2: Attack\n3: HP")] TMP_Text[] _diamondsBeta;

        [SerializeField] GameObject _winGraphics;
        [SerializeField] GameObject _loseGraphics;

        public void UpdateDiamondCountText(BattleTeamNumber battleTeamNumber, DiamondType diamondType, int count)
        {
            GetDiamondText(battleTeamNumber, diamondType).text = count.ToString();
        }

        public void ShowWinGraphics(BattleTeamNumber goalSide, BattleTeamNumber teamNumber)
        {
            if (goalSide != teamNumber)
            {
                _winGraphics.SetActive(true);
                _loseGraphics.SetActive(false);
            }
            else
            {
                _winGraphics.SetActive(false);
                _loseGraphics.SetActive(true);
            }
        }

        private TMP_Text GetDiamondText(BattleTeamNumber teamNumber, DiamondType diamondType)
        {
            TMP_Text[] teamDiamonds = teamNumber switch
            {
                BattleTeamNumber.TeamAlpha => _diamondsAlpha,
                BattleTeamNumber.TeamBeta  => _diamondsBeta,
                _ => null
            };

            return diamondType switch
            {
                DiamondType.DiamondSpeed      => teamDiamonds[0],
                DiamondType.DiamondCharacterSize => teamDiamonds[1],
                DiamondType.DiamondAttack     => teamDiamonds[2],
                DiamondType.DiamondHP         => teamDiamonds[3],
                _ => null,
            };
        }
    }
}
