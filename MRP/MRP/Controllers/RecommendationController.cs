using System.Net;
using MRP.Model;
using MRP.Server;
using MRP.Services;

namespace MRP.Controllers
{
    public class RecommendationController
    {
        private readonly IMediaRepository _media;
        private readonly IRatingRepository _ratings;
        private readonly IUserRepository _users;
        private readonly ITokenService _tokens;

        public RecommendationController(
            IMediaRepository media,
            IRatingRepository ratings,
            IUserRepository users,
            ITokenService tokens)
        {
            _media = media;
            _ratings = ratings;
            _users = users;
            _tokens = tokens;
        }

        // GET /api/users/recommendations
        public async Task ForUser(HttpListenerContext ctx)
        {
            if (!_tokens.TryGetUser(ServerUtils.GetAuthHeader(ctx.Request), out var username))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            var user = _users.GetByUsername(username);
            if (user == null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }

            var allMedia = _media.GetAll().ToList();
            var mediaById = allMedia.ToDictionary(m => m.Id, m => m);
            var myRatings = _ratings.GetByUser(user.Id).ToList();
            var ratedMediaIds = new HashSet<int>(myRatings.Select(r => r.MediaId));

            if (myRatings.Count == 0)
            {
                var top = allMedia
                    .OrderByDescending(m => m.AvgScore)
                    .ThenBy(m => m.Title)
                    .Take(10)
                    .ToArray();
                await HttpServer.WriteJson(ctx.Response, top);
                return;
            }

            var preferred = myRatings.Where(r => r.Stars >= 4).ToList();
            if (preferred.Count == 0) preferred = myRatings;

            var preferredGenres = preferred
                .SelectMany(r => mediaById.TryGetValue(r.MediaId, out var m) ? m.Genres : new List<string>())
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .GroupBy(g => g, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(3)
                .ToList();

            var preferredType = preferred
                .Select(r => mediaById.TryGetValue(r.MediaId, out var m) ? (MediaType?)m.MediaType : null)
                .Where(t => t.HasValue)
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var preferredAge = preferred
                .Select(r => mediaById.TryGetValue(r.MediaId, out var m) ? (int?)m.AgeRestriction : null)
                .Where(a => a.HasValue)
                .GroupBy(a => a)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var results = allMedia
                .Where(m => !ratedMediaIds.Contains(m.Id))
                .Select(m =>
                {
                    var matches = m.Genres.Count(g => preferredGenres.Any(pg =>
                        pg.Equals(g, StringComparison.OrdinalIgnoreCase)));
                    var score = matches * 2.0;
                    if (preferredType.HasValue && m.MediaType == preferredType.Value) score += 1;
                    if (preferredAge.HasValue && m.AgeRestriction == preferredAge.Value) score += 1;
                    score += m.AvgScore / 5.0;
                    return new { Media = m, Score = score };
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Media.AvgScore)
                .ThenBy(x => x.Media.Title)
                .Take(10)
                .Select(x => x.Media)
                .ToArray();

            await HttpServer.WriteJson(ctx.Response, results);
        }
    }
}
