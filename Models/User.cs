using System.ComponentModel.DataAnnotations.Schema;
namespace TBLApi.Models
{
    public class User
    {
        private static readonly string DefaultAvatarBase64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...";
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? PhotoBase64 { get; set; } = DefaultAvatarBase64;
        public string? LinkToProfile { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; } = "Город";
        public string Role { get; set; }
        public string? Description { get; set; }
        public bool IsEmailConfirmed { get; set; }=false;
        public string? EmailConfirmationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiration { get; set; }
        public static string GetDefaultAvatarBase64() => DefaultAvatarBase64;
    }
}
