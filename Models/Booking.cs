using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SpecialistId { get; set; }

        [ForeignKey("SpecialistId")]
        public virtual User Specialist { get; set; }

        [Required]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual User Client { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public virtual ServiceModel Service { get; set; }

        [Required]
        public DateTime Day { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [NotMapped]
        public string TimeInterval
        {
            get => $"{StartTime}-{EndTime}";
            set
            {
                var parts = value.Split('-');
                if (parts.Length == 2 && TimeSpan.TryParse(parts[0], out var start) && TimeSpan.TryParse(parts[1], out var end))
                {
                    StartTime = start;
                    EndTime = end;
                }
                else
                {
                    throw new FormatException("Invalid TimeInterval format. Expected format: HH:mm-HH:mm");
                }
            }
        }
    }
}