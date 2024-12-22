using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TBLApi.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int ServiceId { get; set; }

        // Связь с услугой
        [ForeignKey(nameof(ClientId))]
        public User Client { get; set; } // Связь с таблицей Users

        [ForeignKey(nameof(ServiceId))]
        public ServiceModel Service { get; set; } // Связь с таблицей Services
    }
}
