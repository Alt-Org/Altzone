using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prg.Scripts.Common.Http.RestApi
{
    /// <summary>
    /// Helper to create valid URLs for given server base URL.
    /// </summary>
    public class ServerUrl
    {
        private readonly string _urlPrefix;

        public ServerUrl(string urlPrefix)
        {
            _urlPrefix = urlPrefix.EndsWith("/") ? urlPrefix.Substring(0, urlPrefix.Length - 1) : urlPrefix;
        }

        public string GetUrlFor(string path)
        {
            return path.StartsWith("/") ? $"{_urlPrefix}{path}" : $"{_urlPrefix}/{path}";
        }

        public override string ToString()
        {
            return _urlPrefix;
        }
    }

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

            public readonly int StatusCode;
            public readonly string Message;
            public readonly string Payload;

            public bool Success => StatusCode == (int)HttpStatusCode.OK;

            public Response(string payload)
            {
                StatusCode = (int)HttpStatusCode.OK;
                Message = OkMessage;
                Payload = payload;
            }

            public Response(string message, string payload)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError;
                Message = message;
                Payload = payload;
            }

            public Response(int statusCode, string message, string payload)
            {
                StatusCode = statusCode;
                Message = message;
                Payload = payload;
            }

            public override string ToString()
            {
                return $"({StatusCode}) {Message}: {Payload}";
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
        /// <returns><c>Response</c> object with success/failure status and JSON (or error) server response (if any)</returns>
        public static async Task<Response> ExecuteRequest(string verb, string url, object content = null, Headers headers = null)
        {
            Debug.Log($"ExecuteRequest start {url}");
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                // (1) create web request.
                var request = CreateWebRequest(url, verb, headers);
                if (request.Method == "POST")
                {
                    // (2) add post data to request.
                    await WriteRequestData(request, content);
                }
                // (3) submit the request and wait for the response - handle errors if necessary.
                var response = await request.GetResponseAsync();
                if (response is not HttpWebResponse httpResponse)
                {
                    return new Response("Request failed: (NULL/INVALID response)", string.Empty);
                }
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    return new Response((int)httpResponse.StatusCode, httpResponse.StatusDescription, string.Empty);
                }
                // (4) read server response (if any).
                await using var dataStream = httpResponse.GetResponseStream();
                var serverResponse = await ReadStreamToEndAsync(dataStream);
                stopWatch.Stop();
                var responseContentType = httpResponse.ContentType;
                Debug.Log($"Request success: '{responseContentType}' len {serverResponse.Length} in {stopWatch.ElapsedMilliseconds} ms");
                Debug.Log($"Response ({serverResponse.Length}) {FormatServerResponse(responseContentType, serverResponse)}");
                // (5) return our own Response object.
                return new Response(serverResponse);
            }
            catch (WebException e)
            {
                // (A) read server error response (if any).
                await using var dataStream = e.Response?.GetResponseStream();
                var serverResponse = await ReadStreamToEndAsync(dataStream);
                stopWatch.Stop();
                var status = GetWebExceptionStatus(e);
                Debug.Log($"Request failed: {status.Item1} {status.Item2} in {stopWatch.ElapsedMilliseconds} ms");
                if (serverResponse.Length > 0)
                {
                    Debug.Log($"body ({serverResponse.Length}) {FormatServerResponse("json", serverResponse)}");
                }
                // (B) return our own error Response object.
                return new Response(status.Item1, status.Item2, serverResponse);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return new Response(e.Message, string.Empty);
            }

            string FormatServerResponse(string contentType, string contentText)
            {
                var text = contentType.Contains("json")
                    ? contentText.Replace('\r', '.').Replace('\n', '.')
                    : contentText;
                return text.Length > 1000 ? text.Substring(0, 1000) : text;
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

        private static Tuple<int, string> GetWebExceptionStatus(WebException webException)
        {
            if (webException.Status == WebExceptionStatus.ProtocolError && webException.Response is HttpWebResponse httpWebResponse)
            {
                return new Tuple<int, string>((int)httpWebResponse.StatusCode, httpWebResponse.StatusDescription);
            }
            return new Tuple<int, string>((int)HttpStatusCode.InternalServerError, webException.Message);
        }

        [return: NotNull]
        private static async Task<string> ReadStreamToEndAsync([AllowNull] Stream dataStream)
        {
            if (dataStream == null)
            {
                return string.Empty;
            }
            var streamReader = new StreamReader(dataStream);
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