using System;
using System.Collections.Generic;
using System.Linq;
using Genders = Amver.Domain.StaticMembers.Genders;

namespace Amver.MobileClient.ViewModels
{
    public class ModalAuthenticationBirthDayAddViewModel
    {
        public DateTime CurrentDate { get; set; } = DateTime.UtcNow;

        public DateTime MaximumDate { get; set; } = DateTime.UtcNow.AddYears(-18);
        
        public DateTime MinimumDate { get; set; } = DateTime.UtcNow.AddYears(-100);
        
        public List<string> UserGenders { get; set; }
        
        public string UserGender { get; set; }
        
        public ModalAuthenticationBirthDayAddViewModel()  
        {
            UserGenders = Genders.GenderList.Values.Take(2).ToList();
            UserGender = UserGenders.FirstOrDefault();
        }
    }
}