using System.Net;
using MRP.Server;
using MRP.Services;

namespace MRP.Controllers
{
    public class LeaderboardController
    {
        private readonly IRatingRepository _ratings;
        private readonly IUserRepository _users;

        public LeaderboardController(IRatingRepository ratings, IUserRepository users)
        {
            _ratings = ratings;
            _users = users;
        }

        // GET /api/leaderboard
        public async Task TopUsers(HttpListenerContext ctx)
        {
            var result = _ratings.GetAll()
                .GroupBy(r => r.UserId)
                .Select(g =>
                {
                    var u = _users.GetById(g.Key);
                    return new
                    {
                        UserId = g.Key,
                        Username = u?.Username,
                        RatingsCount = g.Count(),
                        TotalLikes = g.Sum(r => r.LikedByUserIds.Count)
                    };
                })
                .OrderByDescending(x => x.RatingsCount)
                .ThenByDescending(x => x.TotalLikes)
                .Take(10)
                .ToArray();

            await HttpServer.WriteJson(ctx.Response, result);
        }
    }
}
