using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;
using Genders = Amver.Domain.StaticMembers.Genders;

namespace Amver.MobileClient.ViewModels
{
    public class ModalAuthenticationEmailAddViewModel
    {
        private readonly INetwork _network;
        public string Email { get; set; }

        public ModalAuthenticationEmailAddViewModel()
        {
            _network = App.Container.Resolve<INetwork>();
        }
        
        public async Task<(BaseResult baseResult, string content)> LoadFinishedRegistrationResultAsync(string userName, MailAddress mailAddress, DateTime birthDateTime, string userGender)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", nameof(userName));
            
            var fullUrl = $"{Url.GetFinishedRegistrationResult}";
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);

            var genderId = Genders.GenderList.First(x=>x.Value == userGender).Key;

            var userDto = new UserDto { Email = mailAddress.ToString(), BirthDay = birthDateTime, FirstName = userName, GenderId = genderId};
            
            var serializedUser = JsonConvert.SerializeObject(userDto);
            
            var result = await _network.LoadDataPostAsync(fullUrl, serializedUser, bearerToken);
            return result;
        }
    }
}