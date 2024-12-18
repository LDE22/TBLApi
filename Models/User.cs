using System.ComponentModel.DataAnnotations.Schema;
namespace TBLApi.Models
{
    public class User
    {
        [Column("Id")]
        public int Id { get; set; }
        [Column("Username")]
        public required string Username { get; set; }
        [Column("Password")]
        public required string Password { get; set; }
        [Column("Email")]
        public required string Email { get; set; }
        [Column("Photo")]
        public string Photo { get; set; } = "default_avatar.png";
        [Column("LinkToProfile")]
        public string? LinkToProfile { get; set; }
        [Column("Name")]
        public string? Name { get; set; }
        [Column("City")]
        public string? City { get; set; } = "Город";
        [Column("Role")]
        public required string Role { get; set; }
        [Column("Description")]
        public string Description { get; set; }
    }
}
