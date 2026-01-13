using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MRP.Server
{
    public class HttpServer
    {
        private readonly HttpListener _listener = new();
        private readonly RequestRouter _router = new();
        private volatile bool _running;

        public HttpServer(IEnumerable<string> prefixes)
        {
            foreach (var p in prefixes) _listener.Prefixes.Add(p);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            _running = true;
            Console.WriteLine($"MRP Server listening on: {string.Join(", ", _listener.Prefixes)}");

            while (_running)
            {
                HttpListenerContext ctx;
                try { ctx = await _listener.GetContextAsync(); }
                catch (HttpListenerException) { break; } // stopped
                catch (ObjectDisposedException) { break; }

                _ = Task.Run(async () =>
                {
                    try { await _router.HandleAsync(ctx); }
                    catch (Exception ex)
                    {
                        await WriteJson(ctx.Response, new { error = "internal_error", detail = ex.Message }, HttpStatusCode.InternalServerError);
                    }
                    finally { ctx.Response.OutputStream.Close(); }
                });
            }
        }

        public void Stop()
        {
            _running = false;
            if (_listener.IsListening) _listener.Stop();
            _listener.Close();
            Console.WriteLine("MRP Server stopped.");
        }

        internal static Task WriteJson(HttpListenerResponse resp, object payload, HttpStatusCode code = HttpStatusCode.OK)
        {
            var json = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            resp.StatusCode = (int)code;
            resp.ContentType = "application/json; charset=utf-8";
            resp.ContentLength64 = bytes.Length;
            return resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
