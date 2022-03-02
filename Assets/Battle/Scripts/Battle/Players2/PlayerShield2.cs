using Battle.Scripts.Battle.interfaces;
using UnityEngine;

namespace Battle.Scripts.Battle.Players2
{
    internal class PlayerShield2 : IPlayerShield2
    {
        private static readonly string[] StateNames = { "Norm", "Frozen", "Ghost" };

        private readonly ShieldConfig _config;
        private readonly ParticleSystem _shieldHitEffect;

        private string _shieldName;
        private bool _isVisible;
        private int _playMode;
        private int _rotationIndex;

        private GameObject _shield;
        private Collider2D _collider;

        public bool IsVisible => _isVisible;
        public int RotationIndex => _rotationIndex;

        public string StateString => $"{(_isVisible ? "V" : "H")} R{_rotationIndex} {(_collider.enabled ? "col" : "~~~")}";

        public PlayerShield2(ShieldConfig config)
        {
            _config = config;
            _shieldHitEffect = _config._shieldHitEffect;
        }

        private void SetupShield(bool isShieldRotated)
        {
            Debug.Log($"SetupShield {_shieldName} isShieldRotated {isShieldRotated}");
            var shields = _config.Shields;
            var pivot = _config._particlePivot;
            for (var i = 0; i < shields.Length; ++i)
            {
                var shield = shields[i];
                shield.Rotate(isShieldRotated);
                pivot.Rotate(isShieldRotated);
                if (i == _rotationIndex)
                {
                    _shield = shield.gameObject;
                    _shield.SetActive(true);
                    _collider = shield.GetComponent<Collider2D>();
                }
                else
                {
                    shield.gameObject.SetActive(false);
                }
            }
        }

        void IPlayerShield2.Setup(string shieldName, bool isShieldRotated, bool isVisible, int playMode, int rotationIndex)
        {
            _shieldName = shieldName;
            _isVisible = isVisible;
            _rotationIndex = rotationIndex;
            SetupShield(isShieldRotated);
            ((IPlayerShield2)this).SetVisibility(isVisible);
            ((IPlayerShield2)this).SetPlayMode(playMode);
            ((IPlayerShield2)this).SetRotation(rotationIndex);
        }

        void IPlayerShield2.SetVisibility(bool isVisible)
        {
            Debug.Log($"SetVisibility {_shieldName} mode {StateNames[_playMode]} isVisible {_isVisible} <- {isVisible}");
            _isVisible = isVisible;
            _shield.SetActive(_isVisible);
        }

        void IPlayerShield2.SetPlayMode(int playMode)
        {
            _playMode = playMode;
            Debug.Log(
                $"SetShieldState {_shieldName} mode {StateNames[_playMode]} <- {StateNames[playMode]} rotation {_rotationIndex} collider {_collider.enabled}");
            _playMode = playMode;
            switch (_playMode)
            {
                case PlayerActor.PlayModeNormal:
                case PlayerActor.PlayModeFrozen:
                    _collider.enabled = true;
                    break;
                case PlayerActor.PlayModeGhosted:
                    _collider.enabled = false;
                    break;
                default:
                    throw new UnityException($"invalid playmode {_playMode}");
            }
            _shield.SetActive(_isVisible);
        }

        void IPlayerShield2.SetRotation(int rotationIndex)
        {
            if (rotationIndex >= _config.Shields.Length)
            {
                rotationIndex %= _config.Shields.Length;
            }
            Debug.Log($"SetRotation {_shieldName} mode {StateNames[_playMode]} rotation {_rotationIndex} <- {rotationIndex}");
            _rotationIndex = rotationIndex;
            _shield.SetActive(false);
            _shield = _config.Shields[_rotationIndex].gameObject;
            _collider = _shield.GetComponent<Collider2D>();
            _shield.SetActive(_isVisible);
        }

        void IPlayerShield2.PlayHitEffects()
        {
            _shieldHitEffect.Play();
        }
    }
}
