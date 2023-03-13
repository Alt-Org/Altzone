using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal interface IShieldPoseManager
    {
        void SetNextPose();
    }
    public class ShieldPoseManager : MonoBehaviour, IShieldPoseManager
    {
        private GameObject[] _shields;
        private GameObject _currentPose;
        private int _maxPoseIndex;
        private int _currentPoseIndex;

        private void Awake()
        {
            var childCount = transform.childCount;
            _maxPoseIndex = childCount - 1;
            _shields = new GameObject[childCount];
            for (int i = 0; i <= _maxPoseIndex; i++)
            {
                _shields[i] = transform.GetChild(i).gameObject;
                _shields[i].SetActive(false);
            }
            _currentPoseIndex = 0;
            SetPose(_currentPoseIndex);
        }

        private void SetPose(int poseIndex)
        {
            if (_currentPose != null)
            {
                _currentPose.SetActive(false);
            }            
            _currentPose = _shields[poseIndex];
            _shields[poseIndex].transform.localPosition = Vector3.zero;
            _shields[poseIndex].SetActive(true);
        }

        void IShieldPoseManager.SetNextPose()
        {
            if (_currentPoseIndex < _maxPoseIndex)
            {
                _currentPoseIndex++;
                SetPose(_currentPoseIndex);
            }
        }
    }
}
