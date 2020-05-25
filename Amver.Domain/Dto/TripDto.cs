using System;

namespace Amver.Domain.Dto
{
    public class TripDto
    {
        public int Id { get; set; }
        public int? FromCityId { get; set; }
        public string FromCity { get; set; }
        public int? FromCountryId { get; set; }
        public string FromCountry { get; set; }
        public int? ToCityId { get; set; }
        public string ToCity { get; set; }
        public int? ToCountryId { get; set; }
        public string ToCountry { get; set; }
        public int UserId { get; set; }
        public string UserFirsName { get; set; }
        public string UserLastName { get; set; }
        public DateTime? UserBirthDay { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int PreferredGender { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLiked { get; set; }
        public byte[] Image { get; set; }
        public byte[] UserImage { get; set; }
        public string UserLogin { get; set; }
        public bool IsCompleted { get; set; }

        public string ImageUri { get; set; }
        
        public string UserImageUri { get; set; }
    }
}