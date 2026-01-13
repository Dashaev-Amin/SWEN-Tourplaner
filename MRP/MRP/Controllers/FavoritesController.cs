using System.Net;
using MRP.Model;
using MRP.Server;
using MRP.Services;

namespace MRP.Controllers
{
    public class FavoritesController
    {
        private readonly IFavoritesRepository _favorites;
        private readonly IMediaRepository _media;
        private readonly IUserRepository _users;
        private readonly ITokenService _tokens;

        public FavoritesController(
            IFavoritesRepository favorites,
            IMediaRepository media,
            IUserRepository users,
            ITokenService tokens)
        {
            _favorites = favorites;
            _media = media;
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

        // POST /api/media/{id}/favorite
        public async Task Add(HttpListenerContext ctx, int mediaId)
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            if (_media.Get(mediaId) == null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }

            var ok = _favorites.Add(user.Id, mediaId);
            if (!ok)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "already_favorite" }, HttpStatusCode.BadRequest);
                return;
            }

            await HttpServer.WriteJson(ctx.Response, new { message = "favorited" }, HttpStatusCode.OK);
        }

        // DELETE /api/media/{id}/favorite
        public async Task Remove(HttpListenerContext ctx, int mediaId)
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var ok = _favorites.Remove(user.Id, mediaId);
            if (!ok)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_favorite" }, HttpStatusCode.BadRequest);
                return;
            }

            await HttpServer.WriteJson(ctx.Response, new { message = "unfavorited" }, HttpStatusCode.OK);
        }

        // GET /api/users/favorites
        public async Task List(HttpListenerContext ctx)
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var ids = _favorites.GetMediaIds(user.Id).ToList();
            var result = ids
                .Select(id => _media.Get(id))
                .Where(m => m != null)
                .ToArray();

            await HttpServer.WriteJson(ctx.Response, result);
        }
    }
}
