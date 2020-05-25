using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Amver.Domain.Constants;
using Amver.Domain.Enums;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Localization;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Amver.MobileClient.ViewModels
{
    public class ModalAuthenticationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly INetwork _network;

        public string LoginRegistration { get; set; }
        
        public string PasswordOne { get; set; }

        public string PasswordTwo { get; set; }

        private bool _isSignInVisible;
        public bool IsSignInVisible
        {
            get => _isSignInVisible;
            set
            {
                _isSignInVisible = value;
                OnPropertyChanged(nameof(IsSignInVisible));
            }
        }

        private bool _isCreateAnAccountVisible;
        public bool IsCreateAnAccountVisible
        {
            get => _isCreateAnAccountVisible;
            set
            {
                _isCreateAnAccountVisible = value;
                OnPropertyChanged(nameof(IsCreateAnAccountVisible));
            }
        }


        public string Login { get; set; }

        public string Password { get; set; }

        public ModalAuthenticationViewModel()
        {
            _network = App.Container.Resolve<INetwork>();
        }

        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public async Task<(BaseResult baseResult, string content)> GetTokenFromServer()
        {
            if(string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Login))
                return (new BaseResult {Result = StatusCode.Error, Message = AppResources.FieldIsAnEmpty}, null);
            if(!IsValidUserName(Login))
                return (new BaseResult {Result = StatusCode.Error, Message = AppResources.LoginIsNotValid}, null);
            if(!IsValidPassword(Password))
                return (new BaseResult {Result = StatusCode.Error, Message = AppResources.PasswordIsNotValid }, null);
            var isSuccessFcmToken = Application.Current.Properties.TryGetValue(AuthOptions.FcmTokenAmver, out var fcmTokenAmver);

           
            var userCredential = new UserCredential
            {
                Login = Login,
                UserPassword = Password,
                FcmToken = isSuccessFcmToken ? fcmTokenAmver.ToString() : null
            };
            if (Device.RuntimePlatform == Device.iOS)
            {
                userCredential.Os = (int)OperationSystems.iOs;
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                userCredential.Os = (int)OperationSystems.Android;
            }

            var serializedUserCredential = JsonConvert.SerializeObject(userCredential);
            
            var result = await _network.LoadDataPostAsync(Url.GetTokenByClassicLogin,serializedUserCredential, null);
            return result;
        }
        
        public async Task<(BaseResult baseResult, string content)> GetRegistrationTokenFromServer()
        {
            if(string.IsNullOrWhiteSpace(LoginRegistration) || string.IsNullOrWhiteSpace(PasswordOne) || string.IsNullOrWhiteSpace(PasswordTwo))
                return (new BaseResult {Result = StatusCode.Error, Message = AppResources.FieldIsAnEmpty }, null);
            if (string.IsNullOrEmpty(PasswordOne) || string.IsNullOrEmpty(PasswordTwo))
                return (new BaseResult { Result = StatusCode.Error, Message = AppResources.PasswordIsNotValid }, null);

            var isPasswordsAreNotEqual = PasswordOne != PasswordTwo;

            if (isPasswordsAreNotEqual)
                return (new BaseResult {Result = StatusCode.Error, Message = AppResources.PasswordsAreNotEqual}, null);
            if(!IsValidUserName(LoginRegistration))
                return (new BaseResult {Result = StatusCode.Error, Message = AppResources.LoginIsNotValid }, null);
            if(!IsValidPassword(PasswordOne))
                return (new BaseResult {Result = StatusCode.Error, Message = AppResources.PasswordIsNotValid }, null);
            var isSuccessFcmToken = Application.Current.Properties.TryGetValue(AuthOptions.FcmTokenAmver, out var fcmTokenAmver);

            var userCredential = new UserCredential
            {
                Login = LoginRegistration,
                UserPassword = PasswordOne,
                FcmToken = isSuccessFcmToken ? fcmTokenAmver.ToString() : null
            };

            if (Device.RuntimePlatform == Device.iOS)
            {
                userCredential.Os = (int)OperationSystems.iOs;
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                userCredential.Os = (int)OperationSystems.Android;
            }

            var serializedUserCredential = JsonConvert.SerializeObject(userCredential);
            
            var result = await _network.LoadDataPostAsync(Url.GetTokenByClassicTokenRegistration,serializedUserCredential, null);
            return result;
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", nameof(password));

            return Regex.IsMatch(password, AuthOptions.PatternToValidatePassword);
        }
        private bool IsValidUserName(string login)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentException("Value cannot be null or empty.", nameof(login));

            return Regex.IsMatch(login, AuthOptions.PatternToValidateLogin);
        }
    }
}