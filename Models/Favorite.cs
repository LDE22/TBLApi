using TBLApi.Controllers;

namespace TBLApi.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int ServiceId { get; set; }

        public ServiceModel Service { get; set; }
    }
}
