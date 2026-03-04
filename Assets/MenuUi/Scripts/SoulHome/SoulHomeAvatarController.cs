using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D.Animation;

namespace MenuUI.Scripts.SoulHome
{
    public enum AvatarStatus
    {
        Idle,
        Wander
    }

    public class SoulHomeAvatarController : MonoBehaviour, ISoulHomeObjectClick
    {
        [SerializeField] private float _minIdleTimer = 2f;
        [SerializeField] private float _maxIdleTimer = 4f;
        [SerializeField] private float _speed = 5;
        // how far away the avatar stays from furniture
        [SerializeField] private float _movePadding = 2f;
        [SerializeField]
        private SortingGroup _sortingGroup;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private AnimationClip _idleAnimation;
        [SerializeField]
        private AnimationClip _walkAnimation;
        [SerializeField]
        private List<AnimationClip> _interactAnimation;

        private bool _performingAnimation = false;

        private AvatarStatus _status;

        private Transform _points;
        private RoomData _roomData;
        private List<Vector2> _travelPoints = new();

        private AvatarRig _rig;
        private SpriteResolver _lHandResolver;
        private SpriteResolver _rHandResolver;
        private string _lHandLabel;
        private string _rHandLabel;

        private Coroutine _statusCoroutine;
        private List<AnimationClip> _verifiedInteractClips = new();
        public AvatarStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                OnStatusChanged();
            }
        }
        void Start()
        {
            if (transform.parent.CompareTag("Room"))
            {
                _points = transform.parent.Find("FurniturePoints").Find("FloorFurniturePoints");
                _roomData = transform.parent.GetComponent<RoomData>();

                SetAnimationClips();

                SetAvatar(_points, _roomData);
                OnStatusChanged();

                _rig = GetComponentInChildren<AvatarRig>();
                if (_rig == null)
                {
                    Debug.LogError("Failed to get AvatarRig");
                    return;
                }

                _lHandResolver = _rig.Resolvers[AvatarPart.L_Hand];
                _rHandResolver = _rig.Resolvers[AvatarPart.R_Hand];
                _lHandLabel = _lHandResolver.GetLabel();
                _rHandLabel = _rHandResolver.GetLabel();
            }
        }
        private void OnEnable()
        {
            if (_lHandResolver != null && _rHandResolver != null)
            {
                _lHandLabel = _lHandResolver.GetLabel();
                _rHandLabel = _rHandResolver.GetLabel();
            }
        }

        private void SetAnimationClips()
        {
            AnimationClip[] animatorClips = _animator.runtimeAnimatorController.animationClips;

            foreach (AnimationClip clip in _interactAnimation)
            {
                foreach (AnimationClip controllerClip in animatorClips)
                {
                    if (controllerClip == clip)
                    {
                        _verifiedInteractClips.Add(clip);
                        break;
                    }
                }
            }
        }

        private void SelectStatus()
        {
            if (Status == AvatarStatus.Idle) Status = AvatarStatus.Wander;
            else if (Status == AvatarStatus.Wander) Status = AvatarStatus.Idle;
        }

        private void OnStatusChanged()
        {
            if (_statusCoroutine != null)
            {
                StopCoroutine(_statusCoroutine);
            }

            switch(_status)
            {
                case AvatarStatus.Idle:
                    _statusCoroutine = StartCoroutine(HandleIdle());
                    break;
                case AvatarStatus.Wander:
                    HandleWander();
                    break;
            }
        }

        private IEnumerator HandleIdle()
        {
            float idleTime = Random.Range(_minIdleTimer, _maxIdleTimer);
            float elapsed = 0f;

            while (elapsed < idleTime)
            {
                elapsed += Time.deltaTime;

                if (!_animator.GetCurrentAnimatorStateInfo(0).IsName(_idleAnimation.name))
                {
                    _animator.Play(_idleAnimation.name);
                    UseDefaultHands(false);
                }
                yield return null;
                
            }

            SelectStatus();
        }

        private void HandleWander()
        {
            Debug.LogError("wander");
            SelectStatus();
        }


        public void SetAvatar(Transform points, RoomData data)
        {
            int column;
            int row;
            while (true)
            {
                column = Random.Range(0, data.SlotColumns - 1);
                row = Random.Range(0, data.SlotRows - 1);

                if (points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>().Furniture != null) continue;

                if(column + 1 < data.SlotColumns)
                {
                    if (points.GetChild(row).GetChild(column + 1).GetComponent<FurnitureSlot>().Furniture == null) break;
                }
                if (column - 1 >= 0)
                {
                    if (points.GetChild(row).GetChild(column - 1).GetComponent<FurnitureSlot>().Furniture == null)
                    {
                        column--;
                        break;
                    }
                }
            }
            //transform.SetParent(points.GetChild(row).GetChild(column), false);
            FurnitureSlot slot = points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
            _sortingGroup.sortingOrder = 6 + (slot.row) * 100;

            Vector2 position = slot.transform.position;

            float width = slot.width*2;
            position.x += (width / 2) - slot.width / 2;
            position.y += -1 * (slot.height / 2);

            transform.position = position;

            Status = AvatarStatus.Idle;
            _travelPoints.Clear();
        }


        private IEnumerator InteractAnimation()
        {
            if (_performingAnimation || _verifiedInteractClips.Count == 0)
            {
                yield break;
            }

            int index = Random.Range(0, _verifiedInteractClips.Count);
            AnimationClip selectedClip = _verifiedInteractClips[index];

            _animator.Play(selectedClip.name);
            _performingAnimation = true;
            UseDefaultHands(true);
            yield return null;

            yield return new WaitUntil(() => !_animator.GetCurrentAnimatorStateInfo(0).IsName(selectedClip.name) && !_animator.IsInTransition(0));
            _performingAnimation = false;
            UseDefaultHands(false);

            Status = AvatarStatus.Idle;
        }


        private void UseDefaultHands(bool useDefaultHands)
        {
            if (_lHandResolver == null || _rHandResolver == null)
            {
                return;
            }

            string category = _lHandResolver.GetCategory();
            if (useDefaultHands)
            {
                _lHandResolver.SetCategoryAndLabel(category, "0000000L");
                _rHandResolver.SetCategoryAndLabel(category, "0000000R");
            }
            else
            {
                _lHandResolver.SetCategoryAndLabel(category, _lHandLabel);
                _rHandResolver.SetCategoryAndLabel(category, _rHandLabel);
            }
        }

        public void HandleClick()
        {
            if (_performingAnimation)
            {
                return;
            }

            if (_statusCoroutine != null)
            {
                StopCoroutine(_statusCoroutine);
            }

            StartCoroutine(InteractAnimation());
        }
    }
}
