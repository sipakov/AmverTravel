using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.MobileClient.Localization;
using Amver.MobileClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModalAuthenticationUserNameAddPage : ContentPage
    {
        private readonly ModalAuthenticationUserNameAddViewModel _viewModel;
        public ModalAuthenticationUserNameAddPage()
        {
            InitializeComponent();
            _viewModel = new ModalAuthenticationUserNameAddViewModel();
            BindingContext = _viewModel;           
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<object>(this, "ClosedModalAuthUserBirthday", async (sender) =>
            {
                await Task.Delay(10);
                MessagingCenter.Send<object>(this, "ClosedModalAuthUserName");
               await Navigation.PopAsync();                
            });
        }

        private async void OnNextButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.UserName))
            {
                await DisplayAlert(AppResources.Notification, AppResources.FieldIsAnEmpty, AppResources.Ok);
            }
            else if (!IsValidUserName(_viewModel.UserName))
            {
                await DisplayAlert(AppResources.Notification, AppResources.UserNameIsNotValid, AppResources.Ok);
            }
            else if (IsValidUserName(_viewModel.UserName))
            {
                _viewModel.UserName = _viewModel.UserName.Trim();
                await Navigation.PushAsync(new ModalAuthenticationBirthDayAddPage(_viewModel.UserName));              
            }
        }
        
        private static bool IsValidUserName(string login)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentException("Value cannot be null or empty.", nameof(login));

            return Regex.IsMatch(login, AuthOptions.PatternToValidateName);
        }
    }
}