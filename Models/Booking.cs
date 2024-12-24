using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TBLApi.Models;

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
    public DateOnly Day { get; set; }

    public string TimeInterval { get; set; } // Удалено [NotMapped]

    [NotMapped] // Только для внутреннего использования
    public TimeSpan StartTime
    {
        get => TimeSpan.Parse(TimeInterval.Split('-')[0]);
        set
        {
            var parts = TimeInterval?.Split('-');
            TimeInterval = $"{value}-{parts?[1]}";
        }
    }

    [NotMapped] // Только для внутреннего использования
    public TimeSpan EndTime
    {
        get => TimeSpan.Parse(TimeInterval.Split('-')[1]);
        set
        {
            var parts = TimeInterval?.Split('-');
            TimeInterval = $"{parts?[0]}-{value}";
        }
    }
}
