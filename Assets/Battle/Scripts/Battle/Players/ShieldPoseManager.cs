using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class ShieldPoseManager : MonoBehaviour
    {
        private GameObject[] _shields;
        private GameObject _currentPose;
        private int _maxPoseIndex;

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
            _shields[0].SetActive(true);
        }

        public int MaxPoseIndex => _maxPoseIndex;

        public void SetPose(int poseIndex)
        {
            if (_currentPose != null)
            {
                _currentPose.SetActive(false);
            }            
            _currentPose = _shields[poseIndex];
            _currentPose.transform.localPosition = Vector3.zero;
            _currentPose.SetActive(true);
        }
    }
}
