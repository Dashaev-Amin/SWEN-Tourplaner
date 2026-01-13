using MRP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Services
{
    public interface IUserRepository
    {
        bool Exists(string username);
        void Add(User user);
        User? Find(string username, string password);
        User? GetByUsername(string username);

        User? GetById(int id);
        User? UpdateProfile(int id, string? displayName, string? bio);

    }
}
