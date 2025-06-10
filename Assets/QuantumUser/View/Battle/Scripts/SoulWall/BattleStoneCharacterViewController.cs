using System.Collections;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace Battle.View
{
    public class BattleStoneCharacterViewController : MonoBehaviour
    {
        [SerializeField] private GameObject[] _topCharacterParts;
        [SerializeField] private GameObject[] _bottomCharacterParts;
        [SerializeField] private SpriteRenderer[] _topCharacterEmotionIndicators;
        [SerializeField] private SpriteRenderer[] _bottomCharacterEmotionIndicators;
        [SerializeField] private Color[] _emotionColors;

        public void DestroyCharacterPart(int wallNumber, BattleTeamNumber team)
        {
            switch (team)
            {
                case BattleTeamNumber.TeamAlpha:
                    _bottomCharacterParts[wallNumber].gameObject.SetActive(false);
                    break;
                case BattleTeamNumber.TeamBeta:
                    _topCharacterParts[wallNumber].gameObject.SetActive(false);
                    break;
            }
        }

        public void SetEmotionIndicator(int wallNumber, BattleTeamNumber team, int emotionIndex)
        {
            switch (team)
            {
                case BattleTeamNumber.TeamAlpha:
                    _bottomCharacterEmotionIndicators[wallNumber].color = _emotionColors[emotionIndex];
                    break;
                case BattleTeamNumber.TeamBeta:
                    _topCharacterEmotionIndicators[wallNumber].color = _emotionColors[emotionIndex];
                    break;
            }
        }
    }
}
