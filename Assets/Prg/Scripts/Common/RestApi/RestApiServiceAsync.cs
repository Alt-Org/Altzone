using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prg.Scripts.Common.RestApi
{
    /// <summary>
    /// Async helper for REST API requests.
    /// </summary>
    public static class RestApiServiceAsync
    {
        public class Response
        {
            private const string OkMessage = "OK";

            public readonly bool Success;
            public readonly string Message;
            public readonly string Payload;

            public Response(string payload)
            {
                Success = true;
                Message = OkMessage;
                Payload = payload;
            }

            public Response(string message, string payload)
            {
                Success = false;
                Message = message;
                Payload = payload;
            }

            public override string ToString()
            {
                return $"{(Success ? "OK" : $"ERROR:{Message}")}: Response: {Payload}";
            }
        }

        public class Headers
        {
            public readonly List<Tuple<string, string>> HeaderList;

            public Headers(string name, string value)
            {
                HeaderList = new List<Tuple<string, string>>() { new(name, value) };
            }

            public Headers(List<Tuple<string, string>> headerList)
            {
                HeaderList = headerList;
            }
        }

        /// <summary>
        /// Container for HTTP Authorization header.
        /// </summary>
        /// <remarks>
        /// Name is typically 'Authorization' and value is service dependent, like e.g. jwt token etc.
        /// </remarks>
        public class AuthorizationHeader : Headers
        {
            public AuthorizationHeader(string value) : base("Authorization", value)
            {
            }

            public AuthorizationHeader(string name, string value) : base(name, value)
            {
            }
        }

        public static async Task<Response> ExecuteRequest(string verb, string url, object content = null, Headers headers = null)
        {
            string GetWebExceptionStatus(WebException webException)
            {
                if (webException.Status == WebExceptionStatus.ProtocolError && webException.Response is HttpWebResponse httpWebResponse)
                {
                    return $"{(int)httpWebResponse.StatusCode} {httpWebResponse.StatusDescription}";
                }
                return webException.Message;
            }

            async Task<string> ReadToEndAsyncNonNull(TextReader streamReader)
            {
                var response = await streamReader.ReadToEndAsync();
                return response ?? string.Empty;
            }

            async Task WriteRequestData(WebRequest webRequest, object contentData)
            {
                if (contentData is string stringData)
                {
                    var bytes = Encoding.ASCII.GetBytes(stringData);
                    webRequest.ContentLength = bytes.Length;
                    if (bytes.Length > 0)
                    {
                        webRequest.ContentType = "application/json";
                        using (var stream = await webRequest.GetRequestStreamAsync())
                        {
                            await stream.WriteAsync(bytes, 0, bytes.Length);
                        }
                    }
                }
                else if (contentData is byte[] formData)
                {
                    webRequest.ContentLength = formData.Length;
                    if (formData.Length > 0)
                    {
                        webRequest.ContentType = "application/x-www-form-urlencoded";
                        using (var stream = await webRequest.GetRequestStreamAsync())
                        {
                            await stream.WriteAsync(formData, 0, formData.Length);
                        }
                    }
                }
                else
                {
                    throw new UnityException($"Invalid content type: {contentData?.GetType().FullName}");
                }
            }
            
            Debug.Log($"ExecuteRequest start {url}");
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            string contentType;
            var httpResponse = string.Empty;
            try
            {
                var request = WebRequest.Create(url);
                request.Method = verb;
                if (headers != null)
                {
                    foreach (var header in headers.HeaderList)
                    {
                        request.Headers.Add(header.Item1, header.Item2);
                    }
                }
                Debug.Log($"Headers #{request.Headers.Count}");
                for (var i = 0; i < request.Headers.Count; ++i)
                {
                    var h = request.Headers;
                    Debug.Log($"{h.GetKey(i)}: {string.Join(',', h.GetValues(i))}");
                }
                if (request is HttpWebRequest webRequest)
                {
                    webRequest.Accept = "application/json";
                }
                if (verb == "POST")
                {
                    await WriteRequestData(request, content);
                }
                if (!(await request.GetResponseAsync() is HttpWebResponse response))
                {
                    return new Response("Request failed: (NULL response)", string.Empty);
                }
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new Response($"Request failed: {response.StatusCode}", string.Empty);
                }
                contentType = response.ContentType;
                await using var dataStream = response.GetResponseStream();
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    httpResponse = await ReadToEndAsyncNonNull(reader);
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    await using var dataStream = e.Response.GetResponseStream();
                    if (dataStream != null)
                    {
                        var reader = new StreamReader(dataStream);
                        httpResponse = await ReadToEndAsyncNonNull(reader);
                    }
                }
                stopWatch.Stop();
                var status = GetWebExceptionStatus(e);
                Debug.Log($"request failed: {status} in {stopWatch.ElapsedMilliseconds} ms");
                Debug.Log($"body {httpResponse}");
                return new Response(status, httpResponse ?? string.Empty);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new Response(e.Message, string.Empty);
            }
            stopWatch.Stop();
            Debug.Log($"request success {contentType} len {httpResponse.Length} in {stopWatch.ElapsedMilliseconds} ms");
            if (contentType.Contains("json"))
            {
                httpResponse = httpResponse.Replace("\r", "").Replace("\n", "");
            }
            Debug.Log($"response {httpResponse}");
            return new Response(httpResponse);
        }
    }
}