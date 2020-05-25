using System;
using Xamarin.Forms;

namespace Amver.MobileClient.Models
{
    public class ConversationForList
    {
        public Guid Id { get; set; }
        public int TripId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FromCity { get; set; }
        public string ToCity { get; set; }
        public string DateFromDateTo { get; set; }
        public ImageSource ImageSource { get; set; }
        public string LastMessage { get; set; }
        public UriImageSource ImageUri { get; set; }
        public bool IsNewMessage { get; set; }
    }
}