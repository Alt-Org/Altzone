using UnityEngine;

namespace Altzone.Scripts.BattleUiShared
{
    [CreateAssetMenu(fileName = "BattleUiPrefabs", menuName = "ALT-Zone/BattleUiPrefabs")]
    public class BattleUiPrefabs : ScriptableObject
    {
        public GameObject timer;
        public GameObject playerInfo;
        public GameObject diamonds;
        public GameObject giveUpButton;
        public GameObject joystick;
    }
}
