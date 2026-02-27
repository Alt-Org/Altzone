using UnityEngine;

namespace Altzone.Scripts.BattleUiShared
{
    [CreateAssetMenu(fileName = "BattleUiPrefabs", menuName = "ALT-Zone/BattleUiPrefabs")]
    public class BattleUiPrefabs : ScriptableObject
    {
        [SerializeField] private GameObject timer;
        [SerializeField] private GameObject playerInfo;
        [SerializeField] private GameObject diamonds;
        [SerializeField] private GameObject giveUpButton;
        [SerializeField] private GameObject joystick;

        public GameObject Timer => timer;
        public GameObject PlayerInfo => playerInfo;
        public GameObject Diamonds => diamonds;
        public GameObject GiveUpButton => giveUpButton;
        public GameObject Joystick => joystick;
    }
}
