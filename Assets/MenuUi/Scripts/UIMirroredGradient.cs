using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Mirrored Gradient")]
[RequireComponent(typeof(Image))]
public class UIMirroredGradient : BaseMeshEffect
{
    [Tooltip("Color at the top and bottom edges")]
    public Color colorEdge = new Color(0.71f, 0.76f, 1.0f); // #B6C3FF

    [Tooltip("Color in the center")]
    public Color colorCenter = new Color(0.89f, 0.91f, 1.0f); // #E3E8FF

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0) return;

        // Ensure the image type is "Simple" (4 vertices); otherwise, skip modification
        if (vh.currentVertCount == 4)
        {
            UIVertex v0 = new UIVertex(); vh.PopulateUIVertex(ref v0, 0); // Bottom-left
            UIVertex v1 = new UIVertex(); vh.PopulateUIVertex(ref v1, 1); // Top-left
            UIVertex v2 = new UIVertex(); vh.PopulateUIVertex(ref v2, 2); // Top-right
            UIVertex v3 = new UIVertex(); vh.PopulateUIVertex(ref v3, 3); // Bottom-right

            // Create new vertices exactly in the middle of the panel
            UIVertex vMidLeft = new UIVertex();
            vMidLeft.position = Vector3.Lerp(v0.position, v1.position, 0.5f);
            vMidLeft.color = colorCenter * v0.color;
            vMidLeft.uv0 = Vector2.Lerp(v0.uv0, v1.uv0, 0.5f);

            UIVertex vMidRight = new UIVertex();
            vMidRight.position = Vector3.Lerp(v3.position, v2.position, 0.5f);
            vMidRight.color = colorCenter * v3.color;
            vMidRight.uv0 = Vector2.Lerp(v3.uv0, v2.uv0, 0.5f);

            // Apply the edge color to the original corner vertices
            v0.color = colorEdge * v0.color;
            v1.color = colorEdge * v1.color;
            v2.color = colorEdge * v2.color;
            v3.color = colorEdge * v3.color;

            vh.Clear();

            // Add all 6 vertices back to the vertex helper
            vh.AddVert(v0);       // 0: Bottom-left
            vh.AddVert(vMidLeft); // 1: Middle-left
            vh.AddVert(v1);       // 2: Top-left
            vh.AddVert(v3);       // 3: Bottom-right
            vh.AddVert(vMidRight);// 4: Middle-right
            vh.AddVert(v2);       // 5: Top-right

            // Draw the bottom half (two triangles)
            vh.AddTriangle(0, 1, 4);
            vh.AddTriangle(4, 3, 0);

            // Draw the top half (two triangles)
            vh.AddTriangle(1, 2, 5);
            vh.AddTriangle(5, 4, 1);
        }
    }
}