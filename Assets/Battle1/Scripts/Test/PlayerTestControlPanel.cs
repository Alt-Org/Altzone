using UnityEngine;

namespace Battle1.Scripts.Test
{
    public class PlayerTestControlPanel : MonoBehaviour
    {
        [SerializeField] private bool _moveTo;
        [SerializeField] private bool _startRotate;
        [SerializeField] private float _rotate;
        [SerializeField] private Vector2 _targetPosition;
        //private IPlayerInputTarget _IPlayerInputTarget;
        //private IPlayerDriver _IPlayerDriver;


        void Update()
        {
            // This is utterly rubbish
            /*if (GameObject.Find("PlayerDriverPhoton(Clone)") != null && _moveTo)
            {
                _moveTo = false;
                _IPlayerInputTarget = GameObject.Find("PlayerDriverPhoton(Clone)").GetComponent<PlayerDriverPhoton>();
                _IPlayerInputTarget.MoveTo(_targetPosition);
            }
            //PlayerDriverState._autoRotate has to be false
            if (GameObject.Find("PlayerDriverPhoton(Clone)") != null && _startRotate)
            {
                _startRotate = false;
                _IPlayerDriver = GameObject.Find("PlayerDriverPhoton(Clone)").GetComponent<PlayerDriverPhoton>();
                _IPlayerDriver.Rotate(_rotate);
            }*/
            
        }
    }
}