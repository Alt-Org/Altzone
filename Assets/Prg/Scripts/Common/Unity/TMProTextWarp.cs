using TMPro;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;

#endif
using UnityEngine;

/// <summary> Adapted from TMPro examples and extras provided by Unity </summary>
[ExecuteAlways]
public class TMProWarpText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;

    public AnimationCurve vertexCurve = new AnimationCurve(
        new Keyframe(0, 0, 0, 1, 0, 0), new Keyframe(0.5f, 1.0f), new Keyframe(1, 0, 1, 0, 0, 0));
    public float yCurveScaling = 50f;

    private bool isForceUpdatingMesh;

    private void Reset()
    {
        text = gameObject.GetComponent<TMP_Text>();
    }

    void Awake()
    {
        if (!text) text = gameObject.GetComponent<TMP_Text>();
#if UNITY_EDITOR
        PrefabStage.prefabStageOpened += PrefabStageOpened;
#endif
    }

    private void OnEnable()
    {
        WarpText();
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ReactToTextChanged);
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ReactToTextChanged);
        text.ForceMeshUpdate();
    }

    //private void OnDidApplyAnimationProperties()
    //{
    //    WarpText();
    //}

#if UNITY_EDITOR
    private void OnDestroy()
    {
        PrefabStage.prefabStageOpened -= PrefabStageOpened;
    }

    private void PrefabStageOpened(PrefabStage prefabStage)
    {
        WarpText();
    }
#endif

    private void OnValidate()
    {
        WarpText();
    }

    private void ReactToTextChanged(Object obj)
    {
        TMP_Text tmpText = obj as TMP_Text;
        if (tmpText && text && tmpText == text && !isForceUpdatingMesh)
            WarpText();
    }

    /// <summary> Method to curve text along a Unity animation curve. </summary>
    private void WarpText()
    {
        if (!text) return;
        isForceUpdatingMesh = true;
        vertexCurve.preWrapMode = WrapMode.Clamp;
        vertexCurve.postWrapMode = WrapMode.Clamp;

        Vector3[] vertices;
        Matrix4x4 matrix;

        text.havePropertiesChanged = true; // Need to force the TextMeshPro Object to be updated.
        text.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

        TMP_TextInfo textInfo = text.textInfo;
        if (textInfo == null) return;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) return;

        float boundsMinX = text.bounds.min.x;
        float boundsMaxX = text.bounds.max.x;

        for (int i = 0; i < characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            // Get the index of the mesh used by this character.
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            vertices = textInfo.meshInfo[materialIndex].vertices;

            // Compute the baseline mid point for each character
            Vector3 offsetToMidBaseline = new Vector2(
                (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);

            // Apply offset to adjust our pivot point.
            vertices[vertexIndex + 0] += -offsetToMidBaseline;
            vertices[vertexIndex + 1] += -offsetToMidBaseline;
            vertices[vertexIndex + 2] += -offsetToMidBaseline;
            vertices[vertexIndex + 3] += -offsetToMidBaseline;

            // Compute the angle of rotation for each character based on the animation curve
            // Character's position relative to the bounds of the mesh.
            float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX);
            float x1 = x0 + 0.0001f;
            float y0 = vertexCurve.Evaluate(x0) * yCurveScaling;
            float y1 = vertexCurve.Evaluate(x1) * yCurveScaling;

            Vector3 horizontal = new Vector3(1, 0, 0);
            Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) -
                new Vector3(offsetToMidBaseline.x, y0);

            float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * Mathf.Rad2Deg;
            Vector3 cross = Vector3.Cross(horizontal, tangent);
            float angle = cross.z > 0 ? dot : 360 - dot;

            matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

            vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
            vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
            vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
            vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

            vertices[vertexIndex + 0] += offsetToMidBaseline;
            vertices[vertexIndex + 1] += offsetToMidBaseline;
            vertices[vertexIndex + 2] += offsetToMidBaseline;
            vertices[vertexIndex + 3] += offsetToMidBaseline;

            // Upload the mesh with the revised information
            text.UpdateVertexData();
        }

        isForceUpdatingMesh = false;
    }
}
