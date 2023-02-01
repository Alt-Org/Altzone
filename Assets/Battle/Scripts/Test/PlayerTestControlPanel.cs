using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Players;
using UnityEngine;
namespace Battle.Scripts.Battle.Test
{
    public class PlayerTestControlPanel : MonoBehaviour
    {
        [SerializeField] private bool _moveTo;
        [SerializeField] private bool _startRotate;
        [SerializeField] private float _rotate;
        [SerializeField] private Vector2 _targetPosition;
        private IPlayerInputTarget _IPlayerInputTarget;
        private IPlayerDriver _IPlayerDriver;


        void Update()
        {
            if (GameObject.Find("PlayerDriverPhoton(Clone)") != null && _moveTo)
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
            }
            
        }
    }
}