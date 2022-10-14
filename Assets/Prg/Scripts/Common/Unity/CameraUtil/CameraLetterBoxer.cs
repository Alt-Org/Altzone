using UnityEngine;

namespace Prg.Scripts.Common.Unity.CameraUtil
{
    /// <summary>
    /// Preserve your 2D game's aspect ratio adding letterbox / pillarbox (bars) if required.
    /// See: https://github.com/rabidgremlin/LetterBoxer
    /// </summary>
    /// <remarks>
    /// This could be used instead of <c>CameraAspectRatio</c>.
    /// They are implemented differently and I do not know which one is better or more suitable for the task.
    /// </remarks>
    public class CameraLetterBoxer : MonoBehaviour
    {
        // TODO: BEFORE MAKING CHANGES test that this work and then fix code style.
        // NOTE: It seems that this uses extra camera for background and CameraAspectRatio uses OnPreCull()
        
        public enum ReferenceMode
        {
            DesignedAspectRatio,
            OrginalResolution
        };

        public Color matteColor = new Color(0, 0, 0, 1);
        public ReferenceMode referenceMode;
        public float x = 9;
        public float y = 16;
        public float width = 960;
        public float height = 540;
        public bool onAwake = true;
        public bool onUpdate = true;

        private Camera cam;
        private Camera letterBoxerCamera;

        public void Awake()
        {
            // store reference to the camera
            cam = GetComponent<Camera>();

            // add the letterboxing camera
            AddLetterBoxingCamera();

            // perform sizing if onAwake is set
            if (onAwake)
            {
                PerformSizing();
            }
        }

        public void Update()
        {
            // perform sizing if onUpdate is set
            if (onUpdate)
            {
                PerformSizing();
            }
        }

        private void OnValidate()
        {
            x = Mathf.Max(1, x);
            y = Mathf.Max(1, y);
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
        }

        private void AddLetterBoxingCamera()
        {
            // check that we don't have a camera already at -100 (lowest depth) which will cause issues
            Camera[] allCameras = FindObjectsOfType<Camera>();
            foreach (Camera camera in allCameras)
            {
                if (camera.depth == -100)
                {
                    Debug.LogError("Found " + camera.name + " with a depth of -100. Will cause letter boxing issues. Please increase it's depth.");
                }
            }

            // create a camera to render bcakground used for matte bars
            letterBoxerCamera = new GameObject().AddComponent<Camera>();
            letterBoxerCamera.backgroundColor = matteColor;
            letterBoxerCamera.cullingMask = 0;
            letterBoxerCamera.depth = -100;
            letterBoxerCamera.farClipPlane = 1;
            letterBoxerCamera.useOcclusionCulling = false;
            letterBoxerCamera.allowHDR = false;
            letterBoxerCamera.allowMSAA = false;
            letterBoxerCamera.clearFlags = CameraClearFlags.Color;
            letterBoxerCamera.name = "Letter Boxer Camera";
        }

        // based on logic here from http://gamedesigntheory.blogspot.com/2010/09/controlling-aspect-ratio-in-unity.html
        private void PerformSizing()
        {
            // calc based on aspect ratio
            float targetRatio = x / y;

            // recalc if using resolution as reference
            if (referenceMode == CameraLetterBoxer.ReferenceMode.OrginalResolution)
            {
                targetRatio = width / height;
            }

            // determine the game window's current aspect ratio
            float windowaspect = (float)Screen.width / (float)Screen.height;

            // current viewport height should be scaled by this amount
            float scaleheight = windowaspect / targetRatio;

            // if scaled height is less than current height, add letterbox
            if (scaleheight < 1.0f)
            {
                Rect rect = cam.rect;

                rect.width = 1.0f;
                rect.height = scaleheight;
                rect.x = 0;
                rect.y = (1.0f - scaleheight) / 2.0f;

                cam.rect = rect;
            }
            else // add pillarbox
            {
                float scalewidth = 1.0f / scaleheight;

                Rect rect = cam.rect;

                rect.width = scalewidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scalewidth) / 2.0f;
                rect.y = 0;

                cam.rect = rect;
            }
        }
    }
}