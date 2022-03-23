using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Prg.Scripts.Common.RestApi
{
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

        public static async Task<Response> ExecuteRequest(string verb, string url, byte[] content, string jwtToken = null)
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

            Debug.Log($"ExecuteRequest start {url}");
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            string contentType;
            var httpResponse = string.Empty;
            try
            {
                var request = WebRequest.Create(url);
                request.Method = verb;
                if (jwtToken != null)
                {
                    request.Headers.Add("Authorization", "Bearer " + jwtToken);
                }
                if (request is HttpWebRequest webRequest)
                {
                    webRequest.Accept = "application/json";
                }
                if (verb == "POST")
                {
                    request.ContentLength = content.Length;
                    if (content.Length > 0)
                    {
                        request.ContentType = "application/x-www-form-urlencoded";
                        using (var stream = await request.GetRequestStreamAsync())
                        {
                            await stream.WriteAsync(content, 0, content.Length);
                        }
                    }
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
                using (var dataStream = response.GetResponseStream())
                {
                    if (dataStream != null)
                    {
                        var reader = new StreamReader(dataStream);
                        httpResponse = await ReadToEndAsyncNonNull(reader);
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (var dataStream = e.Response.GetResponseStream())
                    {
                        if (dataStream != null)
                        {
                            var reader = new StreamReader(dataStream);
                            httpResponse = await ReadToEndAsyncNonNull(reader);
                        }
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