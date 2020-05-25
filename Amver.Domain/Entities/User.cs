using System;
namespace Amver.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDay { get; set; }
        public int? GenderId { get; set; }
        public virtual City City { get; set; }
        public int? CityId { get; set; }
        public virtual Country Country { get; set; }
        public int? CountryId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsBanned { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
