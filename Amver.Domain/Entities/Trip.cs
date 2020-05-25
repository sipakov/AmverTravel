using System;

namespace Amver.Domain.Entities
{
    public class Trip
    {
        public int Id { get; set; }
        public virtual City FromCity { get; set; }
        public int FromCityId { get; set; }
        public virtual Country FromCountry { get; set; }
        public int? FromCountryId { get; set; }
        public virtual City ToCity { get; set; }
        public int? ToCityId { get; set; }
        public virtual Country ToCountry { get; set; }
        public int ToCountryId { get; set; }
        public virtual User User { get; set; }
        public int UserId { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int PreferredGender { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }

        public bool IsBanned { get; set; }
    }
}
