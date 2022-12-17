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
    /// Async helper for common REST API requests using JSON.
    /// </summary>
    public static class RestApiServiceAsync
    {
        private const string JsonContentType = "application/json";
        private const string FormPostContentType = "application/x-www-form-urlencoded";
        
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
                return $"{Message}: {Payload}";
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

        /// <summary>
        /// Execute a REST API style request.
        /// </summary>
        /// <param name="verb">GET or POST</param>
        /// <param name="url">API endpoint url</param>
        /// <param name="content">JSON string or FORM POST byte[]</param>
        /// <param name="headers">Optional headers for the request</param>
        /// <returns><c>Response</c> object with success/failure status and JSON response (if any)</returns>
        public static async Task<Response> ExecuteRequest(string verb, string url, object content = null, Headers headers = null)
        {
            Debug.Log($"ExecuteRequest start {url}");
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            string responseContentType;
            var httpResponse = string.Empty;
            try
            {
                // (1) create web request.
                var request = CreateWebRequest(url, verb, headers);
                if (request.Method == "POST")
                {
                    // (2) add post data to request.
                    await WriteRequestData(request, content);
                }
                // (3) submit and wait for response - handle errors if necessary.
                if (await request.GetResponseAsync() is not HttpWebResponse response)
                {
                    return new Response("Request failed: (NULL response)", string.Empty);
                }
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new Response($"Request failed: {response.StatusCode}: {response.StatusDescription}", string.Empty);
                }
                // (4) read response (if any).
                responseContentType = response.ContentType;
                await using var dataStream = response.GetResponseStream();
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    httpResponse = await ReadToEndAsyncNonNull(reader);
                }
                // (5) return our own Response object.
                stopWatch.Stop();
                Debug.Log($"Request success: '{responseContentType}' len {httpResponse.Length} in {stopWatch.ElapsedMilliseconds} ms");
                Debug.Log($"Response: {LogHttpResponse()}");
                return new Response(httpResponse);
            }
            catch (WebException e)
            {
                await using var dataStream = e.Response?.GetResponseStream();
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    httpResponse = await ReadToEndAsyncNonNull(reader);
                }
                stopWatch.Stop();
                var status = GetWebExceptionStatus(e);
                Debug.Log($"Request failed: {status} in {stopWatch.ElapsedMilliseconds} ms");
                Debug.Log($"body {httpResponse}");
                return new Response(status, httpResponse ?? string.Empty);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new Response(e.Message, string.Empty);
            }

            string LogHttpResponse()
            {
                var response = responseContentType.Contains("json") 
                    ? httpResponse.Replace('\r', '.').Replace('\n', '.')
                    : httpResponse;
                return response.Length > 1000 ? response.Substring(0, 1000) : response;
            }
        }

        private static WebRequest CreateWebRequest(string url, string verb, Headers headers)
        {
            var request = WebRequest.Create(url);
            request.Method = verb;
            if (request is HttpWebRequest webRequest)
            {
                webRequest.Accept = JsonContentType;
            }
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
            return request;
        }

        private static string GetWebExceptionStatus(WebException webException)
        {
            if (webException.Status == WebExceptionStatus.ProtocolError && webException.Response is HttpWebResponse httpWebResponse)
            {
                return $"{(int)httpWebResponse.StatusCode} {httpWebResponse.StatusDescription}";
            }
            return webException.Message;
        }

        private static async Task<string> ReadToEndAsyncNonNull(TextReader streamReader)
        {
            var response = await streamReader.ReadToEndAsync();
            return response ?? string.Empty;
        }

        private static async Task WriteRequestData(WebRequest webRequest, object contentData)
        {
            if (contentData is string stringData)
            {
                // This is JSON.
                var bytes = Encoding.UTF8.GetBytes(stringData);
                webRequest.ContentLength = bytes.Length;
                if (bytes.Length > 0)
                {
                    webRequest.ContentType = JsonContentType;
                    await using var stream = await webRequest.GetRequestStreamAsync();
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }
                return;
            }
            if (contentData is byte[] formData)
            {
                // This is Form POST data.
                webRequest.ContentLength = formData.Length;
                if (formData.Length > 0)
                {
                    webRequest.ContentType = FormPostContentType;
                    await using var stream = await webRequest.GetRequestStreamAsync();
                    await stream.WriteAsync(formData, 0, formData.Length);
                }
                return;
            }
            throw new UnityException($"Invalid content type for request data: {contentData?.GetType().FullName}");
        }
    }
}