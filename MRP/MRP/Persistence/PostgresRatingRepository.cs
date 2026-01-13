using MRP.Model;
using MRP.Services;
using Npgsql;

namespace MRP.Persistence
{
    public class PostgresRatingRepository : IRatingRepository
    {
        public Rating Upsert(int mediaId, int userId, int stars, string? comment)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO ratings (media_id, user_id, stars, comment, confirmed, created, updated_at) " +
                "VALUES (@m, @u, @s, @c, @confirmed, @now, NULL) " +
                "ON CONFLICT (media_id, user_id) DO UPDATE " +
                "SET stars = EXCLUDED.stars, comment = EXCLUDED.comment, confirmed = EXCLUDED.confirmed, updated_at = @now " +
                "RETURNING id, media_id, user_id, stars, comment, confirmed, created, updated_at", conn);

            var confirmed = string.IsNullOrWhiteSpace(comment);
            cmd.Parameters.AddWithValue("@m", mediaId);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@s", stars);
            cmd.Parameters.AddWithValue("@c", (object?)comment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@confirmed", confirmed);
            cmd.Parameters.AddWithValue("@now", DateTime.Now);

            Rating rating;
            using (var r = cmd.ExecuteReader())
            {
                r.Read();
                rating = Map(r);
            }
            rating.LikedByUserIds = GetLikes(conn, rating.Id).ToHashSet();
            return rating;
        }

        public Rating? Get(int ratingId)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT id, media_id, user_id, stars, comment, confirmed, created, updated_at FROM ratings WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", ratingId);
            Rating? rating;
            using (var r = cmd.ExecuteReader())
            {
                if (!r.Read()) return null;
                rating = Map(r);
            }
            rating.LikedByUserIds = GetLikes(conn, rating.Id).ToHashSet();
            return rating;
        }

        public IEnumerable<Rating> GetByMedia(int mediaId)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT id, media_id, user_id, stars, comment, confirmed, created, updated_at FROM ratings WHERE media_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", mediaId);
            var list = new List<Rating>();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) list.Add(Map(r));
            }

            AttachLikes(conn, list);
            return list;
        }

        public IEnumerable<Rating> GetByUser(int userId)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT id, media_id, user_id, stars, comment, confirmed, created, updated_at FROM ratings WHERE user_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", userId);
            var list = new List<Rating>();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) list.Add(Map(r));
            }

            AttachLikes(conn, list);
            return list;
        }

        public bool Delete(int ratingId, int userId)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "DELETE FROM ratings WHERE id = @id AND user_id = @u", conn);
            cmd.Parameters.AddWithValue("@id", ratingId);
            cmd.Parameters.AddWithValue("@u", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Like(int ratingId, int userId)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var check = new NpgsqlCommand(
                "SELECT user_id FROM ratings WHERE id = @id", conn);
            check.Parameters.AddWithValue("@id", ratingId);
            var owner = check.ExecuteScalar();
            if (owner == null) return false;
            if (Convert.ToInt32(owner) == userId) return false;

            using var cmd = new NpgsqlCommand(
                "INSERT INTO rating_likes (rating_id, user_id) VALUES (@r, @u) ON CONFLICT DO NOTHING", conn);
            cmd.Parameters.AddWithValue("@r", ratingId);
            cmd.Parameters.AddWithValue("@u", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Confirm(int ratingId, int userId)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "UPDATE ratings SET confirmed = true, updated_at = @now WHERE id = @id AND user_id = @u", conn);
            cmd.Parameters.AddWithValue("@now", DateTime.Now);
            cmd.Parameters.AddWithValue("@id", ratingId);
            cmd.Parameters.AddWithValue("@u", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public IEnumerable<Rating> GetAll()
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT id, media_id, user_id, stars, comment, confirmed, created, updated_at FROM ratings", conn);
            var list = new List<Rating>();
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read()) list.Add(Map(r));
            }

            AttachLikes(conn, list);
            return list;
        }

        private static Rating Map(NpgsqlDataReader r)
            => new Rating
            {
                Id = r.GetInt32(0),
                MediaId = r.GetInt32(1),
                UserId = r.GetInt32(2),
                Stars = r.GetInt32(3),
                Comment = r.IsDBNull(4) ? null : r.GetString(4),
                Confirmed = r.GetBoolean(5),
                Created = r.GetDateTime(6),
                UpdatedAt = r.IsDBNull(7) ? null : r.GetDateTime(7)
            };

        private static IEnumerable<int> GetLikes(NpgsqlConnection conn, int ratingId)
        {
            using var cmd = new NpgsqlCommand(
                "SELECT user_id FROM rating_likes WHERE rating_id = @r", conn);
            cmd.Parameters.AddWithValue("@r", ratingId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) yield return r.GetInt32(0);
        }

        private static void AttachLikes(NpgsqlConnection conn, List<Rating> ratings)
        {
            if (ratings.Count == 0) return;
            var ids = ratings.Select(r => r.Id).ToArray();
            using var cmd = new NpgsqlCommand(
                "SELECT rating_id, user_id FROM rating_likes WHERE rating_id = ANY(@ids)", conn);
            cmd.Parameters.AddWithValue("@ids", ids);
            using var r = cmd.ExecuteReader();
            var map = new Dictionary<int, HashSet<int>>();
            while (r.Read())
            {
                var ratingId = r.GetInt32(0);
                var userId = r.GetInt32(1);
                if (!map.TryGetValue(ratingId, out var set))
                {
                    set = new HashSet<int>();
                    map[ratingId] = set;
                }
                set.Add(userId);
            }

            foreach (var rating in ratings)
                rating.LikedByUserIds = map.TryGetValue(rating.Id, out var set)
                    ? set
                    : new HashSet<int>();
        }
    }
}
