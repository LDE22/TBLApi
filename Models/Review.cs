using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; }
        public int ClientId { get; set; }
        public string Content { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User Specialist { get; set; }
        public virtual User Client { get; set; }

    }
}
