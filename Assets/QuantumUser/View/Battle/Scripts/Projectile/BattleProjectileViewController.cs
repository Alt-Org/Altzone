/// @file BattleProjectileViewController.cs
/// <summary>
/// Handles projectile's sprite changes and its trail.
/// </summary>

using System.Collections.Generic;
using UnityEngine;
using Quantum;

using Battle.View.Game;

namespace Battle.View.Projectile
{
    /// <summary>
    /// <span class="brief-h">%Projectile view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles projectile's sprite changes and its trail.
    /// </summary>
    public class BattleProjectileViewController : QuantumEntityViewComponent
    {
        /// @anchor BattleProjectileViewController-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] SpriteRenderer reference for projectile's sprite.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private SpriteRenderer _spriteRenderer;

        /// <summary>[SerializeField] SpriteRenderer reference for projectile's glow.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private SpriteRenderer _spriteGlowRenderer;

        /// <summary>[SerializeField] TrailRenderer reference for projectile's trail.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private TrailRenderer _trailRenderer;

        [Tooltip("Sprite 0: Sadness\nSprite 1: Joy\nSprite 2: Playful\nSprite 3: Aggression\nSprite 4: Love")]
        /// <summary>[SerializeField] An array of projectile sprites.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private Sprite[] _sprites;

        /// <summary>[SerializeField] An array of gradient colors for projectile's trail.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private Gradient[] _colorGradients;

        /// <summary>[SerializeField] An array of glow colors for projectile's glow.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private Color[] _colorGlows;

        /// <summary>[SerializeField] A reference to the trail segment prefab.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private GameObject _trailObject;

        /// <summary>[SerializeField] Speed change interval at which the trail's length updates.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private float _trailIncreaseSpeed;

        /// <summary>[SerializeField] Maximum amount of trail segments that can be active.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private int _maxTrailAmount;

        /// <summary>[SerializeField] The delay in seconds for each trail segment's movement.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private float _trailDelaySec;

        /// @}

        /// <summary>
        /// Public method that is called when entity is activated upon its creation.<br/>
        /// Fetches needed components and subscribes to BattleChangeEmotionState and BattleProjectileChangeGlowStrength.
        /// <a href = "https://doc.photonengine.com/quantum/current/manual/quantum-ecs/game-events"> Quantum Event.@u-exlink</a>
        /// </summary>
        ///
        /// <param name="_">Current simulation frame.</param>
        public override void OnActivate(Frame _)
        {
            _trailObjects = new GameObject[_maxTrailAmount];
            _trailSpriteRenderers = new SpriteRenderer[_maxTrailAmount];

            _savedPositionQueue = new Queue<SavedPosition>[_maxTrailAmount];
            _savedPositionPool = new List<SavedPosition>[_maxTrailAmount];

            for (int i = 0; i < _maxTrailAmount; i++)
            {
                _trailObjects[i] = Instantiate(_trailObject);
                float scaleFactor = 1 - (i + 1) * 0.05f;
                _trailObjects[i].transform.localScale *= scaleFactor;
                _trailObjects[i].SetActive(false);
                _trailSpriteRenderers[i] = _trailObjects[i].GetComponent<SpriteRenderer>();

                _savedPositionQueue[i] = new Queue<SavedPosition>();
                _savedPositionPool[i] = new List<SavedPosition>();
            }

            if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta)
            {
                transform.rotation = Quaternion.Euler(90, 180, 0);
            }

            QuantumEvent.Subscribe<EventBattleChangeEmotionState>(this, OnChangeEmotionState);
            QuantumEvent.Subscribe<EventBattleProjectileChangeSpeed>(this, OnProjectileChangeSpeed);
            QuantumEvent.Subscribe<EventBattleProjectileChangeGlowStrength>(this, OnProjectileChangeGlowStrength);
            QuantumEvent.Subscribe<EventBattleViewGameOver>(this, OnGameOver);

            BattleGameViewController.AssignProjectileReference(gameObject);
        }

        /// <value>Holder variable for the projectile's glow value.</value>
        private float _glowStrength;

        /// <value>Current amount of active trail segments.</value>
        private int _currentTrailAmount;

        /// <value>The speed value at which the trail was last updated.</value>
        private float _previousTrailIncrementSpeed;

        /// <value>The projectile's current speed value.</value>
        private float _currentSpeed;

        /// <value>The projectile's base speed value.</value>
        private float _baseSpeed;

        /// <value></value>
        private Queue<SavedPosition>[] _savedPositionQueue;

        /// <value></value>
        private List<SavedPosition>[] _savedPositionPool;

        /// <value>An array containing all the trail segment gameobjects.</value>
        private GameObject[] _trailObjects;

        /// <value>An array containing all the trail segment spriterenderers.</value>
        private SpriteRenderer[] _trailSpriteRenderers;

        /// <summary>
        /// Private class to create timestamped position objects from for use in handling trail segment delayed movement.
        /// </summary>
        private class SavedPosition
        {
            /// <value>Saved position of the projectile.</value>
            public Vector3 Position;
            /// <value>Timestamp of when this position was saved.</value>
            public float Time;
        }

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method. Handles calling HandleTrail to update each trail segment's position.
        /// </summary>
        private void Update()
        {
            HandleTrail();
        }

        /// <summary>
        /// Handles updating the position of each trail segment.
        /// </summary>
        private void HandleTrail()
        {
            for (int i = 0; i < _currentTrailAmount; i++)
            {

                SavedPosition position = null;
                if (_savedPositionPool[i].Count > 1)
                {
                    position = _savedPositionPool[i][0];
                    _savedPositionPool[i].Remove(position);

                    position.Position = transform.position;
                    position.Time = Time.time;
                }
                else
                {
                    position = new SavedPosition();
                    position.Position = transform.position;
                    position.Time = Time.time;
                }

                _savedPositionQueue[i].Enqueue(position);

                SavedPosition delayPosition = null;
                while (_savedPositionQueue[i].Count > 0)
                {
                    var peek = _savedPositionQueue[i].Peek();
                    if (peek == null || position.Time - peek.Time < _trailDelaySec * (i + 1)) break;

                    delayPosition = _savedPositionQueue[i].Dequeue();
                    _savedPositionPool[i].Add(delayPosition);
                }

                if (delayPosition != null)
                {
                    _trailObjects[i].transform.position = delayPosition.Position;
                }
            }
        }

        /// <summary>
        /// Private method that gets called by Quantum via BattleProjectileChangeSpeed Event.<br/>
        /// Updates saved speed values, and updates the trail if speed has changed enough since previous update.
        /// </summary>
        /// 
        /// <param name="e">BattleProjectileChangeSpeed Event</param>
        private void OnProjectileChangeSpeed(EventBattleProjectileChangeSpeed e)
        {
            float newSpeed = (float)e.NewSpeed;

            if (_baseSpeed == 0) _baseSpeed = newSpeed;
            if (_previousTrailIncrementSpeed == 0) _previousTrailIncrementSpeed = newSpeed;

            _currentSpeed = newSpeed;

            while (_currentSpeed - _previousTrailIncrementSpeed >= _trailIncreaseSpeed)
            { 
                if (_currentTrailAmount < _maxTrailAmount)
                {
                    _currentTrailAmount++;
                    _trailObjects[_currentTrailAmount - 1].SetActive(true);
                    _trailObjects[_currentTrailAmount - 1].transform.position = transform.position;

                    _previousTrailIncrementSpeed += _trailIncreaseSpeed;
                }
                else
                {
                    _previousTrailIncrementSpeed = _currentSpeed;
                }
            }

            if (newSpeed == _baseSpeed)
            {
                for (int i = 0; i < _currentTrailAmount; i++)
                {
                    _trailObjects[i].SetActive(false);
                    _savedPositionQueue[i].Clear();
                    _savedPositionPool[i].Clear();
                }

                _currentTrailAmount = 0;
                _previousTrailIncrementSpeed = _baseSpeed;
            }
        }

        /// <summary>
        /// Private method that gets called by Quantum via BattleChangeEmotionState Event.<br/>
        /// Changes projectile's sprite, glow strength and its trail's color and sprites.
        /// </summary>
        ///
        /// <param name="e">BattleChangeEmotionState Event</param>
        private void OnChangeEmotionState(EventBattleChangeEmotionState e)
        {
            _spriteRenderer.sprite = _sprites[(int)e.Emotion];
            _spriteGlowRenderer.color = _colorGlows[(int)e.Emotion].Alpha(_glowStrength);
            _trailRenderer.colorGradient = _colorGradients[(int)e.Emotion];

            foreach (SpriteRenderer spriteRenderer in _trailSpriteRenderers)
            {
                spriteRenderer.sprite = _sprites[(int)e.Emotion];
            }
        }
        /// <summary>
        /// Private method that gets called by Quantum via BattleProjectileChangeGlowStrength.<br/>
        /// Changes projectile's glow strength.
        /// </summary>
        ///
        /// <param name="e">BattleProjectileChangeGlowStrength Event</param>
        private void OnProjectileChangeGlowStrength(EventBattleProjectileChangeGlowStrength e)
        {
            _glowStrength = (float)e.Strength;
            _spriteGlowRenderer.color = _spriteGlowRenderer.color.Alpha(_glowStrength);
        }

        /// <summary>
        /// Private method that gets called by Quantum via BattleViewGameOver Event.<br/>
        /// Disables trailRenderer and resets trail.
        /// </summary>
        /// 
        /// <param name="e">BattleViewGameOver Event</param>
        private void OnGameOver(EventBattleViewGameOver e)
        {
            _trailRenderer.enabled = false;

            for (int i = 0; i < _currentTrailAmount; i++)
            {
                _trailObjects[i].SetActive(false);
                _savedPositionQueue[i].Clear();
                _savedPositionPool[i].Clear();
            }

            _currentTrailAmount = 0;
            _previousTrailIncrementSpeed = _baseSpeed;
        }
    }
}
