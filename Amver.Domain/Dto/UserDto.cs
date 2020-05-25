using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amver.Domain.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDay { get; set; }
        public int GenderId { get; set; }
        public string City { get; set; }
        public int? CityId { get; set; }
        public int CountryId { get; set; }
        public string Country { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsBanned { get; set; }
        public bool IsDeleted { get; set; }
        public byte[] UserImage { get; set; }
        public string UserImageUri { get; set; }

        //public bool IsConfirmedEmail { get; set; }
        
        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as UserDto);
        }

        private bool Equals(UserDto obj)
        {
            return obj != null && obj.Login == Login && obj.IsBanned == IsBanned && obj.CreatedDate == CreatedDate &&
                   obj.Email == Email && obj.GenderId == GenderId && obj.BirthDay == BirthDay &&
                   obj.FirstName == FirstName;
        }
    }
}
