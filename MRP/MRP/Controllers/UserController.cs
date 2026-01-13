using System.Net;
using System.Text.Json;
using MRP.Model;
using MRP.Server;
using MRP.Services;

namespace MRP.Controllers;

public class UserController
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly IUserRepository _repo;
    private readonly ITokenService _tokens;
    private readonly IRatingRepository _ratings;
    private readonly IMediaRepository _media;
    private readonly IFavoritesRepository _favorites;

    public UserController(
        IUserRepository repo,
        ITokenService tokens,
        IRatingRepository ratings,
        IMediaRepository media,
        IFavoritesRepository favorites)
    {
        _repo = repo;
        _tokens = tokens;
        _ratings = ratings;
        _media = media;
        _favorites = favorites;
    }

    public async Task Register(HttpListenerContext ctx)
    {
        var dto = await JsonSerializer.DeserializeAsync<User>(ctx.Request.InputStream, JsonOpts)
                  ?? new User();
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            await HttpServer.WriteJson(ctx.Response, new { error = "invalid_input" }, HttpStatusCode.BadRequest);
            return;
        }
        if (_repo.Exists(dto.Username))
        {
            await HttpServer.WriteJson(ctx.Response, new { error = "user_exists" }, HttpStatusCode.Conflict);
            return;
        }
        _repo.Add(new User(dto.Username, dto.Password));
        await HttpServer.WriteJson(ctx.Response, new { message = "registered" }, HttpStatusCode.Created);
    }

    public async Task Login(HttpListenerContext ctx)
    {
        var dto = await JsonSerializer.DeserializeAsync<User>(ctx.Request.InputStream, JsonOpts)
                  ?? new User();
        var user = _repo.Find(dto.Username, dto.Password);
        if (user is null)
        {
            await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
            return;
        }
        var token = _tokens.Issue(user.Username);
        await HttpServer.WriteJson(ctx.Response, new { token }, HttpStatusCode.OK);
    }

    public async Task Profile(HttpListenerContext ctx, string username)
    {
        if (!_tokens.TryGetUser(ServerUtils.GetAuthHeader(ctx.Request), out var tokenUser))
        {
            await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
            return;
        }

        if (!username.Equals(tokenUser, StringComparison.OrdinalIgnoreCase))
        {
            await HttpServer.WriteJson(ctx.Response, new { error = "forbidden" }, HttpStatusCode.Forbidden);
            return;
        }

        var user = _repo.GetByUsername(username);
        if (user == null)
        {
            await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
            return;
        }

        var ratings = _ratings.GetByUser(user.Id).ToList();
        var avgScore = ratings.Count == 0 ? 0.0 : ratings.Average(r => r.Stars);

        var mediaById = _media.GetAll().ToDictionary(m => m.Id, m => m);
        var genreCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in ratings)
        {
            if (!mediaById.TryGetValue(r.MediaId, out var m)) continue;
            foreach (var g in m.Genres)
            {
                if (string.IsNullOrWhiteSpace(g)) continue;
                genreCounts[g] = genreCounts.TryGetValue(g, out var c) ? c + 1 : 1;
            }
        }
        var favoriteGenre = genreCounts.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).FirstOrDefault();

        var favoritesCount = _favorites.GetMediaIds(user.Id).Count();

        await HttpServer.WriteJson(ctx.Response, new
        {
            username = user.Username,
            displayName = user.DisplayName,
            bio = user.Bio,
            stats = new
            {
                totalRatings = ratings.Count,
                avgScore,
                favoriteGenre,
                favoritesCount
            }
        });
    }

    public Task UpdateProfile(HttpListenerContext ctx, string username)
        => ServerUtils.ReadJson<ProfileUpdateDto>(ctx, async dto =>
        {
            if (!_tokens.TryGetUser(ServerUtils.GetAuthHeader(ctx.Request), out var tokenUser))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "unauthorized" }, HttpStatusCode.Unauthorized);
                return;
            }

            if (!username.Equals(tokenUser, StringComparison.OrdinalIgnoreCase))
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "forbidden" }, HttpStatusCode.Forbidden);
                return;
            }

            var user = _repo.GetByUsername(username);
            if (user == null)
            {
                await HttpServer.WriteJson(ctx.Response, new { error = "not_found" }, HttpStatusCode.NotFound);
                return;
            }

            var updated = _repo.UpdateProfile(user.Id, dto.DisplayName, dto.Bio);
            await HttpServer.WriteJson(ctx.Response, new
            {
                username = updated?.Username,
                displayName = updated?.DisplayName,
                bio = updated?.Bio
            });
        });

    public class ProfileUpdateDto
    {
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
    }
}
