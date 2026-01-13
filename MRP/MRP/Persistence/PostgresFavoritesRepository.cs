using MRP.Services;
using Npgsql;

namespace MRP.Persistence
{
    public class PostgresFavoritesRepository : IFavoritesRepository
    {
        public bool Add(int userId, int mediaId)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO favorites (user_id, media_id) VALUES (@u, @m) ON CONFLICT DO NOTHING", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", mediaId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Remove(int userId, int mediaId)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "DELETE FROM favorites WHERE user_id = @u AND media_id = @m", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", mediaId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public IEnumerable<int> GetMediaIds(int userId)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT media_id FROM favorites WHERE user_id = @u", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) yield return r.GetInt32(0);
        }

        public bool IsFavorite(int userId, int mediaId)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT 1 FROM favorites WHERE user_id = @u AND media_id = @m", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", mediaId);
            return cmd.ExecuteScalar() != null;
        }
    }
}
