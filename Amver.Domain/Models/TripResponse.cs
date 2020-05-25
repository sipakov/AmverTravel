using System.Collections.Generic;
using Amver.Domain.Dto;

namespace Amver.Domain.Models
{
    public class TripResponse
    {
        public IEnumerable<TripForListDto> Trips { get; set; }
        public int Count { get; set; }
    }
}