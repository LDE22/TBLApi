namespace TBLApi.Models
{
    public class LocationUpdateRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
    }
}
