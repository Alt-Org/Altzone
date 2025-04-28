using System;
using System.Collections;
using System.Collections.Generic;
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
                Debug.LogWarning("Error retrieving data from " + address + " - " + request.error + ": " + request.downloadHandler.text);
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
        if (string.IsNullOrEmpty(body)) body = ".";

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
                        Debug.LogWarning("Error posting data to " + address + " - " + request.error + ": "+ request.downloadHandler.text);
                    }

                    if (callback != null)
                    {
                        callback(request);
                    }
                }
            }
        }
    }

    public static IEnumerator Post(string address, List<IMultipartFormSection> formData, string accessToken, string secretKey, string id, Action<UnityWebRequest> callback)
    {
        byte[] boundary = UnityWebRequest.GenerateBoundary();
        byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);

        using (UnityWebRequest request = UnityWebRequest.Post(address, formData))
        {
            using (UploadHandlerRaw uploadHandler = new UploadHandlerRaw(formSections))
            {
                using (DownloadHandlerBuffer downloadHandler = new DownloadHandlerBuffer())
                {
                    uploadHandler.contentType = "multipart/form-data; boundary=" + Encoding.UTF8.GetString(boundary, 0, boundary.Length);
                    request.uploadHandler.Dispose();
                    request.downloadHandler.Dispose();
                    request.uploadHandler = uploadHandler;
                    request.downloadHandler = downloadHandler;
                    //request.SetRequestHeader("Content-Type", "multipart/form-data boundary=" + Encoding.UTF8.GetString(boundary, 0, boundary.Length));

                    if (accessToken != null)
                    {
                        request.SetRequestHeader("authorization", "Bearer " + accessToken);
                    }
                    if (secretKey != null)
                    {
                        request.SetRequestHeader("Secret", secretKey);
                    }
                    if (id != null)
                    {
                        request.SetRequestHeader("Battle-Id", id);
                    }

                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning("Error posting data to " + address + " - " + request.error + ": " + request.downloadHandler.text);
                    }

                    if (callback != null)
                    {
                        callback(request);
                    }
                }
            }
        }
    }

    public static IEnumerator Put(string address, string body, string accessToken, Action<UnityWebRequest> callback)
    {
        byte[] data = new UTF8Encoding().GetBytes(body);

        using (UnityWebRequest request = UnityWebRequest.Put(address, body))
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
                        Debug.LogWarning("Error putting data to " + address + " - " + request.error + ": " + request.downloadHandler.text);
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
                Debug.LogWarning("Error deleting data from " + address + " - " + request.error + ": " + request.downloadHandler.text);
            }

            if (callback != null)
            {
                callback(request);
            }
        }
    }
}
