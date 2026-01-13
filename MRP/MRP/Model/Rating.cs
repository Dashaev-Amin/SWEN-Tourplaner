using System;
using System.Collections.Generic;

namespace MRP.Model
{
    public class Rating
    {
        public Rating() { }

        public Rating(int mediaId, int userId, int stars, string? comment)
        {
            MediaId = mediaId;
            UserId = userId;
            Stars = stars;
            Comment = comment;
            Confirmed = string.IsNullOrWhiteSpace(comment);
            Created = DateTime.Now;
        }

        public int Id { get; set; }
        public int MediaId { get; set; }
        public int UserId { get; set; }
        public int Stars { get; set; }

        public string? Comment { get; set; }
        public bool Confirmed { get; set; }

        public DateTime Created { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Enforces "1 like per user per rating"
        public HashSet<int> LikedByUserIds { get; set; } = new();
    }
}
