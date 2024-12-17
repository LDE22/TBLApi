namespace TBLApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public string Photo { get; set; } = "default_avatar.png";
        public string? LinkToProfile { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; } = "Город"; 
        public required string Role { get; set; }
    }
}
