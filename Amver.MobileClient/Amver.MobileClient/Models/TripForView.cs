using Xamarin.Forms;

namespace Amver.MobileClient.Models
{
    public class TripForView
    {
        public string UserName { get; set; }
        public string UserAge { get; set; }
        public string FromCountry { get; set; }
        public string FromCity { get; set; }
        public string ToCountry { get; set; }
        public string ToCity { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string PreferredGender { get; set; }
        public string CreatedDate { get; set; }
        public string Comment { get; set; }
        public ImageSource ImageSource { get; set; }
        public ImageSource UserImageSource { get; set; }
        public string UserLogin { get; set; }
        public int UserId { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCanModify { get; set; }
        public UriImageSource ImageUri { get; set; }
        public UriImageSource UserImageUri { get; set; }
    }
}