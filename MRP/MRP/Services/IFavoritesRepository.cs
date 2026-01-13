using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Services
{
    public interface IFavoritesRepository
    {
        bool Add(int userId, int mediaId);      // false if already favorite
        bool Remove(int userId, int mediaId);   // false if not existing
        IEnumerable<int> GetMediaIds(int userId);
        bool IsFavorite(int userId, int mediaId);
    }
}

