using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class videoInit : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        /*videoPlayer.url ="vid";
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.EnableAudioTrack (0, true);
        videoPlayer.Prepare ();*/
        StartCoroutine(WaitFor());
    }

    IEnumerator WaitFor()
    {
        yield return new WaitForSeconds(19);
    }
}