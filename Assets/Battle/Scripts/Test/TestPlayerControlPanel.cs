using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Battle.Test
{
    public class TestPlayerControlPanel : MonoBehaviour
    {
        [SerializeField] private Vector2 _targetPosition;

        [Header ("Init")]
        public int _playerPrefabID;
        [Header ("Controll")]
        [SerializeField] private bool _moveTo;
    }
}
