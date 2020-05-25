using System.Collections.Generic;
using Amver.Domain.Dto;

namespace Amver.Domain.Models
{
    public class FavouriteTripResponse
    {
        public IEnumerable<FavouriteTripForListDto> Trips { get; set; }
        public int Count { get; set; }
    }
}