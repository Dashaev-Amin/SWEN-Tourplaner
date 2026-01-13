using System.Net;
using MRP.Controllers;
using MRP.Persistence;
using MRP.Services;

namespace MRP.Server;

public class RequestRouter
{
    private readonly SystemController _system = new();

    private readonly UserController _users;
    private readonly MediaController _media;
    private readonly RatingController _ratings;
    private readonly FavoritesController _favorites;
    private readonly LeaderboardController _leaderboard;
    private readonly RecommendationController _recommendations;

    private readonly ITokenService _tokens;

    private readonly IMediaRepository _mediaRepo;


    public RequestRouter()
    {
        var usePostgres = true;

        IUserRepository userRepo = usePostgres
            ? new PostgresUserRepository()
            : new InMemoryUserRepository();

        _tokens = new TokenService();

        _mediaRepo = usePostgres
            ? new PostgresMediaRepository()
            : new InMemoryMediaRepository();

        IRatingRepository ratingRepo = usePostgres
            ? new PostgresRatingRepository()
            : new InMemoryRatingRepository();

        IFavoritesRepository favRepo = usePostgres
            ? new PostgresFavoritesRepository()
            : new InMemoryFavoritesRepository();

        _users = new UserController(userRepo, _tokens, ratingRepo, _mediaRepo, favRepo);
        _media = new MediaController(_mediaRepo, userRepo, _tokens);
        _ratings = new RatingController(ratingRepo, _mediaRepo, userRepo, _tokens);
        _favorites = new FavoritesController(favRepo, _mediaRepo, userRepo, _tokens);

        _leaderboard = new LeaderboardController(ratingRepo, userRepo);
        _recommendations = new RecommendationController(_mediaRepo, ratingRepo, userRepo, _tokens);

    }

    public async Task HandleAsync(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        var path = req.Url!.AbsolutePath.TrimEnd('/');

        if (!IsAuthFree(path) && !_tokens.TryGetUser(ServerUtils.GetAuthHeader(req), out _))
        {
            await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
            return;
        }

        // --- System ---
        if (req.HttpMethod == "GET" && path.Equals("/api/health", StringComparison.OrdinalIgnoreCase))
        { await _system.Health(ctx); return; }

        if (req.HttpMethod == "GET" && path.Equals("/", StringComparison.OrdinalIgnoreCase))
        { await _system.Root(ctx); return; }

        if (req.HttpMethod == "GET" && path.Equals("/api/leaderboard", StringComparison.OrdinalIgnoreCase))
        { await _leaderboard.TopUsers(ctx); return; }


        // --- Users ---
        if (req.HttpMethod == "POST" && path.Equals("/api/users/register", StringComparison.OrdinalIgnoreCase))
        { await _users.Register(ctx); return; }

        if (req.HttpMethod == "POST" && path.Equals("/api/users/login", StringComparison.OrdinalIgnoreCase))
        { await _users.Login(ctx); return; }

        if (req.HttpMethod == "GET"
            && path.StartsWith("/api/users/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith("/profile", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // api users {username} profile -> [api, users, {username}, profile]
            if (parts.Length == 4)
            { await _users.Profile(ctx, parts[2]); return; }
        }

        if (req.HttpMethod == "PUT"
            && path.StartsWith("/api/users/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith("/profile", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // api users {username} profile -> [api, users, {username}, profile]
            if (parts.Length == 4)
            { await _users.UpdateProfile(ctx, parts[2]); return; }
        }

        if (req.HttpMethod == "GET" && path.Equals("/api/users/recommendations", StringComparison.OrdinalIgnoreCase))
        { await _recommendations.ForUser(ctx); return; }

        if (req.HttpMethod == "GET" && path.Equals("/api/users/favorites", StringComparison.OrdinalIgnoreCase))
        { await _favorites.List(ctx); return; }

        // Step 5.1: GET /api/users/ratings
        if (req.HttpMethod == "GET" && path.Equals("/api/users/ratings", StringComparison.OrdinalIgnoreCase))
        { await _ratings.ListMyRatings(ctx); return; }

        // --- Media ---
        if (req.HttpMethod == "POST" && path.Equals("/api/media", StringComparison.OrdinalIgnoreCase))
        { await _media.Create(ctx); return; }

        if (req.HttpMethod == "GET" && path.Equals("/api/media", StringComparison.OrdinalIgnoreCase))
        { await _media.Search(ctx); return; }

        // POST /api/media/{id}/rate  (MUST be before /api/media/{id} handlers)
        if (req.HttpMethod == "POST"
            && path.StartsWith("/api/media/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith("/rate", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // api media {id} rate -> [api, media, {id}, rate]
            if (parts.Length == 4 && int.TryParse(parts[2], out var mediaId))
            { await _ratings.Rate(ctx, mediaId); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // POST /api/media/{id}/favorite
        if (req.HttpMethod == "POST"
            && path.StartsWith("/api/media/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith("/favorite", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // api media {id} favorite -> [api, media, {id}, favorite]
            if (parts.Length == 4 && int.TryParse(parts[2], out var mediaId))
            { await _favorites.Add(ctx, mediaId); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // DELETE /api/media/{id}/favorite
        if (req.HttpMethod == "DELETE"
            && path.StartsWith("/api/media/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith("/favorite", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // api media {id} favorite -> [api, media, {id}, favorite]
            if (parts.Length == 4 && int.TryParse(parts[2], out var mediaId))
            { await _favorites.Remove(ctx, mediaId); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // Step 5.1: GET /api/media/{id}/ratings  (MUST be before /api/media/{id})
        if (req.HttpMethod == "GET"
            && path.StartsWith("/api/media/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith("/ratings", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // api media {id} ratings -> [api, media, {id}, ratings]
            if (parts.Length == 4 && int.TryParse(parts[2], out var mediaId))
            { await _ratings.ListByMedia(ctx, mediaId); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // GET /api/media/{id}
        if (req.HttpMethod == "GET" && path.StartsWith("/api/media/", StringComparison.OrdinalIgnoreCase))
        {
            var last = path.Split('/').Last();
            if (int.TryParse(last, out var id))
            { await _media.GetById(ctx, id); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // PUT /api/media/{id}
        if (req.HttpMethod == "PUT" && path.StartsWith("/api/media/", StringComparison.OrdinalIgnoreCase))
        {
            var last = path.Split('/').Last();
            if (int.TryParse(last, out var id))
            { await _media.Update(ctx, id); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // DELETE /api/media/{id}
        if (req.HttpMethod == "DELETE" && path.StartsWith("/api/media/", StringComparison.OrdinalIgnoreCase))
        {
            var last = path.Split('/').Last();
            if (int.TryParse(last, out var id))
            { await _media.Delete(ctx, id); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // --- Ratings ---
        // PUT /api/ratings/{id}
        if (req.HttpMethod == "PUT" && path.StartsWith("/api/ratings/", StringComparison.OrdinalIgnoreCase))
        {
            var last = path.Split('/').Last();
            if (int.TryParse(last, out var ratingId))
            { await _ratings.Update(ctx, ratingId); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // DELETE /api/ratings/{id}
        if (req.HttpMethod == "DELETE" && path.StartsWith("/api/ratings/", StringComparison.OrdinalIgnoreCase))
        {
            var last = path.Split('/').Last();
            if (int.TryParse(last, out var ratingId))
            { await _ratings.Delete(ctx, ratingId); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // POST /api/ratings/{id}/like
        if (req.HttpMethod == "POST"
            && path.StartsWith("/api/ratings/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith("/like", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // api ratings {id} like -> [api, ratings, {id}, like]
            if (parts.Length == 4 && int.TryParse(parts[2], out var ratingId))
            { await _ratings.Like(ctx, ratingId); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // POST /api/ratings/{id}/confirm
        if (req.HttpMethod == "POST"
            && path.StartsWith("/api/ratings/", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith("/confirm", StringComparison.OrdinalIgnoreCase))
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            // api ratings {id} confirm -> [api, ratings, {id}, confirm]
            if (parts.Length == 4 && int.TryParse(parts[2], out var ratingId))
            { await _ratings.Confirm(ctx, ratingId); return; }

            await HttpServer.WriteJson(ctx.Response, new { error = "bad_id" }, HttpStatusCode.BadRequest);
            return;
        }

        // --- 404 fallback ---
        await HttpServer.WriteJson(ctx.Response, new { error = "not_found", path }, HttpStatusCode.NotFound);
    }

    private static bool IsAuthFree(string path)
        => path.Equals("/api/users/register", StringComparison.OrdinalIgnoreCase)
           || path.Equals("/api/users/login", StringComparison.OrdinalIgnoreCase);
}
