using System;
using Prg.Scripts.Common.Unity;
using UnityEngine;

namespace Battle.Scripts.Test
{
    [Serializable]
    internal class BallSettings
    {
        [Header("Ball movement")] public Vector2 initialVelocity;
        public float minVelocity;

        [Header("Layers")] public LayerMask bounceMask;
        public LayerMask teamAreaMask;
        public LayerMask headMask;
        public LayerMask brickMask;
        public LayerMask wallMask;

        [Header("Tags"), TagSelector] public string redTeamTag;
        [TagSelector] public string blueTeamTag;
    }

    internal class BallReflectTest : MonoBehaviour
    {
        [SerializeField] private BallSettings settings;

        [Header("Live Data"), SerializeField] private bool isRedTeamActive;
        [SerializeField] private bool isBlueTeamActive;

        private Rigidbody2D _rigidbody;

        private int _bounceMaskValue;
        private int _teamAreaMaskValue;
        private int _headMaskValue;
        private int _brickMaskValue;
        private int _wallMaskValue;

        [Header("Time.timeScale")] public float timeScale;

        [Header("Collider Debug")] public int ignoredCount;
        public Collider2D[] ignoredColliders = new Collider2D[4];
        public ContactFilter2D contactFilter;
        private int _overlappingCount;
        private readonly Collider2D[] _overlappingColliders = new Collider2D[4];
        private readonly float[] _overlappingDistance = new float[4];

        // Diamond hack
        private GameObject _topDiamond;
        private GameObject _bottomDiamond;
        private GameObject _upperStoneWall;
        private GameObject _lowerStoneWall;
        [Header("Diamond Debug")] public int upperStoneWallCount;
        public int lowerStoneWallCount;

        private void Awake()
        {
            _bounceMaskValue = settings.bounceMask.value;
            _teamAreaMaskValue = settings.teamAreaMask.value;
            _headMaskValue = settings.headMask.value;
            _brickMaskValue = settings.brickMask.value;
            _wallMaskValue = settings.wallMask.value;
            _rigidbody = GetComponent<Rigidbody2D>();
            // We need to track these colliders while ball bounces
            contactFilter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true,
                layerMask = settings.wallMask.value + settings.brickMask.value // Implicitly converts an integer to a LayerMask
            };
            _topDiamond = GameObject.Find("TopDiamond");
            _bottomDiamond = GameObject.Find("BottomDiamond");
            _upperStoneWall = GameObject.Find("UpperStoneWall");
            _lowerStoneWall = GameObject.Find("LowerStoneWall");
            upperStoneWallCount = _upperStoneWall.transform.childCount;
            lowerStoneWallCount = _lowerStoneWall.transform.childCount;
            if (upperStoneWallCount > 0)
            {
                _topDiamond.SetActive(false);
            }
            if (lowerStoneWallCount > 0)
            {
                _bottomDiamond.SetActive(false);
            }
        }

        private void OnEnable()
        {
            _rigidbody.velocity = settings.initialVelocity;
            if (timeScale > 1f)
            {
                Time.timeScale = timeScale;
                Debug.Log($"SET Time.timeScale {Time.timeScale:F3}");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"ignore {other.name} layer {other.gameObject.layer}");
                return;
            }
            var colliderMask = 1 << layer;
            if (_bounceMaskValue == (_bounceMaskValue | colliderMask))
            {
                Bounce(other);
                return;
            }
            if (_teamAreaMaskValue == (_teamAreaMaskValue | colliderMask))
            {
                TeamEnter(otherGameObject);
                return;
            }
            if (_brickMaskValue == (_brickMaskValue | colliderMask))
            {
                Bounce(other);
                Brick(otherGameObject);
                return;
            }
            Debug.Log($"UNHANDLED hit {other.name} layer {layer}");
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (ignoredCount > 0)
            {
                for (var i = 0; i < ignoredCount; ++i)
                    if (ignoredColliders[i].Equals(other))
                    {
                        return;
                    }
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                return;
            }
            var colliderMask = 1 << layer;
            if (_teamAreaMaskValue == (_teamAreaMaskValue | colliderMask))
            {
                return;
            }

            Debug.Log($"STOP @ {_rigidbody.position} on STAY hit {other.name} frame {Time.frameCount}");
            _rigidbody.velocity = Vector2.zero;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            if (ignoredCount > 0)
            {
                if (RemoveIgnoredCollider(other))
                {
                    return;
                }
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                return;
            }
            var colliderMask = 1 << layer;
            if (_teamAreaMaskValue == (_teamAreaMaskValue | colliderMask))
            {
                TeamExit(otherGameObject);
            }
        }

        private void AddIgnoredCollider(Collider2D other)
        {
            ignoredColliders[ignoredCount] = other;
            ignoredCount += 1;
        }

        private bool RemoveIgnoredCollider(Collider2D other)
        {
            for (var i = 0; i < ignoredCount; ++i)
                if (ignoredColliders[i].Equals(other))
                {
                    Debug.Log($"REMOVE ignore {other.name} frame {Time.frameCount} ignored {ignoredCount}");
                    if (ignoredCount == 1)
                    {
                        ignoredColliders[i] = null;
                        ignoredCount = 0;
                        return true;
                    }
                    // Move items down by one
                    Array.Copy(ignoredColliders, i + 1, ignoredColliders, i, ignoredColliders.Length - 2);
                    ignoredColliders[ignoredCount] = null;
                    ignoredCount -= 1;
                    return true;
                }
            return false;
        }

        private void Bounce(Collider2D other)
        {
            if (ignoredCount > 0)
            {
                for (var i = 0; i < ignoredCount; ++i)
                    if (ignoredColliders[i].Equals(other))
                    {
                        Debug.Log($"SKIP ignore {other.name} frame {Time.frameCount} ignored {ignoredCount}");
                        return;
                    }
            }
            _overlappingCount = _rigidbody.OverlapCollider(contactFilter, _overlappingColliders);
            if (_overlappingCount < 2)
            {
                BounceAndReflect(other);
                return;
            }
            // Count wall colliders and print print all colliders
            var wallColliderCount = 0;
            var position = _rigidbody.position;
            for (var i = 0; i < _overlappingColliders.Length; i++)
                if (i < _overlappingCount)
                {
                    var overlappingCollider = _overlappingColliders[i];
                    var closest = overlappingCollider.ClosestPoint(_rigidbody.position);
                    _overlappingDistance[i] = (closest - position).sqrMagnitude;
                    if (overlappingCollider.name.EndsWith("Wall"))
                    {
                        wallColliderCount += 1;
                    }
                    Debug.Log(
                        $"overlapping {other.name} {i}/{_overlappingCount} {overlappingCollider.name} pos {closest} dist {Mathf.Sqrt(_overlappingDistance[i]):F3}");
                }
                else
                {
                    _overlappingColliders[i] = null;
                }
            if (wallColliderCount == _overlappingCount)
            {
                // Let wall colliders run normally
                BounceAndReflect(other);
                return;
            }
            // Collide with nearest only
            var nearest = 0;
            for (var i = 1; i < _overlappingCount; i++)
                if (_overlappingDistance[i] < _overlappingDistance[nearest])
                {
                    nearest = i;
                }
            // Add everything to ignored colliders so that ball can move out while bouncing
            ignoredCount = 0;
            for (var i = 0; i < _overlappingCount; i++) AddIgnoredCollider(_overlappingColliders[i]);
            // Do the bounce
            var nearestCollider = _overlappingColliders[nearest];
            BounceAndReflect(nearestCollider);
        }

        private void Brick(GameObject brick)
        {
            Debug.Log($"Destroy {brick.name} brick count {upperStoneWallCount + lowerStoneWallCount}");
            Destroy(brick);
            if (ignoredCount > 0)
            {
                var brickCollider = brick.GetComponent<Collider2D>();
                RemoveIgnoredCollider(brickCollider);
            }
            // Diamond hack
            brick.transform.SetParent(null);
            if (upperStoneWallCount > 0)
            {
                upperStoneWallCount = _upperStoneWall.transform.childCount;
                if (upperStoneWallCount == 0)
                {
                    _topDiamond.SetActive(true);
                    Debug.Log($"SetActive {_topDiamond.name}");
                    var diamondCollider = _topDiamond.GetComponent<Collider2D>();
                    AddIgnoredCollider(diamondCollider);
                }
            }
            if (lowerStoneWallCount > 0)
            {
                lowerStoneWallCount = _lowerStoneWall.transform.childCount;
                if (lowerStoneWallCount == 0)
                {
                    _bottomDiamond.SetActive(true);
                    Debug.Log($"SetActive {_bottomDiamond.name}");
                    var diamondCollider = _bottomDiamond.GetComponent<Collider2D>();
                    AddIgnoredCollider(diamondCollider);
                }
            }
        }

        private void TeamEnter(GameObject teamArea)
        {
            if (teamArea.CompareTag(settings.redTeamTag))
            {
                if (!isRedTeamActive)
                {
                    Debug.Log("isRedTeamActive <- false");
                }
                isRedTeamActive = false;
                return;
            }
            if (teamArea.CompareTag(settings.blueTeamTag))
            {
                if (!isBlueTeamActive)
                {
                    Debug.Log("isBlueTeamActive <- false");
                }
                isBlueTeamActive = false;
            }
        }

        private void TeamExit(GameObject teamArea)
        {
            if (teamArea.CompareTag(settings.redTeamTag))
            {
                if (isRedTeamActive)
                {
                    Debug.Log("isRedTeamActive <- true");
                }
                isRedTeamActive = true;
                return;
            }
            if (teamArea.CompareTag(settings.blueTeamTag))
            {
                if (isBlueTeamActive)
                {
                    Debug.Log("isBlueTeamActive <- true");
                }
                isBlueTeamActive = true;
            }
        }

        private void BounceAndReflect(Collider2D other)
        {
            var currentVelocity = _rigidbody.velocity;
            var position = _rigidbody.position;
            var closestPoint = other.ClosestPoint(position);
            var direction = closestPoint - position;
            Reflect(currentVelocity, direction.normalized);
            Debug.Log(
                $"bounce {other.name} @ {position} closest {closestPoint} dir {currentVelocity} <- {_rigidbody.velocity} frame {Time.frameCount} ol-count {_overlappingCount}");
        }

        private void Reflect(Vector2 currentVelocity, Vector2 collisionNormal)
        {
            var speed = currentVelocity.magnitude;
            var direction = Vector2.Reflect(currentVelocity.normalized, collisionNormal);
            _rigidbody.velocity = direction * Mathf.Max(speed, settings.minVelocity);
        }
    }
}