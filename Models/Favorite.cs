using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TBLApi.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        [Required] // Поле ClientId обязательно
        public int ClientId { get; set; }

        [Required] // Поле ServiceId обязательно
        public int ServiceId { get; set; }

        // Навигационные свойства. НЕ делайте их [Required], если вы отправляете только идентификаторы.
        [ForeignKey(nameof(ClientId))]
        public User Client { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public ServiceModel Service { get; set; }
    }

}
