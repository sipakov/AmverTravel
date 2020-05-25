using System;
using System.Net.Mail;
using Amver.Domain.Constants;
using Amver.MobileClient.Localization;
using Amver.MobileClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModalAuthenticationEmailAddPage : ContentPage
    {
        private readonly ModalAuthenticationEmailAddViewModel _viewModel;
        private readonly DateTime _birthDateTime;
        private readonly string _userName;
        private readonly string _userGender;
        public ModalAuthenticationEmailAddPage(string userName, DateTime birthDateTime, string userGender)
        {
            InitializeComponent();
            _viewModel = new ModalAuthenticationEmailAddViewModel();
            BindingContext = _viewModel;
            _birthDateTime = birthDateTime;
            _userName = userName;
            _userGender = userGender;
        }

        private async void OnFinishButtonClicked(object sender, EventArgs e)
        {
            var validEmail = GetValidEmail(_viewModel.Email);
            if (validEmail == null)
            {
                await DisplayAlert(AppResources.Notification, AppResources.EmailIsNotValid , AppResources.Ok);
            }
            else
            {
                var (baseResult, _) = await _viewModel.LoadFinishedRegistrationResultAsync(_userName, validEmail, _birthDateTime, _userGender);
                
                if (baseResult.Result == StatusCode.Ok)
                {                 
                     MessagingCenter.Send<object>(this, "ClosedModalAuthUserEmail");
                     await Navigation.PopAsync();                  
                }
                else
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
            }
        }
        
        private static MailAddress GetValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress;
            }
            catch
            {
                return null;
            }
        }
    }
}