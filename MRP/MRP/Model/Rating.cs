using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            
        }

        public int Id { get; set; }
        public int MediaId { get; set; }
        public int UserId { get; set; }
        public int Stars { get; set; }

        public string? Comment { get; set; }
        public bool Confirmed { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

       


    }
}
