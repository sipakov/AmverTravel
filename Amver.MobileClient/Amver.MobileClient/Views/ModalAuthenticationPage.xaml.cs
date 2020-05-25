using System;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Models;
using Amver.MobileClient.Localization;
using Amver.MobileClient.Services;
using Amver.MobileClient.ViewModels;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModalAuthenticationPage
    {
        private readonly ModalAuthenticationViewModel _viewModel;

        public ModalAuthenticationPage(string from)
        {
            InitializeComponent();
            _viewModel = new ModalAuthenticationViewModel();
            BindingContext = _viewModel;
            MessagingCenter.Subscribe<object>(this, "EulaIsSuccessAgree", (sender) =>
            {
                Create();
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<object>(this, "ClosedModalAuthUserName", async (sender) =>
            {
                await Task.Delay(10);
                await Navigation.PopAsync();
            });
        }

        private void OnNewAccountButtonClicked(object sender, EventArgs e)
        {
            _viewModel.IsCreateAnAccountVisible = true;
        }
        private async void OnTermOfServiceButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new EulaPage(true));
        }
        
        private async void OnPrivacyPolicyButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ModalPrivacyPolicy());
        }

        private async void OnSignInButtonClicked(object sender, EventArgs e)
        {
            var (baseResult, content) = await _viewModel.GetTokenFromServer();

            if (baseResult.Result == StatusCode.Ok)
            {
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);

                var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out _);
                if (isSuccess)
                    Application.Current.Properties.Remove(AuthOptions.AccessTokenAmver);

                Application.Current.Properties.Add(AuthOptions.AccessTokenAmver, tokenResponse.Token);

                await Navigation.PopAsync();
            }
            else
            {
                _viewModel.IsSignInVisible = false;
                await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
            }
        }

        private void OnCreateAnAccountButtonClicked(object sender, EventArgs e)
        {                               
                Navigation.PushModalAsync(new EulaPage(false));             
        }

        private async void Create()
        {
            var (baseResult, content) = await _viewModel.GetRegistrationTokenFromServer();

            if (baseResult.Result == StatusCode.Ok)
            {
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);

                var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out _);
                if (isSuccess)
                    Application.Current.Properties.Remove(AuthOptions.AccessTokenAmver);

                Application.Current.Properties.Add(AuthOptions.AccessTokenAmver, tokenResponse.Token);
                await Navigation.PushAsync(new ModalAuthenticationUserNameAddPage());
            }
            else
            {
                _viewModel.IsSignInVisible = false;
                await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
            }
        }
    }
}