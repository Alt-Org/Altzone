using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Raid_RedHalo : MonoBehaviour
{
    private const string DefaultHaloPrefabPath = "Prefabs/RaidRedHalo";

    private static GameObject defaultHaloPrefab;

    private class HaloLayer
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 PaddingRatio;
    }

    [SerializeField] private GameObject haloPrefab;
    [SerializeField] private Vector2 padding = new Vector2(90f, 90f);
    [SerializeField] private Vector2 shadowOffset = Vector2.zero;

    private RectTransform targetRect;
    private RectTransform haloRoot;
    private readonly List<HaloLayer> haloLayers = new List<HaloLayer>();
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
            haloLayers.Clear();
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
        if (targetImage == null)
        {
            return;
        }

        Transform haloParent = targetRect.parent != null ? targetRect.parent : targetRect;
        GameObject prefab = GetHaloPrefab();
        if (prefab == null)
        {
            return;
        }

        GameObject haloObject = Instantiate(prefab, haloParent, false);
        haloObject.name = $"{gameObject.name}_RedHalo";

        haloObject.transform.SetSiblingIndex(targetRect.GetSiblingIndex());
        haloRoot = haloObject.GetComponent<RectTransform>();
        if (haloRoot == null)
        {
            Destroy(haloObject);
            return;
        }

        BuildLayerList();
    }

    private void BuildLayerList()
    {
        haloLayers.Clear();

        Image[] layerImages = haloRoot.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < layerImages.Length; i++)
        {
            Image layerImage = layerImages[i];

            haloLayers.Add(new HaloLayer
            {
                Rect = layerImage.rectTransform,
                Image = layerImage,
                PaddingRatio = layerImage.rectTransform.sizeDelta
            });
        }
    }

    private void SyncToTarget()
    {
        if (targetRect == null)
        {
            targetRect = transform as RectTransform;
        }

        if (targetRect == null || haloRoot == null || targetImage.sprite == null)
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

        for (int i = 0; i < haloLayers.Count; i++)
        {
            ApplyLayer(haloLayers[i]);
        }

        haloRoot.gameObject.SetActive(visible);
    }

    private void ApplyLayer(HaloLayer layer)
    {
        if (layer.Rect == null || layer.Image == null)
        {
            return;
        }

        layer.Rect.sizeDelta = Vector2.Scale(padding, layer.PaddingRatio);

        layer.Image.sprite = targetImage.sprite;
        layer.Image.type = targetImage.type;
        layer.Image.preserveAspect = targetImage.preserveAspect;
    }

    private GameObject GetHaloPrefab()
    {
        if (haloPrefab != null)
        {
            return haloPrefab;
        }

        if (defaultHaloPrefab == null)
        {
            defaultHaloPrefab = Resources.Load<GameObject>(DefaultHaloPrefabPath);
        }

        return defaultHaloPrefab;
    }
}
