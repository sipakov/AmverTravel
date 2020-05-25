using System;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.MobileClient.Localization;
using Amver.MobileClient.Models;
using Amver.MobileClient.ViewModels;
using Xamarin.Forms;

namespace Amver.MobileClient.Views
{
    public partial class CabinetPage
    {
        private const string Cabinet = "Cabinet";
        private readonly CabinetViewModel _viewModel;
        public CabinetPage()
        {
            InitializeComponent();
            _viewModel = new CabinetViewModel();
            BindingContext = _viewModel;
        }
        private async void OnMyTripListViewItemSelected(object sender, ItemTappedEventArgs e)
        {
            if (!(e.Item is TripForList selectedTrip))
                return;

            MyTripList.SelectedItem = null;
            var tripId = selectedTrip.Id;
            await Navigation.PushAsync(new MyTripPage(tripId));
        }

        private async void OnUserLoginButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage());
        }

        private async Task LoadData()
        {
            var (baseResult, content) = await _viewModel.LoadTripListAsync();

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.FillTripLists(content);
                    MyTravelsLabel.IsVisible = true;
                    LoginLogoutFrame.IsVisible = false;
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.IsBusy = false;
                    _viewModel.IsMyTripsNotEmpty = false;
                    _viewModel.IsCompletedNotEmpty = false;
                    _viewModel.IsInCompletedNotEmpty = false;
                    _viewModel.ActiveTrips = null;
                    _viewModel.CompletedTrips = null;
                    _viewModel.UserName = string.Empty;
                    _viewModel.IsLogin = false;
                    _viewModel.UserImageUri = null;
                    _viewModel.LoginLogout = AppResources.LogInButtonTitle;
                    _viewModel.MyTrips = null;
                    MyTravelsLabel.IsVisible = false;
                    LoginLogoutFrame.IsVisible = true;
                    return;
                case StatusCode.Unauthorized:
                    _viewModel.IsBusy = false;
                    _viewModel.IsMyTripsNotEmpty = false;
                    _viewModel.IsCompletedNotEmpty = false;
                    _viewModel.IsInCompletedNotEmpty = false;
                    _viewModel.ActiveTrips = null;
                    _viewModel.CompletedTrips = null;
                    _viewModel.UserName = string.Empty;
                    _viewModel.IsLogin = false;
                    _viewModel.UserImageUri = null;
                    _viewModel.LoginLogout = AppResources.LogInButtonTitle;
                    _viewModel.IsModalAuthWasOpen = true;
                    _viewModel.MyTrips = null;
                    MyTravelsLabel.IsVisible = false;
                    LoginLogoutFrame.IsVisible = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    _viewModel.IsBusy = false;
                    _viewModel.IsMyTripsNotEmpty = false;
                    _viewModel.IsCompletedNotEmpty = false;
                    _viewModel.IsInCompletedNotEmpty = false;
                    _viewModel.ActiveTrips = null;
                    _viewModel.CompletedTrips = null;
                    _viewModel.UserName = string.Empty;
                    _viewModel.IsLogin = false;
                    _viewModel.UserImageUri = null;
                    _viewModel.LoginLogout = AppResources.LogInButtonTitle;
                    _viewModel.MyTrips = null;
                    break;
            }
        }

        private async Task LoadUserName()
        {
            var (baseResult, content) = await _viewModel.LoadUserNameAsync();

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.IsLogin = true;
                    _viewModel.LoginLogout = AppResources.LogOutButtonTitle;
                    _viewModel.FillUserName(content);
                    _viewModel.IsBusy = false;
                    MyTravelsLabel.IsVisible = true;
                    LoginLogoutFrame.IsVisible = false;
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.IsBusy = false;
                    _viewModel.UserName = string.Empty;
                    MyTravelsLabel.IsVisible = false;
                    LoginLogoutFrame.IsVisible = true;
                    return;
                case StatusCode.Unauthorized:
                    _viewModel.IsBusy = false;
                    _viewModel.UserName = string.Empty;
                    _viewModel.IsModalAuthWasOpen = true;
                    MyTravelsLabel.IsVisible = false;
                    LoginLogoutFrame.IsVisible = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    _viewModel.IsBusy = false;
                    break;
            }
        }
        private async void OnEulaButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new EulaPage(true));
        }

        private async void OnPrivacyPolicyButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new ModalPrivacyPolicy());
        }

        private async void OnLoginLogoutButtonClicked(object sender, EventArgs e)
        {
            if (_viewModel.IsLogin)
            {
                await _viewModel.SignOut();
                OnAppearing();
            }
            else
            {
                
                await Navigation.PushAsync(new ModalAuthenticationPage(Cabinet));
            }
        }

        protected override async void OnAppearing()
        {
            _viewModel.IsBusy = true;
            base.OnAppearing();
            var tasks = new[] { LoadUserName(), LoadData() };
            await Task.WhenAll(tasks);
        }
    }
}
