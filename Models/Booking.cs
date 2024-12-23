using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int SpecialistId { get; set; }
        public int ClientId { get; set; }
        public int ServiceId { get; set; } // Внешний ключ для услуги
        public DateTime Day { get; set; }
        public string TimeInterval { get; set; }

        // Связь с услугой
        public virtual ServiceModel Service { get; set; }

        public virtual User Client { get; set; }
    }

}
