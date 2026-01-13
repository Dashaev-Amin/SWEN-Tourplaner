using MRP.Model;
using MRP.Services;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Persistence
{
    public class PostgresUserRepository : IUserRepository
    {
        public bool Exists(string username)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT 1 FROM users WHERE username = @u LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@u", username);

            return cmd.ExecuteScalar() != null;
        }

        public void Add(User user)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "INSERT INTO users (username, password, display_name, bio, created) VALUES (@u, @p, @d, @b, @c)", conn);

            cmd.Parameters.AddWithValue("@u", user.Username);
            cmd.Parameters.AddWithValue("@p", user.Password);
            cmd.Parameters.AddWithValue("@d", (object?)user.DisplayName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@b", (object?)user.Bio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@c", user.Created);

            cmd.ExecuteNonQuery();
        }

        public User? Find(string username, string password)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT id, username, password, display_name, bio, created " +
                "FROM users WHERE username = @u AND password = @p LIMIT 1", conn);

            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                Id = r.GetInt32(0),
                Username = r.GetString(1),
                Password = r.GetString(2),
                DisplayName = r.IsDBNull(3) ? null : r.GetString(3),
                Bio = r.IsDBNull(4) ? null : r.GetString(4),
                Created = r.GetDateTime(5)
            };
        }
        public User? GetByUsername(string username)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT id, username, password, display_name, bio, created FROM users WHERE username = @u LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@u", username);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                Id = r.GetInt32(0),
                Username = r.GetString(1),
                Password = r.GetString(2),
                DisplayName = r.IsDBNull(3) ? null : r.GetString(3),
                Bio = r.IsDBNull(4) ? null : r.GetString(4),
                Created = r.GetDateTime(5)
            };
        }

        public User? GetById(int id)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "SELECT id, username, password, display_name, bio, created FROM users WHERE id = @id LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                Id = r.GetInt32(0),
                Username = r.GetString(1),
                Password = r.GetString(2),
                DisplayName = r.IsDBNull(3) ? null : r.GetString(3),
                Bio = r.IsDBNull(4) ? null : r.GetString(4),
                Created = r.GetDateTime(5)
            };
        }

        public User? UpdateProfile(int id, string? displayName, string? bio)
        {
            using var conn = DbConnectionFactory.CreateOpen();
            using var cmd = new NpgsqlCommand(
                "UPDATE users SET display_name = @d, bio = @b WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@d", (object?)displayName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@b", (object?)bio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);
            var rows = cmd.ExecuteNonQuery();
            return rows == 0 ? null : GetById(id);
        }


    }
}
