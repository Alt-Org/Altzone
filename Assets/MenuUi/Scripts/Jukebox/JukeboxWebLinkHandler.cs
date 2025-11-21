using Altzone.Scripts.ReferenceSheets;
using UnityEngine;
using UnityEngine.UI;

public class JukeboxWebLinkHandler : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI _weblinkLabel;
    [SerializeField] Button _weblinkButton;

    private string _weblinkAddress;

    private void Awake()
    {
        _weblinkButton.onClick.AddListener(() => OpenWeblink());
    }

    public bool InUse() { return string.IsNullOrEmpty(_weblinkAddress); }

    public void Set(ArtistReference artist)
    {
        if (artist == null || string.IsNullOrEmpty(artist.WebsiteAddress))
        {
            if (string.IsNullOrEmpty(_weblinkAddress)) Clear();

            return;
        }

        _weblinkLabel.text = artist.WebsiteName;
        _weblinkAddress = artist.WebsiteAddress;
        gameObject.SetActive(true);
    }

    public void Clear()
    {
        _weblinkLabel.text = "";
        _weblinkAddress = "";
        gameObject.SetActive(false);
    }

    private void OpenWeblink() { Application.OpenURL(_weblinkAddress); }
}
