namespace TBLApi.Models
{
    public class ChatPreview
    {
        public int ChatId { get; set; }
        public string Name { get; set; } // Имя пользователя
        public string LastMessage { get; set; }
        public DateTime Timestamp { get; set; }
        public int TargetUserId { get; set; }
    }
}
