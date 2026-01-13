using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Model
{
    public class User
    {

        public User() { }

        public User(string username, string password) {

            Username = username;
            Password = password;    
        
        }

        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;


    }
}
