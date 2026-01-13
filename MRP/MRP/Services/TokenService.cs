using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Services
{
    public class TokenService : ITokenService
    {
        private readonly Dictionary<string, string> _tokens = new(); // token -> username
        public string Issue(string username)
        {
            var token = $"{username}-mrpToken";
            _tokens[token] = username;
            return token;
        }
        public bool TryGetUser(string? bearerHeader, out string username)
        {
            username = "";
            if (string.IsNullOrWhiteSpace(bearerHeader)) return false;
            var parts = bearerHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase)) return false;
            return _tokens.TryGetValue(parts[1], out username);
        }
    }
}
