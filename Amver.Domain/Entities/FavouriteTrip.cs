namespace Amver.Domain.Entities
{
    public class FavouriteTrip
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int TripId { get; set; }
        public Trip Trip { get; set; }
        public bool IsFavourite { get; set; }
    }
}