using Battle.Scripts.Battle.interfaces;
using UnityEngine;

namespace Battle.Scripts.Battle.Players2
{
    /// <summary>
    /// Manager for local shield state.
    /// </summary>
    internal class PlayerShield : IPlayerShield
    {
        private static readonly string[] StateNames = { "Norm", "Frozen", "Ghost" };

        private readonly ShieldConfig _config;
        private readonly ParticleSystem _shieldHitEffect;

        private string _shieldName;
        private int _playMode;

        private GameObject _shield;
        private Collider2D _collider;

        public bool IsVisible { get; private set; }

        public bool CanRotate => RotationIndex < _config.Shields.Length - 1;
        public int RotationIndex { get; private set; }

        public string StateString => $"{(IsVisible ? "V" : "H")} R{RotationIndex} {(_collider.enabled ? "col" : "~~~")}";

        public PlayerShield(ShieldConfig config)
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
                if (i == RotationIndex)
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

        void IPlayerShield.Setup(string shieldName, bool isShieldRotated, bool isVisible, int playMode, int rotationIndex)
        {
            _shieldName = shieldName;
            IsVisible = isVisible;
            RotationIndex = rotationIndex;
            SetupShield(isShieldRotated);
            ((IPlayerShield)this).SetVisibility(isVisible);
            ((IPlayerShield)this).SetPlayMode(playMode);
            ((IPlayerShield)this).SetRotation(rotationIndex);
        }

        void IPlayerShield.SetVisibility(bool isVisible)
        {
            Debug.Log($"SetVisibility {_shieldName} mode {StateNames[_playMode]} isVisible {IsVisible} <- {isVisible}");
            IsVisible = isVisible;
            _shield.SetActive(IsVisible);
        }

        void IPlayerShield.SetPlayMode(int playMode)
        {
            _playMode = playMode;
            Debug.Log(
                $"SetShieldState {_shieldName} mode {StateNames[_playMode]} <- {StateNames[playMode]} rotation {RotationIndex} collider {_collider.enabled}");
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
            _shield.SetActive(IsVisible);
        }

        void IPlayerShield.SetRotation(int rotationIndex)
        {
            if (rotationIndex >= _config.Shields.Length)
            {
                rotationIndex %= _config.Shields.Length;
            }
            Debug.Log($"SetRotation {_shieldName} mode {StateNames[_playMode]} rotation {RotationIndex} <- {rotationIndex}");
            RotationIndex = rotationIndex;
            _shield.SetActive(false);
            _shield = _config.Shields[RotationIndex].gameObject;
            _collider = _shield.GetComponent<Collider2D>();
            _shield.SetActive(IsVisible);
        }

        void IPlayerShield.PlayHitEffects()
        {
            _shieldHitEffect.Play();
        }
    }
}