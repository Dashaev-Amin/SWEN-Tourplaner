using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MRP.Server
{
    public static class ServerUtils
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public static string? GetAuthHeader(HttpListenerRequest req)
            => req.Headers["Authorization"] ?? req.Headers["Authentication"];

        public static async Task ReadJson<T>(HttpListenerContext ctx, Func<T, Task> handler)
        {
            try
            {
                var dto = await JsonSerializer.DeserializeAsync<T>(ctx.Request.InputStream, JsonOpts);
                if (dto == null) throw new Exception("empty");
                await handler(dto);
            }
            catch
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "invalid_input" }, HttpStatusCode.BadRequest);
            }
        }
    }
}
