namespace TBLApi.Models
{
    public class ActionLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public int? RecordId { get; set; }
        public DateTime Timestamp { get; set; }

        public User User { get; set; }
    }
}
