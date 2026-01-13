using MRP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace MRP.Services
{
    public class InMemoryMediaRepository : IMediaRepository
    {
        private readonly Dictionary<int, Media> _store = new();
        private int _nextId = 1;

        public Media Add(Media m)
        {
            m.Id = _nextId++;
            _store[m.Id] = m;
            return m;
        }

        public Media? Get(int id)
            => _store.TryGetValue(id, out var m) ? m : null;

        public IEnumerable<Media> GetAll()
            => _store.Values;

        public IEnumerable<Media> Search(string? title = null)
            => _store.Values.Where(m =>
                string.IsNullOrWhiteSpace(title) ||
                m.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

        public Media? Update(int id, Media updated)
        {
            if (!_store.TryGetValue(id, out var existing))
                return null;

            existing.Title = updated.Title;
            existing.Description = updated.Description;
            existing.MediaType = updated.MediaType;
            existing.ReleaseYear = updated.ReleaseYear;
            existing.Genres = updated.Genres;
            existing.AgeRestriction = updated.AgeRestriction;

            return existing;
        }

        public bool Delete(int id)
            => _store.Remove(id);

        public bool UpdateAvgScore(int id, double avgScore)
        {
            if (!_store.TryGetValue(id, out var m)) return false;
            m.AvgScore = avgScore;
            return true;
        }

        public IEnumerable<Media> Filter(
            string? title = null,
            string? genre = null,
            MediaType? mediaType = null,
            int? releaseYear = null,
            int? ageRestriction = null,
            double? minRating = null,
            string? sortBy = null
        )
        {
            IEnumerable<Media> q = _store.Values;

            if (!string.IsNullOrWhiteSpace(title))
                q = q.Where(m => m.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(genre))
                q = q.Where(m => m.Genres.Any(g =>
                    g.Equals(genre, StringComparison.OrdinalIgnoreCase)));

            if (mediaType.HasValue)
                q = q.Where(m => m.MediaType == mediaType.Value);

            if (releaseYear.HasValue)
                q = q.Where(m => m.ReleaseYear == releaseYear.Value);

            if (ageRestriction.HasValue)
                q = q.Where(m => m.AgeRestriction <= ageRestriction.Value);

            if (minRating.HasValue)
                q = q.Where(m => m.AvgScore >= minRating.Value);

            q = sortBy switch
            {
                "title" => q.OrderBy(m => m.Title),
                "year" => q.OrderBy(m => m.ReleaseYear),
                "score" => q.OrderByDescending(m => m.AvgScore),
                _ => q
            };

            return q;
        }
    }
}
