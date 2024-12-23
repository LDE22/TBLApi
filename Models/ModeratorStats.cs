namespace TBLApi.Models
{
    public class ModeratorStatistic
    {
        public int Id { get; set; }
        public int ModeratorId { get; set; } // ID модератора (UserId)
        public int ClosedTickets { get; set; } = 0; // Количество закрытых тикетов
        public int BlockedProfiles { get; set; } = 0; // Количество заблокированных профилей
        public int RestrictedProfiles { get; set; } = 0; // Количество профилей с ограничением
        public int RejectedTickets { get; set; } = 0; // Количество отклонённых тикетов
    }

}
