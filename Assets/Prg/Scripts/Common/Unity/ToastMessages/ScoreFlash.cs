using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Prg.Scripts.Common.Unity.ToastMessages
{
    /// <summary>
    /// Flash message system for current level. It will destroy itself when scene is unloaded!
    /// </summary>
    public static class ScoreFlash
    {
        private static IScoreFlash Get()
        {
            if (ScoreFlasher.ScoreFlash == null)
            {
                ScoreFlasher.ScoreFlash = Object.FindObjectOfType<ScoreFlasher>();
                if (ScoreFlasher.ScoreFlash == null)
                {
                    var instance = UnitySingleton.CreateGameObjectAndComponent<ScoreFlasher>();
                    ScoreFlasher.ScoreFlash = instance;
                    var config = Resources.Load<ScoreFlashConfig>(nameof(ScoreFlashConfig));
                    instance.Setup(config, Camera.main);
                }
            }
            return ScoreFlasher.ScoreFlash;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            ScoreFlasher.ScoreFlash = null;
        }

        public static void Push(string message)
        {
            Get().Push(message, 0f, 0f);
        }

        public static void Push(string message, float worldX, float worldY)
        {
            Get().Push(message, worldX, worldY);
        }

        public static void Push(string message, Vector2 worldPosition)
        {
            Get().Push(message, worldPosition.x, worldPosition.y);
        }

        public static void Push(string message, Vector3 worldPosition)
        {
            Get().Push(message, worldPosition.x, worldPosition.y);
        }
    }

    public interface IScoreFlash
    {
        void Push(string message, float worldX, float y);
    }

    [Serializable]
    internal class ScoreFlashItem
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private RectTransform _rectTransform;

        [SerializeField] private TextMeshProUGUI _uiText;
        [SerializeField] private RectTransform _textRectTransform;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Rect _rect;

        public readonly int Index;

        public Vector2 Position => _position;

        public ScoreFlashItem(int index, GameObject root, TextMeshProUGUI uiText)
        {
            Index = index;
            _root = root;
            _rectTransform = _root.GetComponent<RectTransform>();
            _uiText = uiText;
            _textRectTransform = _uiText.GetComponent<RectTransform>();
            _position.z = _root.GetComponent<Transform>().position.z;
        }

        private Rect GetTextRect()
        {
            _rect = _textRectTransform.rect;
            var localPosition = _rectTransform.localPosition;
            _rect.x = localPosition.x;
            _rect.y = localPosition.y;
            return _rect;
        }

        public float TextHeight => GetTextRect().height;

        public void SetText(string text)
        {
            _uiText.text = text;
        }

        public void SetPosition(float x, float y)
        {
            _position.x = x;
            _position.y = y;
            _rectTransform.anchoredPosition = _position;
        }

        public void Move(float deltaX, float deltaY)
        {
            SetPosition(_position.x + deltaX, _position.y + deltaY);
        }

        public void SetRotation(float angleZ)
        {
            _rectTransform.rotation = Quaternion.identity;
            _rectTransform.Rotate(0f, 0f, angleZ);
        }

        public void SetScale(float scale)
        {
            var localScale = _rectTransform.localScale;
            localScale.x = scale;
            localScale.y = scale;
            _rectTransform.localScale = localScale;
        }

        public void SetColor(Color color)
        {
            _uiText.color = color;
        }

        public void Show()
        {
            Assert.IsTrue(_root != null, "_root != null");
            _root.SetActive(true);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        public bool Overlaps(ScoreFlashItem other)
        {
            var textRect = GetTextRect();
            var otherRect = other.GetTextRect();
            Debug.Log($"Overlaps index {Index} {textRect} <-> index {other.Index} {otherRect}");
            return textRect.Overlaps(otherRect);
        }
    }

    internal class ScoreFlasher : MonoBehaviour, IScoreFlash
    {
        /// <summary>
        /// Reference to static <c>IScoreFlash</c> instance during our own lifetime to ensure that is is set to null when level is unloaded.
        /// </summary>
        public static IScoreFlash ScoreFlash;

        private const float InflateScreenX = -0.20f;
        private const float InflateScreenY = -0.10f;

        [SerializeField] private Camera _camera;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _canvasRectTransform;

        [SerializeField] private ScoreFlashItem[] _entries;
        [SerializeField] private Animator[] _animators;
        [SerializeField] private int _curIndex;
        private Coroutine[] _routines;

        private bool _isClampToScreen;
        private Rect _messageAreaScreen;
        private Vector3 _worldPos;
        private Vector3 _screenPos;
        private Vector2 _localPos;

        public void Setup(ScoreFlashConfig config, Camera screenCamera)
        {
            Assert.IsNotNull(screenCamera, "screenCamera != null");
            _camera = screenCamera;
            _isClampToScreen = config._isClampToScreen;
            if (_isClampToScreen)
            {
                var width = Screen.width;
                var height = Screen.height;
                var screenRect = Rect.MinMaxRect(0, 0, width, height);
                _messageAreaScreen = screenRect.Inflate(new Vector2(width * InflateScreenX, height * InflateScreenY));
                Debug.Log($"screenRect {screenRect} -> _messageAreaScreen {_messageAreaScreen}");
                Debug.Log($"_messageAreaScreen x [{_messageAreaScreen.xMin} .. {_messageAreaScreen.xMax}] " +
                          $"y [{_messageAreaScreen.yMin} .. {_messageAreaScreen.yMax}]");
            }
            _canvas = Instantiate(config._canvasPrefab, Vector3.zero, Quaternion.identity);
            Assert.IsTrue(_canvas.isRootCanvas, "_canvas.isRootCanvas");
            _canvasRectTransform = _canvas.GetComponent<RectTransform>();

            var children = _canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            Debug.Log($"children {children.Length}");
            _entries = new ScoreFlashItem[children.Length];
            _animators = new Animator[children.Length];
            _routines = new Coroutine[children.Length];
            for (var i = 0; i < children.Length; ++i)
            {
                var parent = children[i].gameObject;
                _entries[i] = new ScoreFlashItem(i, parent, children[i]);
                _entries[i].Hide();
                _animators[i] = new Animator(config._phases);
                _routines[i] = null;
            }
            _curIndex = -1;
        }

        private void OnDestroy()
        {
            Debug.Log($"{name}");
            for (var i = 0; i < _entries.Length; ++i)
            {
                StopCoroutine(i);
            }
            ScoreFlash = null;
        }

        private void SetText(string text, float x, float y)
        {
            _curIndex += 1;
            if (_curIndex >= _entries.Length)
            {
                _curIndex = 0;
            }
            StopCoroutine(_curIndex);
            _routines[_curIndex] = StartCoroutine(AnimateText(_animators[_curIndex], _entries[_curIndex], text, x, y));
        }

        private void StopCoroutine(int index)
        {
            var routine = _routines[index];
            if (routine != null)
            {
                Debug.Log($"index {index}");
                StopCoroutine(routine);
                _routines[index] = null;
            }
            var animator = _animators[index];
            if (animator.IsWorking)
            {
                animator.Release();
            }
        }

        private ScoreFlashItem Previous(ScoreFlashItem entry)
        {
            if (_entries.Length == 1)
            {
                return null;
            }
            var index = entry.Index == 0 ? _entries.Length - 1 : entry.Index - 1;
            return _animators[index].IsWorking ? _entries[index] : null;
        }

        private IEnumerator AnimateText(Animator animator, ScoreFlashItem entry, string text, float x, float y)
        {
            animator.Reserve(entry);
            yield return null;
            animator.Start(x, y);
            CheckOverlapping(entry);
            yield return null;
            entry.SetText(text);
            while (animator.Animate(Time.deltaTime))
            {
                yield return null;
            }
            animator.Release();
            _routines[entry.Index] = null;
        }

        private void CheckOverlapping(ScoreFlashItem current)
        {
            var previous = Previous(current);
            var first = previous;
            while (previous != null && previous.Overlaps(current))
            {
                MoveAway(current, previous);
                current = previous;
                previous = Previous(current);
                if (ReferenceEquals(first, previous))
                {
                    Debug.LogWarning($"CheckOverlapping overflow: items ({_entries.Length}) array is full");
                    break;
                }
            }
        }

        private void MoveAway(ScoreFlashItem current, ScoreFlashItem previous)
        {
            Debug.Log($"current {current.Index} {current.Position} previous {previous.Index} {previous.Position}");
            var animator = _animators[previous.Index];
            animator.MoveAway();
        }

        void IScoreFlash.Push(string message, float worldX, float worldY)
        {
            _worldPos.x = worldX;
            _worldPos.y = worldY;
            _screenPos = _camera.WorldToScreenPoint(_worldPos);
            if (_isClampToScreen)
            {
                var temp = _screenPos;
                _screenPos.x = Mathf.Clamp(_screenPos.x, _messageAreaScreen.xMin, _messageAreaScreen.xMax);
                _screenPos.y = Mathf.Clamp(_screenPos.y, _messageAreaScreen.yMin, _messageAreaScreen.yMax);
                Debug.Log($"Clamp {temp} <- {_screenPos} delta x {Mathf.Abs(temp.x - _screenPos.x)} y {Mathf.Abs(temp.y - _screenPos.y)}");
            }

            var hit = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform, _screenPos, null, out _localPos);
            Assert.IsTrue(hit, "RectTransformUtility.ScreenPointToLocalPointInRectangle was hit");
            Debug.Log($"{message} @ WorldToScreenPoint {(Vector2)_worldPos} -> {(Vector2)_screenPos} -> rect {_localPos}");

            SetText(message, _localPos.x, _localPos.y);
        }

        [Serializable]
        private class Animator
        {
            private readonly ScoreFlashPhases _phases;

            [SerializeField] private float _elapsedTime;
            [SerializeField] private float _duration;
            [SerializeField] private float _fraction;
            [SerializeField] private ScoreFlashItem _entry;

            private float _fadeOutRotationAngle;
            private float _fadeOutRotationSpeed;

            private bool _isOverlapped;
            private float _overlappedDistance;
            private float _overlappedPercent;
            private float _overlappedTimeTravelled;

            public bool IsWorking { get; private set; }

            public Animator(ScoreFlashPhases phases)
            {
                _phases = phases;
            }

            public bool Animate(float elapsedTime)
            {
                _elapsedTime = elapsedTime;
                _duration += elapsedTime;
                if (_duration < _phases._fadeInTimeSeconds)
                {
                    FadeInPhase();
                    return true;
                }
                if (_duration < _phases._fadeInTimeSeconds + _phases._stayTimeSeconds)
                {
                    StayVisiblePhase();
                    return true;
                }
                if (_duration < _phases._fadeInTimeSeconds + _phases._stayTimeSeconds + _phases._fadeOutTimeSeconds)
                {
                    FadeOutPhase();
                    return true;
                }
                _entry.Hide();
                return false;
            }

            public void Reserve(ScoreFlashItem entry)
            {
                _entry = entry;
                Assert.IsFalse(IsWorking, $"IsWorking index {_entry.Index}");
                IsWorking = true;
            }

            public void Release()
            {
                Assert.IsTrue(IsWorking, $"IsWorking index {_entry.Index}");
                _entry = null;
                IsWorking = false;
            }

            public void MoveAway()
            {
                _isOverlapped = true;
                _overlappedPercent = 0;
                _overlappedTimeTravelled = 0;
                _overlappedDistance = _entry.TextHeight * _phases._overlappingHeightMultiplier;
            }

            public void Start(float x, float y)
            {
                _duration = 0;
                _fadeOutRotationAngle = 0;
                _fadeOutRotationSpeed = _phases._fadeOutRotationInitialSpeed;
                _entry.SetColor(_phases._fadeInColor);
                _entry.SetScale(_phases._fadeInScale);
                _entry.SetText(string.Empty);
                _entry.SetPosition(x, y);
                _entry.SetRotation(0);
                _entry.Show();
            }

            private void FadeInPhase()
            {
                _fraction = _duration / _phases._fadeInTimeSeconds;
                var textColor = Easing.EaseOnCurve(_phases._fadeInColorCurve, _phases._fadeInColor, _phases._stayColorStart, _fraction);
                _entry.SetColor(textColor);
                var scale = Easing.EaseOnCurve(_phases._fadeInScaleCurve, _phases._fadeInScale, 1f, _fraction);
                _entry.SetScale(scale);
                var x = Easing.EaseOnCurve(_phases._fadeInOffsetXCurve, _phases._fadeInOffsetX, 0, _fraction);
                var y = Easing.EaseOnCurve(_phases._fadeInOffsetYCurve, _phases._fadeInOffsetY, 0, _fraction);
                Move(x * _elapsedTime, y * _elapsedTime);
            }

            private void StayVisiblePhase()
            {
                _fraction = (_duration - _phases._fadeInTimeSeconds) / _phases._stayTimeSeconds;
                var textColor = Easing.EaseOnCurve(_phases._readColorCurve, _phases._stayColorStart, _phases._stayColorEnd, _fraction);
                _entry.SetColor(textColor);
                var scale = Easing.EaseOnCurve(_phases._stayScaleCurve, 1f, _phases._stayScale, _fraction);
                _entry.SetScale(scale);
                var x = Easing.EaseOnCurve(_phases._stayVelocityXCurve, 0, _phases._stayVelocityFloatRight, _fraction);
                var y = Easing.EaseOnCurve(_phases._stayVelocityYCurve, 0, _phases._stayVelocityFloatUp, _fraction);
                Move(x * _elapsedTime, -y * _elapsedTime);
            }

            private void FadeOutPhase()
            {
                _fraction = (_duration - _phases._fadeInTimeSeconds - _phases._stayTimeSeconds) / _phases._fadeOutTimeSeconds;
                var textColor = Easing.EaseOnCurve(_phases._fadeOutColorCurve, _phases._stayColorEnd, _phases._fadeOutColor, _fraction);
                _entry.SetColor(textColor);
                var scale = Easing.EaseOnCurve(_phases._fadeOutScaleCurve, _phases._stayScale, _phases._fadeOutScale, _fraction);
                _entry.SetScale(scale);
                var x = Easing.EaseOnCurve(
                    _phases._fadeOutVelocityXCurve, _phases._stayVelocityFloatRight, _phases._fadeOutVelocityFloatRight, _fraction);
                var y = Easing.EaseOnCurve(
                    _phases._fadeOutVelocityYCurve, _phases._stayVelocityFloatUp, _phases._fadeOutVelocityFloatUp, _fraction);
                Move(x * _elapsedTime, -y * _elapsedTime);
                _fadeOutRotationSpeed += _phases._fadeOutRotationAcceleration * _elapsedTime;
                _fadeOutRotationAngle += _fadeOutRotationSpeed * _elapsedTime;
                _entry.SetRotation(_fadeOutRotationAngle);
            }

            private void Move(float deltaX, float deltaY)
            {
                if (_isOverlapped)
                {
                    _overlappedTimeTravelled += _elapsedTime;
                    var fraction = _overlappedTimeTravelled / _phases._overlappingTimeSeconds;
                    var newPercent = Easing.EaseOnCurve(_phases._overlappingCurve, 0, 1, fraction);
                    var sign = Mathf.Sign(deltaY);
                    var deltaDistance = (newPercent - _overlappedPercent) * _overlappedDistance * 100f;
                    deltaY += sign * deltaDistance * _elapsedTime;
                    if (_overlappedTimeTravelled < _phases._overlappingTimeSeconds)
                    {
                        _overlappedPercent = newPercent;
                    }
                    else
                    {
                        _isOverlapped = false;
                    }
                }
                _entry.Move(deltaX, deltaY);
            }

            private static class Easing
            {
                public static Color EaseOnCurve(AnimationCurve curve, Color from, Color to, float time)
                {
                    var distance = to - from;
                    return from + curve.Evaluate(time) * distance;
                }

                public static float EaseOnCurve(AnimationCurve curve, float from, float to, float time)
                {
                    var distance = to - from;
                    return from + curve.Evaluate(time) * distance;
                }
            }
        }
    }
}