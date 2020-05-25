using System;
using Amver.Domain.Entities;
using Xamarin.Forms;

namespace Amver.MobileClient.Models
{
    public class ProfileForView
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Photo { get; set; }
        public string FirstName { get; set; }
        public string BirthDay { get; set; }
        public string Age { get; set; }
        public int? GenderId { get; set; }
        public string Gender { get; set; }
        public virtual City City { get; set; }
        public int? CityId { get; set; }
        public string From { get; set; }
        public string Comment { get; set; }
        public string CreatedDate { get; set; }
        public bool IsBanned { get; set; }
        public bool IsDeleted { get; set; }
        public ImageSource UserImageSource { get; set; }
        public UriImageSource UserImageUri { get; set; }
    }
}