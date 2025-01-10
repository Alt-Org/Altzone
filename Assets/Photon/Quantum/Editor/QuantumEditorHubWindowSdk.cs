namespace Quantum.Editor {
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using UnityEditor;
  using UnityEditor.PackageManager.Requests;
  using UnityEngine;

  internal partial class QuantumEditorHubWindow {
    static partial void CreateWindowUser(ref QuantumEditorHubWindow window) {
      window = GetWindow<QuantumEditorHubWindowSdk>(true, "Photon Quantum Hub", true);
    }

    static partial void CheckPopupConditionUser(ref bool shouldPopup, ref int page) {
      // Installation requires popup
      if (HubUtils.AreImportantUserFilesInstalled == false) {
        shouldPopup = true;
        page = 0;
        return;
      }

      // Layouts requires popup
      for (int i = 0; i < Pages.Count; i++) {
        if (Pages[i].IsPopupRequired) {
          shouldPopup = true;
          page = i;
          break;
        }
      }
    }

    /// <summary>
    /// This methods installs all user files and generates the workspace files.
    /// </summary>
    public static void InstallAllUserFiles() {
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<PhotonServerSettings>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumEditorSettings>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumDeterministicSessionConfigAsset>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumGameGizmosSettingsScriptableObject>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumDefaultConfigs>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumDotnetProjectSettings>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumDotnetBuildSettings>();
      QuantumGlobalScriptableObjectUtils.EnsureAssetExists<QuantumUnityDB>();
      QuantumEditorUserScriptGeneration.GenerateWorkspaceFiles();
      QuantumEditorUserScriptGeneration.GenerateUserFiles();

      if (Quantum.Input.MaxCount == 0) {
        EditorApplication.ExecuteMenuItem("Tools/Quantum/CodeGen/Run Qtn CodeGen");
      }

      if (SceneView.lastActiveSceneView != null) {
        AssetDatabaseExt.UpdateScriptingDefineSymbol("QUANTUM_XY", SceneView.lastActiveSceneView.in2DMode);
      }

      // Add qtn extension to the VS project generation
      if (EditorSettings.projectGenerationUserExtensions.Contains("qtn") == false) {
        var userExtensions = EditorSettings.projectGenerationUserExtensions;
        ArrayUtils.Add(ref userExtensions, "qtn");
        EditorSettings.projectGenerationUserExtensions = userExtensions;
      }

      if (AssetDatabase.FindAssets("t:Scene", new[] { QuantumEditorUserScriptGeneration.FolderPath }).Length == 0) {
        // Create Quantum game scene
        Directory.CreateDirectory($"{QuantumEditorUserScriptGeneration.FolderPath}/Scenes");
        QuantumEditorMenuCreateScene.CreateNewQuantumScene(
          $"{QuantumEditorUserScriptGeneration.FolderPath}/Scenes/QuantumGameScene.unity",
          $"{QuantumEditorSettings.Global.DefaultNewAssetsLocation}/QuantumMap.asset",
          saveScene: true,
          addToBuildSettings: true,
          createSceneInfoAsset: true);

        if (Application.isBatchMode == false) {
          // Don't call the gui exception when coming from CI
          GUIUtility.ExitGUI();
        }
      }
    }
  }

  internal partial class QuantumEditorHubWidgetTypeDrawer {
    static partial void RegisterTypesUser(List<string> types) {
      types.Add(QuantumEditorHubWindowSdk.CustomWidgetTypes.SdkInstallationBox);
      types.Add(QuantumEditorHubWindowSdk.CustomWidgetTypes.CreateSimpleConnectionScene);
      types.Add(QuantumEditorHubWindowSdk.CustomWidgetTypes.PingPhotonServerSettings);
      types.Add(QuantumEditorHubWindowSdk.CustomWidgetTypes.ClearQuantumPlayerPrefs);
    }
  }

  internal partial class QuantumEditorHubConditionDrawer {
    static partial void RegisterTypesUser(List<string> types) {
      types.Add(QuantumEditorHubWindowSdk.CustomConditions.AppIdCreated);
      types.Add(QuantumEditorHubWindowSdk.CustomConditions.SdkInstalled);
    }
  }

  internal class QuantumEditorHubWindowSdk : QuantumEditorHubWindow {
    static AddRequest MppmAddRequest;

    internal static class CustomWidgetTypes {
      internal const string SdkInstallationBox = "SdkInstallationBox";
      internal const string CreateSimpleConnectionScene = "CreateSimpleConnectionScene";
      internal const string PingPhotonServerSettings = "PingPhotonServerSettings";
      internal const string ClearQuantumPlayerPrefs = "ClearQuantumPlayerPrefs";
    }

    internal static class CustomConditions {
      internal const string AppIdCreated = "AppIdCreated";
      internal const string SdkInstalled = "SdkInstalled";
    }

    public override string AppId {
      get {
        try { 
        var photonSettings = PhotonServerSettings.Global;
        return photonSettings.AppSettings.AppIdQuantum;
        } catch {
          return string.Empty;
        }
      }
      set {
        var photonSettings = PhotonServerSettings.Global;
        photonSettings.AppSettings.AppIdQuantum = value;
        EditorUtility.SetDirty(photonSettings);
        AssetDatabase.SaveAssets();
      }
    }

    public override GUIStyle GetBoxStyle => HubSkin.GetStyle("SteelBox");
    public override GUIStyle GetButtonPaneStyle => HubSkin.GetStyle("ButtonPane");

    static bool _statusInstallationComplete;
    public static bool _statusAppIdSetup;

    protected override bool CustomConditionCheck(QuantumEditorHubCondition condition) {
      if (condition.Value == CustomConditions.AppIdCreated) {
        return _statusAppIdSetup;
      } else if (condition.Value == CustomConditions.SdkInstalled) {
        return _statusInstallationComplete;
      }
      
      return false;
    }

    protected override void CustomDrawWidget(QuantumEditorHubPage page, QuantumEditorHubWidget widget) {
      if (widget.WidgetMode.Value == CustomWidgetTypes.CreateSimpleConnectionScene) {

        DrawButtonAction(widget.Icon, widget.Text, widget.Subtext,
          statusIcon: widget.GetStatusIcon(this),
          callback: () => {
            QuantumEditorMenuCreateScene.CreateSimpleConnectionScene(widget.Scene);
            GUIUtility.ExitGUI();
          });

      } else if (widget.WidgetMode.Value == CustomWidgetTypes.PingPhotonServerSettings) {

        DrawButtonAction(widget.Icon, widget.Text, widget.Subtext,
          statusIcon: widget.GetStatusIcon(this),
          callback: () => {
            EditorGUIUtility.PingObject(PhotonServerSettings.Global); Selection.activeObject = PhotonServerSettings.Global;
          });

      } else if (widget.WidgetMode.Value == CustomWidgetTypes.ClearQuantumPlayerPrefs) {

        DrawButtonAction(widget.Icon, widget.Text, widget.Subtext,
          statusIcon: widget.GetStatusIcon(this),
          callback: () => {
            ClearAllPlayerPrefs();
          });

      } else if (widget.WidgetMode.Value == CustomWidgetTypes.SdkInstallationBox) {

        DrawInstallationBox(widget);
      }
    }

    protected override void OnGuiHeartbeat() {
      _statusInstallationComplete = HubUtils.AreImportantUserFilesInstalled;
      _statusAppIdSetup = HubUtils.IsValidGuid(AppId);
    }

    void ClearAllPlayerPrefs() {
      // Hub
      foreach (var page in Pages) {
        page.DeleteAllPlayerPrefKeys();
      }

      PlayerPrefs.DeleteKey(CurrentPagePlayerPrefsKey);
      PlayerPrefs.DeleteKey(ScrollRectPlayerPrefsKey);

      // Menu
      ClearQuantumMenuPlayerPrefs();

      // Quantum
      PlayerPrefs.DeleteKey(PhotonServerSettings.Global.BestRegionSummaryKey);
      PlayerPrefs.DeleteKey("Quantum.ReconnectInformation");
    }

    // TODO: call after importing menu
    public static void ClearQuantumMenuPlayerPrefs() {
      PlayerPrefs.DeleteKey("Photon.Menu.Username");
      PlayerPrefs.DeleteKey("Photon.Menu.Region");
      PlayerPrefs.DeleteKey("Photon.Menu.AppVersion");
      PlayerPrefs.DeleteKey("Photon.Menu.MaxPlayerCount");
      PlayerPrefs.DeleteKey("Photon.Menu.Scene");
      PlayerPrefs.DeleteKey("Photon.Menu.SceneName");
      PlayerPrefs.DeleteKey("Photon.Menu.Framerate");
      PlayerPrefs.DeleteKey("Photon.Menu.Fullscreen");
      PlayerPrefs.DeleteKey("Photon.Menu.Resolution");
      PlayerPrefs.DeleteKey("Photon.Menu.VSync");
      PlayerPrefs.DeleteKey("Photon.Menu.QualityLevel");
    }

    public void DrawInstallationBox(QuantumEditorHubWidget widget) {
      using (new EditorGUILayout.VerticalScope(GetBoxStyle)) {
        DrawButtonAction(widget.Icon, widget.Text, widget.Subtext,
          statusIcon: GetStatusIcon(_statusInstallationComplete),
          callback: () => {
            InstallAllUserFiles();
          });


        QuantumGlobalScriptableObjectUtils.CreateFindDefaultAssetPathCache();
        try {
          DrawGlobalObjectStatus<PhotonServerSettings>();
          DrawGlobalObjectStatus<QuantumDeterministicSessionConfigAsset>();
          DrawGlobalObjectStatus<QuantumUnityDB>();
          DrawGlobalObjectStatus<QuantumEditorSettings>();
          DrawGlobalObjectStatus<QuantumGameGizmosSettingsScriptableObject>();
          DrawGlobalObjectStatus<QuantumDefaultConfigs>();
          DrawGlobalObjectStatus<QuantumDotnetBuildSettings>();
          DrawGlobalObjectStatus<QuantumDotnetProjectSettings>();
        } finally {
          QuantumGlobalScriptableObjectUtils.ClearFindDefaultAssetPathCache();
        }

        using (new EditorGUILayout.HorizontalScope()) {
          if (GUILayout.Button(QuantumEditorUserScriptGeneration.FolderPath.Replace("Assets/", "") + " User Workspace", HubSkin.label)) {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(QuantumEditorUserScriptGeneration.PingWorkspaceFile, typeof(UnityEngine.Object)));
          }
          GUILayout.Label(GetStatusIcon(QuantumEditorUserScriptGeneration.WorkspaceFilesExist), GUILayout.Width(StatusIconWidthDefault.x), GUILayout.Height(StatusIconWidthDefault.y));
        }

        using (new EditorGUILayout.HorizontalScope()) {
          if (GUILayout.Button(QuantumEditorUserScriptGeneration.FolderPath.Replace("Assets/", "") + " Partial Classes (*.cs.User)", HubSkin.label)) {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(QuantumEditorUserScriptGeneration.PingUserFile, typeof(UnityEngine.Object)));
          }
          GUILayout.Label(GetStatusIcon(QuantumEditorUserScriptGeneration.UserFilesExist), GUILayout.Width(StatusIconWidthDefault.x), GUILayout.Height(StatusIconWidthDefault.y));
        }

        using (new EditorGUILayout.HorizontalScope()) {
          GUILayout.Label("QuantumUser Scenes");
          var foundAnySceneInUserFolder = Directory.Exists(QuantumEditorUserScriptGeneration.FolderPath) && AssetDatabase.FindAssets("t:Scene", new[] { QuantumEditorUserScriptGeneration.FolderPath }).Length > 0;
          GUILayout.Label(GetStatusIcon(foundAnySceneInUserFolder), GUILayout.Width(StatusIconWidthDefault.x), GUILayout.Height(StatusIconWidthDefault.y));
        }

        using (new EditorGUILayout.HorizontalScope()) {
          GUILayout.Label("Quantum Qtn CodeGen");
          GUILayout.Label(GetStatusIcon(Quantum.Input.MaxCount > 0), GUILayout.Width(StatusIconWidthDefault.x), GUILayout.Height(StatusIconWidthDefault.y));
        }

        using (new EditorGUILayout.HorizontalScope()) {
          GUILayout.Label("EditorSettings.ProjectGenerationUserExtensions Include Qtn Files");
          GUILayout.Label(GetStatusIcon(EditorSettings.projectGenerationUserExtensions.Contains("qtn")), GUILayout.Width(StatusIconWidthDefault.x), GUILayout.Height(StatusIconWidthDefault.y));
        }

        if (SceneView.lastActiveSceneView != null) {
          using (new EditorGUILayout.HorizontalScope()) {
            var isQuantumXYEnabled =
#if QUANTUM_XY
              true;
#else
              false;
#endif
            var isModeCorrectlySet =
              (SceneView.lastActiveSceneView.in2DMode == true && isQuantumXYEnabled == true) ||
              (SceneView.lastActiveSceneView.in2DMode == false && isQuantumXYEnabled == false);
            GUILayout.Label("Toggle Quantum 2D/3D Mode (QUANTUM_XY)");
            GUILayout.Label(GetStatusIcon(isModeCorrectlySet), GUILayout.Width(StatusIconWidthDefault.x), GUILayout.Height(StatusIconWidthDefault.y));
          }
        }
      }
    }
  }
}
