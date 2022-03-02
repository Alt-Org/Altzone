using System;
using UnityEngine;

namespace Battle.Scripts.Battle.Players2
{
    /// <summary>
    /// Calculates distance between two players.
    /// </summary>
    internal interface IPlayerDistanceMeter
    {
        void CheckShieldVisibility(Action onShieldVisible, Action onShieldHidden);

        float SqrDistance { get; }
    }

    internal class PlayerDistanceMeter : IPlayerDistanceMeter
    {
        private readonly Transform _transform1;
        private readonly Transform _transform2;
        private readonly float _sqrShieldDistance;

        private float _sqrDistance;
        private bool _currentVisibility;

        public PlayerDistanceMeter(Transform transform1, Transform transform2, float distanceToHide)
        {
            _transform1 = transform1;
            _transform2 = transform2;
            _sqrShieldDistance = distanceToHide * distanceToHide;
            // Calculate current distance and reverse visibility to force first callback notification
            _sqrDistance = Mathf.Abs((_transform1.position - _transform2.position).sqrMagnitude);
            var isVisible = _sqrDistance < _sqrShieldDistance;
            _currentVisibility = !isVisible;
        }

        float IPlayerDistanceMeter.SqrDistance => _sqrDistance;

        void IPlayerDistanceMeter.CheckShieldVisibility(Action onShieldVisible, Action onShieldHidden)
        {
            _sqrDistance = Mathf.Abs((_transform1.position - _transform2.position).sqrMagnitude);
            var isVisible = _sqrDistance < _sqrShieldDistance;
            if (isVisible == _currentVisibility)
            {
                return;
            }
            if (isVisible)
            {
                onShieldVisible();
            }
            else
            {
                onShieldHidden();
            }
            _currentVisibility = isVisible;
        }
    }

    /// <summary>
    /// Fake implementation for single player testing.
    /// </summary>
    internal class PlayerDistanceMeterFixed : IPlayerDistanceMeter
    {
        private readonly bool _isVisible;

        private bool _currentVisibility;

        public PlayerDistanceMeterFixed(bool isVisible)
        {
            _isVisible = isVisible;
            _currentVisibility = !isVisible;
        }

        float IPlayerDistanceMeter.SqrDistance => _isVisible ? 0 : 1;

        void IPlayerDistanceMeter.CheckShieldVisibility(Action onShieldVisible, Action onShieldHidden)
        {
            if (_isVisible == _currentVisibility)
            {
                return;
            }
            if (_isVisible)
            {
                onShieldVisible();
            }
            else
            {
                onShieldHidden();
            }
            _currentVisibility = _isVisible;
        }
    }
}