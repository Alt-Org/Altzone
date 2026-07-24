using UnityEngine;
using UnityEngine.UI;

public class VerticalGradientEffect : BaseMeshEffect
{
    [SerializeField] private Color _topColor = Color.white;
    [SerializeField] private Color _bottomColor = Color.white;
    [SerializeField, Range(0.01f, 1f)] private float _transitionEnd = 1f;

    public void SetGradient(Color topColor, Color bottomColor)
    {
        _topColor = topColor;
        _bottomColor = bottomColor;

        if (graphic != null)
            graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive() || vertexHelper.currentVertCount == 0)
            return;

        if (vertexHelper.currentVertCount == 4)
        {
            ModifySimpleQuad(vertexHelper);
            return;
        }

        ModifyExistingMesh(vertexHelper);
    }

    private void ModifySimpleQuad(VertexHelper vertexHelper)
    {
        UIVertex bottomLeft = default;
        UIVertex bottomRight = default;
        UIVertex topLeft = default;
        UIVertex topRight = default;
        GetQuadCorners(vertexHelper, ref bottomLeft, ref bottomRight, ref topLeft, ref topRight);

        float minY = bottomLeft.position.y;
        float maxY = topLeft.position.y;
        float height = Mathf.Max(0.0001f, maxY - minY);
        float transitionEnd = Mathf.Clamp(_transitionEnd, 0.01f, 1f);
        float stopY = minY + height * transitionEnd;

        UIVertex stopLeft = LerpVertex(bottomLeft, topLeft, transitionEnd);
        UIVertex stopRight = LerpVertex(bottomRight, topRight, transitionEnd);
        stopLeft.position.y = stopY;
        stopRight.position.y = stopY;

        TintVertex(ref bottomLeft, _bottomColor);
        TintVertex(ref bottomRight, _bottomColor);
        TintVertex(ref stopLeft, _topColor);
        TintVertex(ref stopRight, _topColor);
        TintVertex(ref topLeft, _topColor);
        TintVertex(ref topRight, _topColor);

        vertexHelper.Clear();
        vertexHelper.AddVert(bottomLeft);
        vertexHelper.AddVert(bottomRight);
        vertexHelper.AddVert(stopRight);
        vertexHelper.AddVert(stopLeft);
        vertexHelper.AddVert(topLeft);
        vertexHelper.AddVert(topRight);

        vertexHelper.AddTriangle(0, 1, 2);
        vertexHelper.AddTriangle(2, 3, 0);
        vertexHelper.AddTriangle(3, 2, 5);
        vertexHelper.AddTriangle(5, 4, 3);
    }

    private void ModifyExistingMesh(VertexHelper vertexHelper)
    {
        UIVertex vertex = default;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < vertexHelper.currentVertCount; i++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, i);
            float y = vertex.position.y;
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
        }

        float height = Mathf.Max(0.0001f, maxY - minY);
        float transitionEnd = Mathf.Max(0.01f, _transitionEnd);

        for (int i = 0; i < vertexHelper.currentVertCount; i++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, i);
            float normalizedY = Mathf.Clamp01((vertex.position.y - minY) / height);
            float t = Mathf.Clamp01(normalizedY / transitionEnd);
            vertex.color *= Color.Lerp(_bottomColor, _topColor, t);
            vertexHelper.SetUIVertex(vertex, i);
        }
    }

    private static void GetQuadCorners(VertexHelper vertexHelper, ref UIVertex bottomLeft, ref UIVertex bottomRight, ref UIVertex topLeft, ref UIVertex topRight)
    {
        UIVertex vertex = default;
        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        for (int i = 0; i < vertexHelper.currentVertCount; i++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, i);
            Vector3 position = vertex.position;
            minX = Mathf.Min(minX, position.x);
            maxX = Mathf.Max(maxX, position.x);
            minY = Mathf.Min(minY, position.y);
            maxY = Mathf.Max(maxY, position.y);
        }

        for (int i = 0; i < vertexHelper.currentVertCount; i++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, i);
            Vector3 position = vertex.position;

            if (Mathf.Approximately(position.x, minX) && Mathf.Approximately(position.y, minY))
                bottomLeft = vertex;
            else if (Mathf.Approximately(position.x, maxX) && Mathf.Approximately(position.y, minY))
                bottomRight = vertex;
            else if (Mathf.Approximately(position.x, minX) && Mathf.Approximately(position.y, maxY))
                topLeft = vertex;
            else if (Mathf.Approximately(position.x, maxX) && Mathf.Approximately(position.y, maxY))
                topRight = vertex;
        }
    }

    private static UIVertex LerpVertex(UIVertex bottom, UIVertex top, float t)
    {
        UIVertex vertex = bottom;
        vertex.position = Vector3.Lerp(bottom.position, top.position, t);
        vertex.uv0 = Vector4.Lerp(bottom.uv0, top.uv0, t);
        vertex.uv1 = Vector4.Lerp(bottom.uv1, top.uv1, t);
        vertex.uv2 = Vector4.Lerp(bottom.uv2, top.uv2, t);
        vertex.uv3 = Vector4.Lerp(bottom.uv3, top.uv3, t);
        vertex.normal = Vector3.Lerp(bottom.normal, top.normal, t);
        vertex.tangent = Vector4.Lerp(bottom.tangent, top.tangent, t);
        return vertex;
    }

    private static void TintVertex(ref UIVertex vertex, Color color)
    {
        vertex.color *= color;
    }
}
