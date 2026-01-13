using MRP.Model;

namespace MRP.Services
{
    public class InMemoryRatingRepository : IRatingRepository
    {
        private readonly Dictionary<int, Rating> _ratings = new();
        private int _nextId = 1;

        // Key = (mediaId,userId) => ratingId, to enforce "one rating per user per media"
        private readonly Dictionary<(int mediaId, int userId), int> _byMediaUser = new();

        public Rating Upsert(int mediaId, int userId, int stars, string? comment)
        {
            if (_byMediaUser.TryGetValue((mediaId, userId), out var ratingId))
            {
                var existing = _ratings[ratingId];
                existing.Stars = stars;
                existing.Comment = comment;
                existing.Confirmed = string.IsNullOrWhiteSpace(comment);
                existing.UpdatedAt = DateTime.Now;
                return existing;
            }

            var r = new Rating(mediaId, userId, stars, comment)
            {
                Id = _nextId++,
                Created = DateTime.Now,
                Confirmed = string.IsNullOrWhiteSpace(comment) // if no comment => nothing to confirm
            };

            _ratings[r.Id] = r;
            _byMediaUser[(mediaId, userId)] = r.Id;
            return r;
        }

        public Rating? Get(int ratingId)
            => _ratings.TryGetValue(ratingId, out var r) ? r : null;

        public IEnumerable<Rating> GetByMedia(int mediaId)
            => _ratings.Values.Where(r => r.MediaId == mediaId);

        public IEnumerable<Rating> GetByUser(int userId)
            => _ratings.Values.Where(r => r.UserId == userId);

        public bool Delete(int ratingId, int userId)
        {
            if (!_ratings.TryGetValue(ratingId, out var r)) return false;
            if (r.UserId != userId) return false;

            _ratings.Remove(ratingId);
            _byMediaUser.Remove((r.MediaId, r.UserId));
            return true;
        }

        public bool Like(int ratingId, int userId)
        {
            if (!_ratings.TryGetValue(ratingId, out var r)) return false;
            if (r.UserId == userId) return false; // cannot like own rating
            return r.LikedByUserIds.Add(userId);  // false if already liked
        }

        public bool Confirm(int ratingId, int userId)
        {
            if (!_ratings.TryGetValue(ratingId, out var r)) return false;
            if (r.UserId != userId) return false;

            r.Confirmed = true;
            r.UpdatedAt = DateTime.Now;
            return true;
        }

        public IEnumerable<Rating> GetAll()
    => _ratings.Values;

    }
}
