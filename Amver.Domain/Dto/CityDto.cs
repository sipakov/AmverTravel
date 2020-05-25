using Amver.Domain.Entities;

namespace Amver.Domain.Dto
{
    public class CityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Country Country { get; set; }
        public string Localization { get; set; }
    }
}