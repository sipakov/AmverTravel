using System;

namespace Amver.Domain.Dto
{
    public class FavouriteTripForListDto
    {
        public int TripId { get; set; }
        public string FromCity { get; set; }
        public string FromCountry { get; set; }
        public string ToCity { get; set; }
        public string ToCountry { get; set; }
        public string UserName { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public byte[] Image { get; set; }
        public byte[] UserImage { get; set; }
        public string ImageUri { get; set; }
        public string UserImageUri { get; set; }
    }
}