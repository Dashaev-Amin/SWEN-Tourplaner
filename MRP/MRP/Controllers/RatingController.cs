using System.Net;
using MRP.Model;
using MRP.Server;
using MRP.Services;

namespace MRP.Controllers
{
    public class RatingController
    {
        private readonly IRatingRepository _ratings;
        private readonly IMediaRepository _media;
        private readonly IUserRepository _users;
        private readonly ITokenService _tokens;

        public RatingController(IRatingRepository ratings, IMediaRepository media, IUserRepository users, ITokenService tokens)
        {
            _ratings = ratings;
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

        private void RecalcAvgScore(int mediaId)
        {
            var list = _ratings.GetByMedia(mediaId).ToList();
            var avg = list.Count == 0 ? 0 : list.Average(r => r.Stars);
            _media.UpdateAvgScore(mediaId, avg);
        }

        // POST /api/media/{mediaId}/rate
        public Task Rate(HttpListenerContext ctx, int mediaId) => ServerUtils.ReadJson<RateDto>(ctx, async dto =>
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var m = _media.Get(mediaId);
            if (m == null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }

            if (dto.Stars < 1 || dto.Stars > 5)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "invalid_input" }, HttpStatusCode.BadRequest);
                return;
            }

            var r = _ratings.Upsert(mediaId, user.Id, dto.Stars, dto.Comment);
            RecalcAvgScore(mediaId);

            await HttpServer.WriteJson(ctx.Response, r, HttpStatusCode.OK);
        });

        // PUT /api/ratings/{ratingId}
        public Task Update(HttpListenerContext ctx, int ratingId) => ServerUtils.ReadJson<RateDto>(ctx, async dto =>
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var existing = _ratings.Get(ratingId);
            if (existing == null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }
            if (existing.UserId != user.Id)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "forbidden" }, HttpStatusCode.Forbidden);
                return;
            }

            if (dto.Stars < 1 || dto.Stars > 5)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "invalid_input" }, HttpStatusCode.BadRequest);
                return;
            }

            var updated = _ratings.Upsert(existing.MediaId, user.Id, dto.Stars, dto.Comment);
            RecalcAvgScore(existing.MediaId);

            await HttpServer.WriteJson(ctx.Response, updated, HttpStatusCode.OK);
        });

        // DELETE /api/ratings/{ratingId}
        public async Task Delete(HttpListenerContext ctx, int ratingId)
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var existing = _ratings.Get(ratingId);
            if (existing == null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }

            var ok = _ratings.Delete(ratingId, user.Id);
            if (!ok)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "forbidden" }, HttpStatusCode.Forbidden);
                return;
            }

            RecalcAvgScore(existing.MediaId);
            await HttpServer.WriteJson(ctx.Response, new { message = "deleted" }, HttpStatusCode.OK);
        }

        // POST /api/ratings/{ratingId}/like
        public async Task Like(HttpListenerContext ctx, int ratingId)
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var exists = _ratings.Get(ratingId);
            if (exists == null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }

            var ok = _ratings.Like(ratingId, user.Id);
            if (!ok)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "cannot_like" }, HttpStatusCode.BadRequest);
                return;
            }

            await HttpServer.WriteJson(ctx.Response, new { message = "liked" }, HttpStatusCode.OK);
        }

        // POST /api/ratings/{ratingId}/confirm
        public async Task Confirm(HttpListenerContext ctx, int ratingId)
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var exists = _ratings.Get(ratingId);
            if (exists == null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }

            var ok = _ratings.Confirm(ratingId, user.Id);
            if (!ok)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "forbidden" }, HttpStatusCode.Forbidden);
                return;
            }

            await HttpServer.WriteJson(ctx.Response, new { message = "confirmed" }, HttpStatusCode.OK);
        }

        // GET /api/media/{mediaId}/ratings
        public async Task ListByMedia(HttpListenerContext ctx, int mediaId)
        {
            var m = _media.Get(mediaId);
            if (m == null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }

            // Only confirmed comments should be visible (safe)
            var list = _ratings.GetByMedia(mediaId)
                .Select(r => new
                {
                    r.Id,
                    r.MediaId,
                    r.UserId,
                    r.Stars,
                    Comment = r.Confirmed ? r.Comment : null,
                    r.Confirmed,
                    Likes = r.LikedByUserIds.Count,
                    r.Created,
                    r.UpdatedAt
                })
                .ToArray();

            await HttpServer.WriteJson(ctx.Response, list);
        }

        // GET /api/users/ratings (auth required)
        public async Task ListMyRatings(HttpListenerContext ctx)
        {
            if (!TryAuth(ctx, out var user))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var list = _ratings.GetByUser(user.Id)
                .Select(r => new
                {
                    r.Id,
                    r.MediaId,
                    r.Stars,
                    r.Comment,
                    r.Confirmed,
                    Likes = r.LikedByUserIds.Count,
                    r.Created,
                    r.UpdatedAt
                })
                .ToArray();

            await HttpServer.WriteJson(ctx.Response, list);
        }


        public class RateDto
        {
            public int Stars { get; set; }
            public string? Comment { get; set; }
        }
    }
}
