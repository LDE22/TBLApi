using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public User Client { get; set; } // Уберите [Required], если есть
        public ServiceModel Service { get; set; } // Уберите [Required], если есть
    }

}
