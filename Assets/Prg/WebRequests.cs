using System;
using System.Collections;
using System.Text;
using Prg;
using UnityEngine.Networking;
using static Prg.Debug;


/// <summary>
/// Most commonly used WebRequests in AltZone.
/// </summary>
public static class WebRequests
{
    public static IEnumerator Get(string address, string accessToken, Action<UnityWebRequest> callback)
    {
        using(UnityWebRequest request = UnityWebRequest.Get(address))
        {
            if (accessToken != null)
            {
                request.SetRequestHeader("authorization", "Bearer " + accessToken);
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Error retrieving data from " + address + " - " + request.error);
            }

            if (callback != null)
            {
                callback(request);
            }
        }
    }
    public static IEnumerator Post(string address, string body, string accessToken, Action<UnityWebRequest> callback)
    {
        byte[] data = new UTF8Encoding().GetBytes(body);

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(address, body))
        {
            using (UploadHandlerRaw uploadHandler = new UploadHandlerRaw(data))
            {
                using (DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer())
                {

                    request.uploadHandler.Dispose();
                    request.downloadHandler.Dispose();
                    request.uploadHandler = uploadHandler;
                    request.downloadHandler = downloadHandler;
                    request.SetRequestHeader("Content-Type", "application/json");

                    if (accessToken != null)
                    {
                        request.SetRequestHeader("authorization", "Bearer " + accessToken);
                    }

                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning("Error posting data to " + address + " - " + request.error);
                    }

                    if (callback != null)
                    {
                        callback(request);
                    }
                }
            }
        }
    }
    public static IEnumerator Delete(string address, string accessToken, Action<UnityWebRequest> callback)
    {
        using(UnityWebRequest request = UnityWebRequest.Delete(address))
        {
            if (accessToken != null)
            {
                request.SetRequestHeader("authorization", "Bearer " + accessToken);
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Error deleting data from " + address + " - " + request.error);
            }

            if (callback != null)
            {
                callback(request);
            }
        }
    }
}
