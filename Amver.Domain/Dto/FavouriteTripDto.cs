namespace Amver.Domain.Dto
{
    public class FavouriteTripDto
    {
        public int UserId { get; set; }
        public int TripId { get; set; }
        public bool IsFavourite { get; set; }
    }
}