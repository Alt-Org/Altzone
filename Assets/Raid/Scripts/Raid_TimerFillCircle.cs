using UnityEngine;
using UnityEngine.UI;

public class Raid_TimerFillCircle : MaskableGraphic
{
    [SerializeField, Range(0f, 1f)]
    private float progress;

    [SerializeField, Range(12, 128)]
    private int segments = 64;

    [SerializeField, Range(0.01f, 0.6f)]
    private float circumferenceThickness = 0.06f;

    [SerializeField, Range(0f, 0.25f)]
    private float circumferenceInset = 0.08f;

    [SerializeField]
    private Vector2 circumferenceCenterOffset = new Vector2(-0.03f, 0.025f);

    public float Progress
    {
        get => progress;
        set
        {
            float clampedValue = Mathf.Clamp01(value);
            if (Mathf.Approximately(progress, clampedValue))
            {
                return;
            }

            progress = clampedValue;
            SetVerticesDirty();
        }
    }

    public int Segments
    {
        get => segments;
        set
        {
            int clampedValue = Mathf.Clamp(value, 12, 128);
            if (segments == clampedValue)
            {
                return;
            }

            segments = clampedValue;
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (progress <= 0f)
        {
            return;
        }

        Rect rect = rectTransform.rect;
        float size = Mathf.Min(rect.width, rect.height);
        Vector2 center = rect.center + circumferenceCenterOffset * size;
        float inset = Mathf.Clamp(circumferenceInset, 0f, 0.25f);
        float outerRadius = size * (0.5f - inset);
        float thicknessRatio = Mathf.Clamp(circumferenceThickness, 0.01f, 0.6f);
        float thickness = Mathf.Clamp(size * thicknessRatio, 1f, outerRadius);
        float innerRadius = outerRadius - thickness;
        int visibleSegments = Mathf.Max(1, Mathf.CeilToInt(segments * progress));
        float angleRange = Mathf.PI * 2f * progress;
        float startAngle = Mathf.PI * 0.5f;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        for (int i = 0; i <= visibleSegments; i++)
        {
            float angle = startAngle - angleRange * i / visibleSegments;
            vertex.position = new Vector3(
                center.x + Mathf.Cos(angle) * outerRadius,
                center.y + Mathf.Sin(angle) * outerRadius);
            vh.AddVert(vertex);

            vertex.position = new Vector3(
                center.x + Mathf.Cos(angle) * innerRadius,
                center.y + Mathf.Sin(angle) * innerRadius);
            vh.AddVert(vertex);
        }

        for (int i = 0; i < visibleSegments; i++)
        {
            int currentOuter = i * 2;
            int currentInner = currentOuter + 1;
            int nextOuter = currentOuter + 2;
            int nextInner = currentOuter + 3;

            vh.AddTriangle(currentOuter, nextOuter, nextInner);
            vh.AddTriangle(currentOuter, nextInner, currentInner);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        progress = Mathf.Clamp01(progress);
        segments = Mathf.Clamp(segments, 12, 128);
        circumferenceThickness = Mathf.Clamp(circumferenceThickness, 0.01f, 0.6f);
        circumferenceInset = Mathf.Clamp(circumferenceInset, 0f, 0.25f);
        circumferenceCenterOffset.x = Mathf.Clamp(circumferenceCenterOffset.x, -0.25f, 0.25f);
        circumferenceCenterOffset.y = Mathf.Clamp(circumferenceCenterOffset.y, -0.25f, 0.25f);
        SetVerticesDirty();
    }
#endif
}
