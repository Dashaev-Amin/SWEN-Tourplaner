using MRP.Server;

var server = new HttpServer(new[] { "http://localhost:8080/" });
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    server.Stop();
};
await server.StartAsync();
