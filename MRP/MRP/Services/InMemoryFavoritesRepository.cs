using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Services
{
    public class InMemoryFavoritesRepository : IFavoritesRepository
    {
        private readonly Dictionary<int, HashSet<int>> _favoritesByUser = new();

        public bool Add(int userId, int mediaId)
        {
            if (!_favoritesByUser.TryGetValue(userId, out var set))
            {
                set = new HashSet<int>();
                _favoritesByUser[userId] = set;
            }
            return set.Add(mediaId);
        }

        public bool Remove(int userId, int mediaId)
        {
            return _favoritesByUser.TryGetValue(userId, out var set) && set.Remove(mediaId);
        }

        public IEnumerable<int> GetMediaIds(int userId)
        {
            return _favoritesByUser.TryGetValue(userId, out var set)
                ? set
                : Enumerable.Empty<int>();
        }

        public bool IsFavorite(int userId, int mediaId)
        {
            return _favoritesByUser.TryGetValue(userId, out var set) && set.Contains(mediaId);
        }
    }
}
