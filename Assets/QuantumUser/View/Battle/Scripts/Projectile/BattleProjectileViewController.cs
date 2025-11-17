/// @file BattleProjectileViewController.cs
/// <summary>
/// Handles projectile's sprite changes and it's trail.
/// </summary>

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

// Battle View usings
using Battle.View.Game;

namespace Battle.View.Projectile
{
    /// <summary>
    /// <span class="brief-h">%Projectile view <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles projectile's sprite changes and it's trail.
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
        [SerializeField] private int _maxTrailCount;

        /// <summary>[SerializeField] The delay in seconds for each trail segment's movement.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private float _trailDelaySec;

        /// <summary>[SerializeField] The amount by which each segment gets smaller compared to the previous one.</summary>
        /// @ref BattleProjectileViewController-SerializeFields
        [SerializeField] private float _trailScaleDegreesAmount;

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
            _trailObjects = new GameObject[_maxTrailCount];
            _trailSpriteRenderers = new SpriteRenderer[_maxTrailCount];

            _trailDelayFrames = (int)(_trailDelaySec / Time.fixedDeltaTime);

            _savedPositionsCount = _trailDelayFrames * (_maxTrailCount + 1);
            _savedPositions = new Vector3[_savedPositionsCount];

            for (int i = 0; i < _maxTrailCount; i++)
            {
                _trailObjects[i] = Instantiate(_trailObject);
                if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta)
                {
                    _trailObjects[i].transform.rotation = Quaternion.Euler(90, 180, 0);
                }
                float scaleFactor = 1 - (i + 1) * 0.05f;
                _trailObjects[i].transform.localScale *= scaleFactor;
                _trailObjects[i].SetActive(false);
                _trailSpriteRenderers[i] = _trailObjects[i].GetComponent<SpriteRenderer>();
            }

            if (BattleGameViewController.LocalPlayerTeam == BattleTeamNumber.TeamBeta)
            {
                transform.rotation = Quaternion.Euler(90, 180, 0);
            }

            QuantumEvent.Subscribe<EventBattleChangeEmotionState>(this, QEventOnChangeEmotionState);
            QuantumEvent.Subscribe<EventBattleProjectileChangeSpeed>(this, QEventOnProjectileChangeSpeed);
            QuantumEvent.Subscribe<EventBattleProjectileChangeGlowStrength>(this, QEventOnProjectileChangeGlowStrength);
            QuantumEvent.Subscribe<EventBattleViewGameOver>(this, QEventOnGameOver);

            BattleGameViewController.AssignProjectileReference(gameObject);
        }

        /// <value>Holder variable for the projectile's glow value.</value>
        private float _glowStrength;

        /// <value>Current amount of active trail segments.</value>
        private int _currentTrailCount;

        /// <value>The speed value at which the trail was last updated.</value>
        private float _previousTrailIncrementSpeed;

        /// <value>The projectile's base speed value.</value>
        private float _baseSpeed;

        /// <value>The time at which FixedUpdate last happened.</value>
        private float _previousFixedUpdateTime;

        /// <value>The trail delay presented in fixed update frames.</value>
        private int _trailDelayFrames;

        /// <value>The index to which the current position is to be saved.</value>
        private int _savedPositionsIndex;

        /// <value>The amount of snapshotted positions that are saved for trail logic.</value>
        private int _savedPositionsCount;

        /// <value>An array containing all saved position snapshots.</value>
        private Vector3[] _savedPositions;

        /// <value>An array containing all the trail segment gameobjects.</value>
        private GameObject[] _trailObjects;

        /// <value>An array containing all the trail segment spriterenderers.</value>
        private SpriteRenderer[] _trailSpriteRenderers;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.FixedUpdate.html">FixedUpdate@u-exlink</a> method. Handles updating the saved position array at fixed intervals.
        /// </summary>
        private void FixedUpdate()
        {
            UpdateTrailArray();

            _previousFixedUpdateTime = Time.time;
        }

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method. Handles updating the trail every frame.
        /// </summary>
        private void Update()
        {
            HandleTrail();
        }

        /// <summary>
        /// Handles updating the saved position array.
        /// </summary>
        private void UpdateTrailArray()
        {
            _savedPositionsIndex = (_savedPositionsIndex + 1) % _savedPositionsCount;

            _savedPositions[_savedPositionsIndex] = transform.position;
        }

        /// <summary>
        /// Handles updating the position of each active segment of the trail.
        /// </summary>
        private void HandleTrail()
        {
            for (int i = 0; i < _currentTrailCount; i++)
            {
                int currentDelayPositionIndex  = _savedPositionsIndex - _trailDelayFrames * (i + 1);
                int previousDelayPositionIndex = currentDelayPositionIndex - 1;
                currentDelayPositionIndex      = (_savedPositionsCount + currentDelayPositionIndex) % _savedPositionsCount;
                previousDelayPositionIndex     = (_savedPositionsCount + previousDelayPositionIndex) % _savedPositionsCount;

                Vector3 currentDelayPosition  = _savedPositions[currentDelayPositionIndex];
                Vector3 previousDelayPosition = _savedPositions[previousDelayPositionIndex];

                if (currentDelayPosition == null) break;

                Vector3 positionDifference = currentDelayPosition - previousDelayPosition;
                float timeDifference       = Time.time - _previousFixedUpdateTime;

                timeDifference /= Time.fixedDeltaTime;

                _trailObjects[i].transform.position = previousDelayPosition + positionDifference * timeDifference;
            }
        }

        /// <summary>
        /// Private method that gets called by Quantum via BattleProjectileChangeSpeed Event.<br/>
        /// Updates saved speed values, and updates the trail if speed has changed enough since previous update.
        /// </summary>
        /// 
        /// <param name="e">BattleProjectileChangeSpeed Event</param>
        private void QEventOnProjectileChangeSpeed(EventBattleProjectileChangeSpeed e)
        {
            float newSpeed = (float)e.NewSpeed;

            if (_baseSpeed == 0)
            {
                _baseSpeed                   = newSpeed;
                _previousTrailIncrementSpeed = newSpeed;
            }

            if (newSpeed == _baseSpeed)
            {
                for (int i = 0; i < _currentTrailCount; i++)
                {
                    _trailObjects[i].SetActive(false);
                }

                _currentTrailCount = 0;
                _previousTrailIncrementSpeed = _baseSpeed;

                return;
            }

            if (_currentTrailCount >= _maxTrailCount)
            {
                return;
            }

            float speedDelta = newSpeed - _previousTrailIncrementSpeed;
            int newTrailAmount = _currentTrailCount + Mathf.FloorToInt(speedDelta / _trailIncreaseSpeed);
            newTrailAmount = Mathf.Min(newTrailAmount, _maxTrailCount);

            for (int i = _currentTrailCount; i < newTrailAmount; i++)
            {
                _trailObjects[i].SetActive(true);
                _trailObjects[i].transform.position = transform.position;
            }

            _currentTrailCount = newTrailAmount;
            _previousTrailIncrementSpeed = _baseSpeed + _trailIncreaseSpeed * _currentTrailCount;
        }

        /// <summary>
        /// Private method that gets called by Quantum via BattleChangeEmotionState Event.<br/>
        /// Changes projectile's sprite, glow strength and its trail's color and sprites.
        /// </summary>
        ///
        /// <param name="e">BattleChangeEmotionState Event</param>
        private void QEventOnChangeEmotionState(EventBattleChangeEmotionState e)
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
        private void QEventOnProjectileChangeGlowStrength(EventBattleProjectileChangeGlowStrength e)
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
        private void QEventOnGameOver(EventBattleViewGameOver e)
        {
            _trailRenderer.enabled = false;

            for (int i = 0; i < _currentTrailCount; i++)
            {
                _trailObjects[i].SetActive(false);
            }

            _currentTrailCount = 0;
            _previousTrailIncrementSpeed = _baseSpeed;
        }
    }
}
