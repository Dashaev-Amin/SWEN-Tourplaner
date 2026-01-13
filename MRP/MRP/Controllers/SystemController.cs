using System.Net;
using MRP.Server;

namespace MRP.Controllers
{
    public class SystemController
    {
        public Task Root(HttpListenerContext ctx) =>
            HttpServer.WriteJson(ctx.Response, new
            {
                name = "MRP",
                status = "ok",
                endpoints = new[] { "/api/health" }
            });

        public Task Health(HttpListenerContext ctx) =>
            HttpServer.WriteJson(ctx.Response, new
            {
                status = "healthy",
                time = DateTime.UtcNow
            });
    }
}
