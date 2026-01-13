using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Services
{
    public interface ITokenService
    {
        string Issue(string username);
        bool TryGetUser(string? bearerHeader, out string username);
    }
}
