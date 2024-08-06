## New Batch Build system

New Batch Build system is documented here: [StartUnityBuild](https://github.com/jpetays/StartUnityBuild).  
_Note that the repo name can be changed in the future but it should still be under same [github account](https://github.com/jpetays)!_

## Old Batch Build system

_(Old Batch Build system is located in `Prg/Editor/BatchBuild folder`)_  
This document gives overview and list of features for 'automated' UNITY command line batch build system.  
_Batch build works only on Windows but it can be adapted easily to other platforms._

Batch build is made of three main components.
* build scripts (.bat files) and configuration (.env files)
* UNITY command line build implementation (C#)
* build post processing

### Build scripts

These are convenience scripts to start the 'UNITY command line build' with desired parameters.

### UNITY command line build

Command line build reads given parameters and configuration files and configures different UNITY settings (typically .asset files) so that build can be done as requested.

Some settings are applicable to all platforms and then there is platform specific parameters that can be changed or are hard coded to certain values according to project requirements.

**Prg.Editor.BatchBuild._BuildPlayer_** is the method that is invoked when build system is started from command line.

### Build post processing

Build post processing collects data from internal UNITY build data structures and makes them accessible for later analysis.
* Show Build Report in browser
* Show Build Report with unused Assets

**Prg.Editor.BatchBuild._BuildPlayerPostProcessing_** is the method that is invoked when build system is started from command line for post processing.

## Version control system

This system alters files on disk in order to configure UNITY settings for the build!  
All changes are reverted back to original version control state after build is done.

So it is advisable to commit all changes before starting the build or accept the fact that changes made before build are lost.  
All files that will be reverted are hard coded in `unityBatchBuild.bat` script file.  
Currently list contains these files:
* ProjectSettings\ProjectSettings.asset
* Assets\Resources\GameAnalytics\Settings.asset

## Platforms

Main reason for this build system is provide 'one button click' to start UNITY build for given platform with minimal amount of human intervention for our convenience.

If build needs some **_secrets_**, they are by default stored under project root in folder `.\etc\secretKeys`.

### Android

Android platform requires some **_secrets_** to be injected into UNITY settings for the build to succeed.  
Furthermore PlayerSettings.Android.`minifyRelease` is set to `true`.

### WebGL

WebGL does not have anything special but these hardcoded settings:  
PlayerSettings.WebGL.`compressionFormat` = `Brotli`.

### Windows

Windows is used just as a test platform for quickest command line builds. There is no specific settings for it.

## Third party integrations

### Game Analytics

Game Analytics **_secrets_** are injected into Game Analytics settings asset for current platform.

## Resources

- [Unity Editor command line arguments](https://docs.unity3d.com/Manual/EditorCommandLineArguments.html)
