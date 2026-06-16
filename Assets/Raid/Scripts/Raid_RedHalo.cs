using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Raid_RedHalo : MonoBehaviour
{
    private static readonly float[] LayerSizeRatios =
    {
        0.08f,
        0.13f,
        0.2f,
        0.3f,
        0.43f,
        0.58f,
        0.76f,
        1f
    };

    private static readonly float[] LayerAlphas =
    {
        0.85f,
        0.68f,
        0.5f,
        0.35f,
        0.23f,
        0.15f,
        0.09f,
        0.055f
    };

    private static Material solidAlphaMaterial;

    [SerializeField] private Color haloColor = new Color32(0xE6, 0x0A, 0x0A, 0xFF);
    [SerializeField] private Vector2 padding = new Vector2(90f, 90f);
    [SerializeField] private Vector2 shadowOffset = Vector2.zero;

    private RectTransform targetRect;
    private RectTransform haloRoot;
    private RectTransform[] haloRects = new RectTransform[0];
    private Image[] haloImages = new Image[0];
    private Image targetImage;
    private bool visible;

    public static Raid_RedHalo SetVisible(GameObject target, bool isVisible, Vector2 haloPadding, Vector2 haloOffset)
    {
        if (target == null)
        {
            return null;
        }

        Raid_RedHalo halo = target.GetComponent<Raid_RedHalo>();
        if (halo == null)
        {
            if (!isVisible)
            {
                return null;
            }

            halo = target.AddComponent<Raid_RedHalo>();
        }

        halo.padding = haloPadding;
        halo.shadowOffset = haloOffset;
        halo.SetVisible(isVisible);
        return halo;
    }

    public void SetVisible(bool isVisible)
    {
        visible = isVisible;

        if (visible)
        {
            EnsureHalo();
            SyncToTarget();
        }

        if (haloRoot != null)
        {
            haloRoot.gameObject.SetActive(visible);
        }
    }

    private void LateUpdate()
    {
        if (visible)
        {
            SyncToTarget();
        }
    }

    private void OnEnable()
    {
        if (!visible)
        {
            return;
        }

        EnsureHalo();
        SyncToTarget();
        if (haloRoot != null)
        {
            haloRoot.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (haloRoot != null)
        {
            haloRoot.gameObject.SetActive(false);
        }
    }

    private void OnTransformParentChanged()
    {
        if (haloRoot != null)
        {
            Destroy(haloRoot.gameObject);
            haloRoot = null;
            haloRects = new RectTransform[0];
            haloImages = new Image[0];
        }

        if (visible)
        {
            EnsureHalo();
            SyncToTarget();
        }
    }

    private void OnDestroy()
    {
        if (haloRoot != null)
        {
            Destroy(haloRoot.gameObject);
        }
    }

    private void EnsureHalo()
    {
        if (haloRoot != null)
        {
            return;
        }

        targetRect = transform as RectTransform;
        if (targetRect == null)
        {
            return;
        }

        targetImage = GetComponent<Image>();
        Transform haloParent = targetRect.parent != null ? targetRect.parent : targetRect;
        GameObject haloObject = new GameObject($"{gameObject.name}_RedHalo", typeof(RectTransform), typeof(LayoutElement));
        haloObject.transform.SetParent(haloParent, false);

        haloRoot = haloObject.GetComponent<RectTransform>();

        LayoutElement layoutElement = haloObject.GetComponent<LayoutElement>();
        layoutElement.ignoreLayout = true;

        haloObject.transform.SetSiblingIndex(targetRect.GetSiblingIndex());

        haloRects = new RectTransform[LayerSizeRatios.Length];
        haloImages = new Image[LayerSizeRatios.Length];
        for (int i = 0; i < LayerSizeRatios.Length; i++)
        {
            GameObject layerObject = new GameObject($"Shadow_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            layerObject.transform.SetParent(haloRoot, false);

            haloRects[i] = layerObject.GetComponent<RectTransform>();
            haloImages[i] = layerObject.GetComponent<Image>();
            haloImages[i].raycastTarget = false;
            haloImages[i].material = GetSolidAlphaMaterial();
        }
    }

    private void SyncToTarget()
    {
        if (targetRect == null)
        {
            targetRect = transform as RectTransform;
        }

        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetRect == null || haloRoot == null || targetImage == null || targetImage.sprite == null)
        {
            if (haloRoot != null)
            {
                haloRoot.gameObject.SetActive(false);
            }
            return;
        }

        if (haloRoot.parent != targetRect.parent && targetRect.parent != null)
        {
            haloRoot.SetParent(targetRect.parent, false);
        }

        haloRoot.anchorMin = targetRect.anchorMin;
        haloRoot.anchorMax = targetRect.anchorMax;
        haloRoot.pivot = targetRect.pivot;
        haloRoot.anchoredPosition = targetRect.anchoredPosition + shadowOffset;
        haloRoot.sizeDelta = targetRect.sizeDelta;
        haloRoot.localRotation = targetRect.localRotation;
        haloRoot.localScale = targetRect.localScale;

        int targetSiblingIndex = targetRect.GetSiblingIndex();
        if (haloRoot.GetSiblingIndex() > targetSiblingIndex)
        {
            haloRoot.SetSiblingIndex(targetSiblingIndex);
        }

        for (int i = 0; i < haloImages.Length; i++)
        {
            ApplyLayer(i);
        }

        haloRoot.gameObject.SetActive(visible);
    }

    private void ApplyLayer(int layerIndex)
    {
        RectTransform haloRect = haloRects[layerIndex];
        Image haloImage = haloImages[layerIndex];
        if (haloRect == null || haloImage == null)
        {
            return;
        }

        float sizeRatio = LayerSizeRatios[layerIndex];
        haloRect.anchorMin = Vector2.zero;
        haloRect.anchorMax = Vector2.one;
        haloRect.pivot = new Vector2(0.5f, 0.5f);
        haloRect.anchoredPosition = Vector2.zero;
        haloRect.sizeDelta = padding * sizeRatio;
        haloRect.localRotation = Quaternion.identity;
        haloRect.localScale = Vector3.one;

        haloImage.enabled = true;
        haloImage.sprite = targetImage.sprite;
        haloImage.type = targetImage.type;
        haloImage.preserveAspect = targetImage.preserveAspect;
        haloImage.material = GetSolidAlphaMaterial();

        Color layerColor = haloColor;
        layerColor.a *= LayerAlphas[layerIndex];
        haloImage.color = layerColor;
    }

    private static Material GetSolidAlphaMaterial()
    {
        if (solidAlphaMaterial != null)
        {
            return solidAlphaMaterial;
        }

        Shader shader = Shader.Find("Raid/UI/SolidAlpha");
        if (shader == null)
        {
            return null;
        }

        solidAlphaMaterial = new Material(shader)
        {
            name = "Raid Red Halo Solid Alpha",
            hideFlags = HideFlags.HideAndDontSave
        };
        return solidAlphaMaterial;
    }
}
