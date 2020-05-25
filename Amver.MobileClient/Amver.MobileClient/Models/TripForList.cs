using Amver.MobileClient.Views;
using Xamarin.Forms;

namespace Amver.MobileClient.Models
{
    public class TripForList
    {
        public int Id { get; set; }
        public string FromCity { get; set; }
        public string ToCity { get; set; }
        public string UserName { get; set; }
        public string DateFromDateTo { get; set; }
        public ImageSource ImageSource { get; set; }
        public ImageSource UserImageSource { get; set; }
        public string Status { get; set; }
        public UriImageSource ImageUri { get; set; }
        public UriImageSource UserImageUri { get; set; }
    }
}