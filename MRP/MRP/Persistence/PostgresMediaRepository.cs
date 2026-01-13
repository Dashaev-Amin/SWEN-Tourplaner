using MRP.Model;
using MRP.Services;
using Npgsql;

namespace MRP.Persistence
{
    public class PostgresMediaRepository : IMediaRepository
    {
        public Media Add(Media m)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO media (creator_user_id, title, description, media_type, release_year, genres, age_restriction, avg_score, created_at) " +
                "VALUES (@c, @t, @d, @mt, @y, @g, @a, @s, @created) " +
                "RETURNING id", conn);

            cmd.Parameters.AddWithValue("@c", m.CreatorUserID);
            cmd.Parameters.AddWithValue("@t", m.Title);
            cmd.Parameters.AddWithValue("@d", m.Description ?? "");
            cmd.Parameters.AddWithValue("@mt", (int)m.MediaType);
            cmd.Parameters.AddWithValue("@y", m.ReleaseYear);
            cmd.Parameters.AddWithValue("@g", ToDbGenres(m.Genres));
            cmd.Parameters.AddWithValue("@a", m.AgeRestriction);
            cmd.Parameters.AddWithValue("@s", m.AvgScore);
            cmd.Parameters.AddWithValue("@created", m.CreatedAt);

            m.Id = Convert.ToInt32(cmd.ExecuteScalar());
            return m;
        }

        public Media? Get(int id)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT id, creator_user_id, title, description, media_type, release_year, genres, age_restriction, avg_score, created_at " +
                "FROM media WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return Map(r);
        }

        public IEnumerable<Media> GetAll()
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT id, creator_user_id, title, description, media_type, release_year, genres, age_restriction, avg_score, created_at FROM media", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) yield return Map(r);
        }

        public IEnumerable<Media> Search(string? title = null)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT id, creator_user_id, title, description, media_type, release_year, genres, age_restriction, avg_score, created_at " +
                "FROM media WHERE (@t IS NULL OR title ILIKE '%' || @t || '%')", conn);
            cmd.Parameters.AddWithValue("@t", (object?)title ?? DBNull.Value);

            using var r = cmd.ExecuteReader();
            while (r.Read()) yield return Map(r);
        }

        public Media? Update(int id, Media updated)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "UPDATE media SET title = @t, description = @d, media_type = @mt, release_year = @y, genres = @g, age_restriction = @a " +
                "WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@t", updated.Title);
            cmd.Parameters.AddWithValue("@d", updated.Description ?? "");
            cmd.Parameters.AddWithValue("@mt", (int)updated.MediaType);
            cmd.Parameters.AddWithValue("@y", updated.ReleaseYear);
            cmd.Parameters.AddWithValue("@g", ToDbGenres(updated.Genres));
            cmd.Parameters.AddWithValue("@a", updated.AgeRestriction);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = cmd.ExecuteNonQuery();
            return rows == 0 ? null : Get(id);
        }

        public bool Delete(int id)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand("DELETE FROM media WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateAvgScore(int id, double avgScore)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "UPDATE media SET avg_score = @s WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@s", avgScore);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public IEnumerable<Media> Filter(
            string? title = null,
            string? genre = null,
            MediaType? mediaType = null,
            int? releaseYear = null,
            int? ageRestriction = null,
            double? minRating = null,
            string? sortBy = null
        )
        {
            var conditions = new List<string>();
            if (!string.IsNullOrWhiteSpace(title)) conditions.Add("title ILIKE '%' || @title || '%'");
            if (!string.IsNullOrWhiteSpace(genre))
                conditions.Add("EXISTS (SELECT 1 FROM unnest(genres) g WHERE lower(g) = lower(@genre))");
            if (mediaType.HasValue) conditions.Add("media_type = @mediaType");
            if (releaseYear.HasValue) conditions.Add("release_year = @releaseYear");
            if (ageRestriction.HasValue) conditions.Add("age_restriction <= @ageRestriction");
            if (minRating.HasValue) conditions.Add("avg_score >= @minRating");

            var sql = "SELECT id, creator_user_id, title, description, media_type, release_year, genres, age_restriction, avg_score, created_at " +
                      "FROM media";
            if (conditions.Count > 0) sql += " WHERE " + string.Join(" AND ", conditions);

            sql += sortBy switch
            {
                "title" => " ORDER BY title",
                "year" => " ORDER BY release_year",
                "score" => " ORDER BY avg_score DESC",
                _ => ""
            };

            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@title", (object?)title ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@genre", (object?)genre ?? DBNull.Value);
            cmd.Parameters.AddWithValue(
                "@mediaType",
                mediaType.HasValue ? (object)(int)mediaType.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@releaseYear", (object?)releaseYear ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ageRestriction", (object?)ageRestriction ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@minRating", (object?)minRating ?? DBNull.Value);

            using var r = cmd.ExecuteReader();
            while (r.Read()) yield return Map(r);
        }

        private static Media Map(NpgsqlDataReader r)
            => new Media
            {
                Id = r.GetInt32(0),
                CreatorUserID = r.GetInt32(1),
                Title = r.GetString(2),
                Description = r.GetString(3),
                MediaType = (MediaType)r.GetInt32(4),
                ReleaseYear = r.GetInt32(5),
                Genres = FromDbGenres(r.GetValue(6)),
                AgeRestriction = r.GetInt32(7),
                AvgScore = r.GetDouble(8),
                CreatedAt = r.GetDateTime(9)
            };

        private static string[] ToDbGenres(List<string>? genres)
            => (genres ?? new List<string>()).ToArray();

        private static List<string> FromDbGenres(object value)
            => value is string[] arr ? arr.ToList() : new List<string>();
    }
}
