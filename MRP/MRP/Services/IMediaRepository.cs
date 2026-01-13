using MRP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Services
{
    public interface IMediaRepository
    {
        Media Add(Media m);
        Media? Get(int id);
        IEnumerable<Media> GetAll();
        IEnumerable<Media> Search(string? title = null);

        Media? Update(int id, Media updated);
        bool Delete(int id);
        bool UpdateAvgScore(int id, double avgScore);

        IEnumerable<Media> Filter(
            string? title = null,
            string? genre = null,
            MediaType? mediaType = null,
            int? releaseYear = null,
            int? ageRestriction = null,
            double? minRating = null,
            string? sortBy = null
        );
    }
}
