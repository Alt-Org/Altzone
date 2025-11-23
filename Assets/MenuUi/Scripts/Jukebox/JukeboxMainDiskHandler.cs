using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxMainDiskHandler : MonoBehaviour
{
    [SerializeField] private Image _diskImage;
    [SerializeField] private Sprite _emptyDiskSprite;
    [SerializeField] private float _diskRotationSpeed = 100f;
    [Space]
    [SerializeField] private GameObject _offlineIndicatorContent;
    [SerializeField] private Button _multiUseButton;
    [SerializeField] private TMPro.TextMeshProUGUI _indicatorText;
    [SerializeField] private GameObject _customIndicatorImageHolder;

    private Coroutine _diskSpinCoroutine;

    public delegate void MultiUseButtonPressed();
    public event MultiUseButtonPressed OnMultiUseButtonPressed;

    private List<string> _indicatorTexts = new List<string>()
    {
        "",
        "Esikuuntelu\r\nPäällä",
        "Jukeboxi\r\nMykistetty",
        "Pysäytetty",
        "Soittolista\r\nTyhjä",
        "Mykistetty\r\nAsetuksista"
    };

    public enum JukeboxDiskTextType
    {
        None = 0,
        Preview = 1,
        Muted = 2,
        Stopped = 3,
        Empty = 4,
        VolumeZero = 5
    }

    private void Awake()
    {
        if (_multiUseButton != null) _multiUseButton.onClick.AddListener(() => OnMultiUseButtonPressed.Invoke());
    }

    public void SetDisk(Sprite sprite) { _diskImage.sprite = sprite; }

    public bool StartSpinDisk()
    {
        if (_diskSpinCoroutine != null) return false;

        _diskSpinCoroutine = StartCoroutine(SpinDisk());
        return true;
    }

    public void StopSpinDisk()
    {
        if (_diskSpinCoroutine != null)
        {
            StopCoroutine(_diskSpinCoroutine);
            _diskSpinCoroutine = null;
        }

        _diskImage.transform.rotation = Quaternion.identity;
    }

    public void ClearDisk() { StopSpinDisk(); _diskImage.sprite = _emptyDiskSprite; }

    private IEnumerator SpinDisk()
    {
        while (true)
        {
            _diskImage.transform.Rotate(Vector3.forward * -_diskRotationSpeed * Time.deltaTime);

            yield return null;
        }
    }

    public void ToggleIndicatorHolder(bool value) { _offlineIndicatorContent.SetActive(value); }

    public void SetIndicatorText(JukeboxDiskTextType textType)
    {
        _indicatorText.text = _indicatorTexts[(int)textType];
        ToggleIndicatorHolder(true);
    }

    public void ToggleCustomIndicatorImage(bool value) { _customIndicatorImageHolder.SetActive(value); }
}
