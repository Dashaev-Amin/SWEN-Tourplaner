using System.Net;
using MRP.Model;
using MRP.Server;
using MRP.Services;

namespace MRP.Controllers
{
    public class MediaController
    {
        private readonly IMediaRepository _repo;
        private readonly IUserRepository _users;
        private readonly ITokenService _tokens;

        public MediaController(IMediaRepository repo, IUserRepository users, ITokenService tokens)
        {
            _repo = repo;
            _users = users;
            _tokens = tokens;
        }

        private bool TryAuth(HttpListenerContext ctx, out User user)
        {
            user = null!;
            if (!_tokens.TryGetUser(ServerUtils.GetAuthHeader(ctx.Request), out var username))
                return false;

            var u = _users.GetByUsername(username);
            if (u == null) return false;

            user = u;
            return true;
        }

        public Task Create(HttpListenerContext ctx) => ServerUtils.ReadJson<Media>(ctx, async dto =>
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "invalid_input" }, HttpStatusCode.BadRequest);
                return;
            }

            var created = _repo.Add(new Media(dto.Title, dto.MediaType, dto.ReleaseYear)
            {
                Description = dto.Description ?? "",
                Genres = dto.Genres ?? new List<string>(),
                AgeRestriction = dto.AgeRestriction,
                CreatorUserID = user.Id
            });

            await HttpServer.WriteJson(ctx.Response, created, HttpStatusCode.Created);
        });

        public async Task GetById(HttpListenerContext ctx, int id)
        {
            if (!TryAuth(ctx, out _))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var m = _repo.Get(id);
            await (m is null
                ? HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound)
                : HttpServer.WriteJson(ctx.Response, m));
        }

        public async Task Search(HttpListenerContext ctx)
        {
            if (!TryAuth(ctx, out _))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            // Filter/Sort via Query-Params
            var qs = ctx.Request.QueryString;

            string? title = qs["title"];
            string? genre = qs["genre"];
            string? sortBy = qs["sortBy"]?.ToLowerInvariant();

            int? releaseYear = int.TryParse(qs["releaseYear"], out var ry) ? ry : null;
            int? ageRestriction = int.TryParse(qs["ageRestriction"], out var ar) ? ar : null;
            double? rating = double.TryParse(qs["rating"], out var r) ? r : null;

            MediaType? mediaType = null;
            var mt = qs["mediaType"];
            if (!string.IsNullOrWhiteSpace(mt) && Enum.TryParse<MediaType>(mt, true, out var parsed))
                mediaType = parsed;

            var list = _repo.Filter(title, genre, mediaType, releaseYear, ageRestriction, rating, sortBy).ToArray();
            await HttpServer.WriteJson(ctx.Response, list);
        }

        public Task Update(HttpListenerContext ctx, int id) => ServerUtils.ReadJson<Media>(ctx, async dto =>
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var existing = _repo.Get(id);
            if (existing is null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }

            if (existing.CreatorUserID != user.Id)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "forbidden" }, HttpStatusCode.Forbidden);
                return;
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "invalid_input" }, HttpStatusCode.BadRequest);
                return;
            }

            dto.Description ??= "";
            dto.Genres ??= new List<string>();

            var updated = _repo.Update(id, dto);
            await HttpServer.WriteJson(ctx.Response, updated!, HttpStatusCode.OK);
        });

        public async Task Delete(HttpListenerContext ctx, int id)
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var existing = _repo.Get(id);
            if (existing is null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }

            if (existing.CreatorUserID != user.Id)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "forbidden" }, HttpStatusCode.Forbidden);
                return;
            }

            _repo.Delete(id);
            await HttpServer.WriteJson(ctx.Response, new { message = "deleted" }, HttpStatusCode.OK);
        }
    }
}
