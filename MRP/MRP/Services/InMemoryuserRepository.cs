using MRP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Services
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();
        public bool Exists(string username) => _users.Any(u => u.Username == username);
        public void Add(User user) { user.Id = _users.Count + 1; _users.Add(user); }
        public User? Find(string username, string password) =>
            _users.FirstOrDefault(u => u.Username == username && u.Password == password);
        public User? GetByUsername(string username) =>
    _users.FirstOrDefault(u => u.Username == username);

        public User? GetById(int id) =>
    _users.FirstOrDefault(u => u.Id == id);

        public User? UpdateProfile(int id, string? displayName, string? bio)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null) return null;
            user.DisplayName = displayName;
            user.Bio = bio;
            return user;
        }

    }
}
