namespace Amver.Domain.Entities
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Country Country { get; set; }
        public int CountryId { get; set; }
        public bool IsDeleted { get; set; }
        public string ruRu { get; set; }
    }
}
