using System;
using System.Collections.Generic;
using Amver.Domain.Enums;
using Amver.Domain.StaticMembers;

namespace Amver.Domain.Models
{
    public class FilterTripRequest
    {
        public int FromCity { get; set; } = default;
        public int FromCountry { get; set; } = default;
        public int ToCity { get; set; } = default;
        public int ToCountry { get; set; } = default;
        public DateTime DateFrom { get; set; } = default;
        public int CompanionGender { get; set; } = default;

        public int UserId { get; set; }

        public string Login { get; set; }
            
        public List<Sort> Sort { get; set; }
        public Pagination Pagination { get; set; }
    }
}