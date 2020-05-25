namespace Amver.Domain.Dto
{
    public class BanDto
    {
        public int ObjectionableReasonId { get; set; }

        public int? ObjectionableUserId { get; set; }

        public int? ObjectionableTripId { get; set; }

        public int UserId { get; set; }

        public string Comment { get; set; }
    }
}