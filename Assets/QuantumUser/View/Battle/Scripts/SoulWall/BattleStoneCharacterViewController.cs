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
    }
}
