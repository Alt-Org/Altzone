using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using ThreadState = System.Threading.ThreadState;

namespace Prg.Scripts.Common.HttpListenerServer
{
    /// <summary>
    /// Simple 'localhost' HTTP Server based on <c>HttpListener</c>.<br />
    /// You can send requests to following URLs when using port number 8090:<br /> 
    /// http://localhost:8090/ or
    /// http://127.0.0.1:8090/ and
    /// http://&lt;COMPUTERNAME&gt;:8090/
    /// </summary>
    /// <remarks>
    /// Inspiration from:<br />
    /// https://gist.github.com/SimonCropp/980071
    /// https://github.com/arshad115/HttpListenerServer
    /// https://github.com/sableangle/UnityHTTPServer
    /// </remarks>
    public interface ISimpleListenerServer
    {
        bool IsRunning { get; }
        void Start();
        void AddHandler(IListenerServerHandler handler);
        void Stop();
    }

    /// <summary>
    /// Request handler for <c>ISimpleListenerServer</c>.
    /// </summary>
    public interface IListenerServerHandler
    {
        /// <summary>
        /// Handles a request (or ignores it).
        /// </summary>
        /// <remarks>
        /// Returned <c>object</c> is converted to JSON string and returned <c>string</c> is assumed to be valid JSON already.
        /// </remarks>
        /// <param name="request">the request to handle</param>
        /// <param name="body">body content (as string)</param>
        /// <returns><c>object</c> or <c>string</c> on success and null if ignored it (did not handle).</returns>
        object HandleRequest(HttpListenerRequest request, string body);
    }

    public static class SimpleListenerServerFactory
    {
        public static ISimpleListenerServer Create(int port, HttpListenerServer watchDog = null)
        {
            return new SimpleListenerServer(port, watchDog);
        }
    }

    internal class SimpleListenerServer : ISimpleListenerServer
    {
        private const string JsonContentType = "application/json";
        private const string FormPostContentType = "application/x-www-form-urlencoded";
        private const int BufferSize = 4 * 1024;

        private readonly int _port;
        private readonly Thread _serverThread;
        private readonly HttpListener _listener;
        private readonly List<IListenerServerHandler> _handlers = new();

        private byte[] _buffer;

        public bool IsRunning => _listener.IsListening;

        public SimpleListenerServer(int port, HttpListenerServer watchDog = null)
        {
            _port = port;
            _serverThread = new Thread(ListenThread);
            _listener = new HttpListener();
            Application.quitting += Stop;
            if (watchDog == null)
            {
                CreateUnityWatchDog();
            }
        }

        [Conditional("UNITY_EDITOR")]
        private void CreateUnityWatchDog()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return;
            }
#endif
            var parent = new GameObject($"{nameof(SimpleListenerServer)}:{_port}");
            Object.DontDestroyOnLoad(parent);
            var watchdog = parent.AddComponent<HttpListenerServer>();
            watchdog._port = _port;
            watchdog.Server = this;
        }

        public void Start()
        {
            Debug.Log($"{_port} {_serverThread.ThreadState}");
            if (_serverThread.ThreadState != ThreadState.Unstarted)
            {
                return;
            }
            _serverThread.Start();
        }

        public void Stop()
        {
            Debug.Log($"{_port} {_serverThread.ThreadState}");
            _serverThread.Abort();
            _listener.Stop();
        }

        public void AddHandler(IListenerServerHandler handler)
        {
            Debug.Log($"{_port} {handler}");
            _handlers.Add(handler);
        }

        private void ListenThread()
        {
            var uriPrefix = "http://*:" + _port + "/";
            _listener.Prefixes.Add(uriPrefix);
            Debug.Log($"{_port} start server @ {uriPrefix}");
            try
            {
                _buffer = new byte[BufferSize];
                _listener.Start();
                for (;;)
                {
                    var context = _listener.GetContext();
                    HandleContext(context);
                }
            }
            catch (ThreadAbortException)
            {
                Debug.Log($"{_port} server stopped");
            }
            catch (Exception x)
            {
                Debug.Log($"{_port} unhandled exception: {x.GetType().FullName} : {x.Message}");
                Debug.LogException(x);
            }
            finally
            {
                _listener.Stop();
            }
        }

        private void HandleContext(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                string body = null;
                if (request.HasEntityBody)
                {
                    var validContentType = request.ContentType is JsonContentType or FormPostContentType;
                    if (!validContentType)
                    {
                        throw new InvalidOperationException($"invalid content type: {request.ContentType}");
                    }
                    var encoding = request.ContentEncoding;
                    var inputStream = request.InputStream;
                    var reader = new StreamReader(inputStream, encoding);
                    body = reader.ReadToEnd();
                    reader.Close();
                    inputStream.Close();
                }
                foreach (var handler in _handlers)
                {
                    var response = handler.HandleRequest(context.Request, body);
                    if (response != null)
                    {
                        WriteResponse(response);
                        context.Response.OutputStream.Flush();
                        context.Response.OutputStream.Close();
                        return;
                    }
                }
                throw new InvalidOperationException("No handlers found");
            }
            catch (Exception x)
            {
                Debug.Log($"{_port} request handler failed: {x.GetType().FullName} : {x.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.StatusDescription = $"{x.GetType().FullName} : {x.Message}";
            }
            finally
            {
                context.Response.Close();
            }

            void WriteResponse(object responseObject)
            {
                context.Response.ContentType = "application/json";
                var jsonString = responseObject is string ? responseObject.ToString() : JsonUtilityWrapper(responseObject);
                var jsonByte = Encoding.UTF8.GetBytes(jsonString);
                context.Response.ContentLength64 = jsonByte.Length;
                var memoryStream = new MemoryStream(jsonByte);
                for (;;)
                {
                    var byteCount = memoryStream.Read(_buffer, 0, _buffer.Length);
                    if (byteCount <= 0)
                    {
                        break;
                    }
                    context.Response.OutputStream.Write(_buffer, 0, byteCount);
                }
                memoryStream.Close();
            }

            string JsonUtilityWrapper(object instance)
            {
                // It seems that JsonUtility.ToJson does HTML URL Encoding :-(
                // - we want to revert that behaviour - at least for now as we don't know of anything better!
                var jsonString = JsonUtility.ToJson(instance);
                if (jsonString.IndexOf('%') < 0)
                {
                    return jsonString;
                }
                jsonString = Uri.UnescapeDataString(jsonString);
                Debug.Log($"=>{jsonString}");
                return jsonString;
            }
        }
    }

    /// <summary>
    /// UNITY Editor helper.
    /// </summary>
    public class HttpListenerServer : MonoBehaviour
    {
        public int _port;

        public ISimpleListenerServer Server;

        private void OnEnable()
        {
            Debug.Log($"port {_port}");
            if (_port > 0 && Server == null)
            {
                Server = SimpleListenerServerFactory.Create(_port, this);
                Server.Start();
            }
        }
    }
}