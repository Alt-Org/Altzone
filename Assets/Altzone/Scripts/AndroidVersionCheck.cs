#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using Google.Play.AppUpdate;
using Google.Play.Common;
using UnityEngine;

public class AndroidVersionCheck : MonoBehaviour
{
    public static IEnumerator VersionCheck(Action<bool> callback)
    {
        AppUpdateManager appUpdateManager = new AppUpdateManager();

        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();

        // Wait until the asynchronous operation completes.
        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();
            // Check AppUpdateInfo's UpdateAvailability, UpdatePriority,
            // IsUpdateTypeAllowed(), ... and decide whether to ask the user
            // to start an in-app update.
            Debug.LogWarning("Test: " + appUpdateInfoResult.UpdateAvailability);
            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
                var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
                var startUpdateRequest = appUpdateManager.StartUpdate(
              // The result returned by PlayAsyncOperation.GetResult().
              appUpdateInfoResult,
              // The AppUpdateOptions created defining the requested in-app update
              // and its parameters.
              appUpdateOptions);
                yield return startUpdateRequest;
            }
            else { callback(true); }
        }
        else
        {
            Debug.LogError("Update Data Fetch Failed. Attempting to use current version.");
            callback(true);
        }
    }
}
#endif
