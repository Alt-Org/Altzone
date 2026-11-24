using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.Jukebox
{
    public class JukeboxDiskBase : MonoBehaviour
    {
        [SerializeField] protected Image _mainDiskImage;
        [SerializeField] protected Sprite _emptyDiskSprite;
        [SerializeField] protected float _diskRotationSpeed = 100f;
        [Header("Switch Disk Animation")]
        [SerializeField] protected bool _useAnimation = false;
        [SerializeField] protected float _switchDiskAnimDuration = 1f;
        [SerializeField] protected AnimationCurve _switchDiskAnimCurve;
        [SerializeField] protected RectTransform _mainDiskRectTransform;
        [SerializeField] protected RectTransform _secondaryDiskRectTransform;
        [SerializeField] protected Image _secondaryDiskImage;

        private Coroutine _diskSpinCoroutine;

        #region Animation
        protected Vector2 _mainAnchorMinStart = new(1f, 0f);
        protected Vector2 _mainAnchorMaxStart = new(1f, 1f);
        protected Vector2 _mainAnchorMinEnd = new(0f, 0f);
        protected Vector2 _mainAnchorMaxEnd = new(1f, 1f);

        protected Vector2 _secondaryAnchorMinStart = new(0f, 0f);
        protected Vector2 _secondaryAnchorMaxStart = new(1f, 1f);
        protected Vector2 _secondaryAnchorMinEnd = new(0f, 0f);
        protected Vector2 _secondaryAnchorMaxEnd = new(0f, 1f);
        #endregion

        #region Base
        public void SetDisk(Sprite sprite)
        {
            if (sprite == _mainDiskImage.sprite) return;

            if (_useAnimation && _secondaryDiskRectTransform)
                StartCoroutine(SwitchDiskViaAnimation(sprite, null));
            else
                _mainDiskImage.sprite = sprite;
        }

        public bool StartDiskSpin()
        {
            if (_diskSpinCoroutine != null || !isActiveAndEnabled) return false;

            _diskSpinCoroutine = StartCoroutine(SpinDisk());
            return true;
        }

        public void StopDiskSpin()
        {
            if (_diskSpinCoroutine != null)
            {
                StopCoroutine(_diskSpinCoroutine);
                _diskSpinCoroutine = null;
            }

            _mainDiskImage.transform.rotation = Quaternion.identity;
        }

        public void ClearDisk() { StopDiskSpin(); _mainDiskImage.sprite = _emptyDiskSprite; }

        private IEnumerator SpinDisk()
        {
            while (true)
            {
                _mainDiskImage.transform.Rotate(Vector3.forward * (-_diskRotationSpeed * Time.deltaTime));

                yield return null;
            }
        }
        #endregion

        #region Animation
        public IEnumerator SwitchDiskViaAnimation(Sprite sprite, System.Action<bool> done)
        {
            bool? diskSwitchDone = null;

            _secondaryDiskImage.sprite = _mainDiskImage.sprite;
            _mainDiskImage.sprite = sprite;

            _mainDiskRectTransform.anchorMin = _mainAnchorMinStart;
            _mainDiskRectTransform.anchorMax = _mainAnchorMaxStart;

            _secondaryDiskRectTransform.anchorMin = _secondaryAnchorMinStart;
            _secondaryDiskRectTransform.anchorMax = _secondaryAnchorMaxStart;

            StopDiskSpin();
            StartCoroutine(SwitchDisk((data) => diskSwitchDone = data));

            yield return new WaitUntil(() => diskSwitchDone != null);

            done?.Invoke(true);
        }

        private IEnumerator SwitchDisk(System.Action<bool> done)
        {
            float timer = 0f;

            while (timer < _switchDiskAnimDuration)
            {
                float animatedFloat = _switchDiskAnimCurve.Evaluate(timer / _switchDiskAnimDuration);

                _mainDiskRectTransform.anchorMin = Vector2.Lerp(_mainAnchorMinStart, _mainAnchorMinEnd, animatedFloat);
                _mainDiskRectTransform.anchorMax = Vector2.Lerp(_mainAnchorMaxStart, _mainAnchorMaxEnd, animatedFloat);

                _secondaryDiskRectTransform.anchorMin = Vector2.Lerp(_secondaryAnchorMinStart, _secondaryAnchorMinEnd, animatedFloat);
                _secondaryDiskRectTransform.anchorMax = Vector2.Lerp(_secondaryAnchorMaxStart, _secondaryAnchorMaxEnd, animatedFloat);

                yield return null;

                timer += Time.deltaTime;
            }

            done?.Invoke(true);
        }
        #endregion
    }
}
