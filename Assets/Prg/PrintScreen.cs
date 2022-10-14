using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrintScreen : MonoBehaviour
{
    private const int SuperSize = 1;

    [Header("Settings")] public string _imageName = "screenshot";

    [Header("Live Data")] public bool _capturing;
    public string _imageFolder;
    public int _imageIndex;

    private void Awake()
    {
        var allowedPlatform = Application.platform == RuntimePlatform.WindowsEditor
                              || Application.platform == RuntimePlatform.WindowsPlayer;
        if (!allowedPlatform)
        {
            enabled = false;
            return;
        }

        // Keep numbering from largest found index.
        _imageIndex = 0;
        _imageFolder = Application.persistentDataPath;
        var oldFiles = Directory.GetFiles(_imageFolder, $"{_imageName.Replace("-", "_")}-???-*.png");
        var today = DateTime.Now.Day;
        foreach (var oldFile in oldFiles)
            if (File.GetCreationTime(oldFile).Day != today)
            {
                File.Delete(oldFile);
            }
            else
            {
                var tokens = Path.GetFileName(oldFile).Split('-');
                if (tokens.Length == 3 && int.TryParse(tokens[1], out var fileIndex))
                {
                    if (fileIndex > _imageIndex)
                    {
                        _imageIndex = fileIndex;
                    }
                }
            }
    }

    private void OnGUI()
    {
        if (!_capturing && Event.current.type == EventType.KeyUp)
        {
            if (Event.current.keyCode == KeyCode.Print
                || Event.current.keyCode == KeyCode.SysReq // This is actually Print Screen!
                || Event.current.keyCode == KeyCode.F6 // Works for Mac
            )
            {
                _capturing = true;
            }
        }
    }

    private void LateUpdate()
    {
        if (_capturing)
        {
            _capturing = false;
            var sceneName = SceneManager.GetActiveScene().name;
            string filename;
            for (;;)
            {
                _imageIndex += 1;
                filename = $"{_imageFolder}{Path.AltDirectorySeparatorChar}{_imageName}-{_imageIndex:000}-{sceneName}.png";
                if (!File.Exists(filename))
                {
                    break;
                }
            }
            ScreenCapture.CaptureScreenshot(filename, SuperSize);
            var sep2 = Path.DirectorySeparatorChar.ToString();
            var sep1 = Path.AltDirectorySeparatorChar.ToString();
            UnityEngine.Debug.Log($"Capture screenshot: {filename.Replace(sep1, sep2)}");
        }
    }
}