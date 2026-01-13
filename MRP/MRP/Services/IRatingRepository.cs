using MRP.Model;

namespace MRP.Services
{
    public interface IRatingRepository
    {
        Rating Upsert(int mediaId, int userId, int stars, string? comment);
        Rating? Get(int ratingId);
        IEnumerable<Rating> GetByMedia(int mediaId);
        IEnumerable<Rating> GetByUser(int userId);
        bool Delete(int ratingId, int userId);

        bool Like(int ratingId, int userId);   // returns false if already liked or own rating
        bool Confirm(int ratingId, int userId); // only author can confirm comment

        IEnumerable<Rating> GetAll();

    }
}
