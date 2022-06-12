using UnityEngine;

namespace Prg.Scripts.Common.Unity.Window
{
    /// <summary>
    /// Allow this component on selected platform(s).<br />
    /// - that is disables this on other but selected platform(s).
    /// </summary>
    public class PlatformSelector : MonoBehaviour
    {
        private const string EditorTooltip = "Applicable when running in UNITY Editor";
        private const string DevelopmentBuildTooltip = "Applicable when 'Development Build' in Build Settings is set";
        private const string ProductionTooltip = "Applicable for given build platforms below";

        [SerializeField, Tooltip(EditorTooltip)] private bool _isAllowInEditor = true;
        [SerializeField, Tooltip(DevelopmentBuildTooltip)] private bool _isAllowDevelopmentBuild = true;
        [SerializeField, Tooltip(ProductionTooltip)] private bool _isAllowInProductionPlatforms;
        [SerializeField] private RuntimePlatform[] _platforms;
        [SerializeField] private GameObject[] _gameObjectsToWatch;

        private void OnEnable()
        {
            Debug.Log(
                $"{name} on {Application.platform} is: editor {AppPlatform.IsEditor}, dev build {AppPlatform.IsDevelopmentBuild}, platform {IsValidPlatform()}");
            if (AppPlatform.IsEditor)
            {
                HandleComponents(_isAllowInEditor);
                return;
            }
            if (AppPlatform.IsDevelopmentBuild)
            {
                HandleComponents(_isAllowDevelopmentBuild);
                return;
            }
            if (IsValidPlatform())
            {
                HandleComponents(_isAllowInProductionPlatforms);
                return;
            }
            HandleComponents(false);
        }

        private void HandleComponents(bool state)
        {
            Debug.Log($"{name} on {Application.platform}");
            foreach (var gameObjectToDisable in _gameObjectsToWatch)
            {
                Debug.Log($"set {gameObjectToDisable.name} {(state ? "activate" : "disabled")}");
                gameObjectToDisable.SetActive(state);
            }
        }

        private bool IsValidPlatform()
        {
            var platform = Application.platform;
            foreach (var runtimePlatform in _platforms)
            {
                if (platform.Equals(runtimePlatform))
                {
                    return true;
                }
            }
            return false;
        }
    }
}