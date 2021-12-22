using UnityEngine;

namespace Altzone.Scripts.Window.ScriptableObjects
{
    /// <summary>
    /// Window definition for <c>WindowManager</c>.<br />
    /// It consists of window prefab (what to show) and optional scene  definition (where to show).
    /// </summary>
    /// <remarks>
    /// Pop out windows are removed from window stack permanently after they are hidden so that they can not appear on window bread crumbs list.
    /// </remarks>
    [CreateAssetMenu(menuName = "ALT-Zone/WindowDef", fileName = "window-")]
    public class WindowDef : ScriptableObject
    {
        private const string Tooltip = "Pop out and hide this window before showing any other window";

        [SerializeField] private GameObject _windowPrefab;
        [Tooltip(Tooltip), SerializeField] private bool _isPopOutWindow;
        [SerializeField] private SceneDef _scene;

        public bool HasScene => _scene != null;
        public bool NeedsSceneLoad => _NeedsSceneLoad();
        public bool IsPopOutWindow => _isPopOutWindow;
        public GameObject WindowPrefab => _windowPrefab;
        public string WindowName => _windowPrefab != null ? _windowPrefab.name : string.Empty;
        public string SceneName => _scene != null ? _scene.SceneName : string.Empty;
        public SceneDef Scene => _scene;

        public void SetWindowPrefab(GameObject sceneWindow)
        {
            _windowPrefab = sceneWindow;
        }

        private bool _NeedsSceneLoad()
        {
            return _scene != null && _scene.NeedsSceneLoad();
        }

        public override string ToString()
        {
            var popOut = IsPopOutWindow ? " PopOut" : string.Empty;
            return HasScene ? $"WindowDef: '{WindowName}'{popOut} ({SceneName})" : $"WindowDef: '{WindowName}'{popOut}";
        }
    }
}