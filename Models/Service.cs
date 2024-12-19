namespace TBLApi.Models
{
    public class ServiceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int SpecialistId { get; set; }

        // Связь с пользователем (специалистом)
        public User Specialist { get; set; }
    }
}
