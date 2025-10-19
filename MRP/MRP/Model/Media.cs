using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Model
{
    public class Media
    {

        public Media() { }
        public Media(string title, MediaType type, int releaseYear) { 
          Title = title;
          MediaType = type;
          ReleaseYear = releaseYear;  
        
        }

        public int Id { get; set; }
        public int CreatorUserID { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public MediaType MediaType { get; set; }
        public int ReleaseYear { get; set; }
        public List<string> Genres { get; set; } = new();
        public int AgeRestriction { get; set; }

        public double AvgScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    


}
