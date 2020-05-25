using System;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.MobileClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Amver.MobileClient.Localization;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TripPage
    {
        private readonly TripViewModel _viewModel;
        private readonly int _tripId;
        private const string Trip = "Trip";
        public TripPage(int tripId)
        {
            InitializeComponent();
            _viewModel = new TripViewModel(tripId);
            BindingContext = _viewModel;
            _tripId = tripId;
            MessagingCenter.Subscribe<object>(this, "ClosedModalAuthTrip", (sender) => {
                OnAppearing();
            });
        }

        private async void OnSendButtonClicked(object sender, EventArgs e)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess || (string.IsNullOrEmpty(bearerTokenObj.ToString())))    
            {
                await Navigation.PushAsync(new ModalAuthenticationPage(Trip));
            }
            else
            {
                await Navigation.PushAsync(new ChatPage(_tripId, _viewModel.Trip.UserId, Guid.Empty, _viewModel.Trip.UserName));
            }        
        }

        private async void OnHeartButtonClicked(object sender, EventArgs e)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess || (string.IsNullOrEmpty(bearerTokenObj.ToString())))
            {
                await Navigation.PushAsync(new ModalAuthenticationPage(Trip));
                return;
            }
           
            var (baseResult, content) = await _viewModel.HeartTapAsync(_tripId);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.FillHeartIfTapped(content);
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.IsBusy = false;
                    return;
                case StatusCode.Unauthorized:
                    _viewModel.IsBusy = false;
                    _viewModel.IsFavouriteTrip = !_viewModel.IsFavouriteTrip;
                    await Navigation.PushAsync(new ModalAuthenticationPage(Trip));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    _viewModel.IsBusy = false;
                    _viewModel.IsModalAuthWasOpen = true;
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }

        private async Task LoadTripData()
        {
            var (baseResult, content) = await _viewModel.LoadTripAsync(_tripId);

            switch (baseResult.Result)
            {
                case StatusCode.Ok: 
                    _viewModel.FillTrip(content);
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    await Navigation.PopAsync();

                    break;
            }
        }
        private const string Heart = "heart36dark.png";
        private async Task LoadIsFavouriteTripData()
        {
            var (baseResult, content) = await _viewModel.LoadIsFavouriteTripAsync(_tripId);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.FillHeart(content);
                    break;
                case StatusCode.Error when baseResult.Message == Messages.BadToken:
                case StatusCode.Unauthorized:
                    _viewModel.HeartStatus = Heart;
                    _viewModel.FavoriteStatus = String.Empty;
                    _viewModel.IsBusy = false;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }

        private async void OnUserNameButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ModalUserProfilePage(_viewModel.UserId));
        }
        
        private async void OnToolbarItemClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ModalObjectionableReasonsPage(null, _tripId));
        }

        protected override async void OnAppearing()
        {
            _viewModel.IsBusy = true;
            base.OnAppearing();
            var tasks = new[] {
                LoadTripData(),
                LoadIsFavouriteTripData()
            };
            await Task.WhenAll(tasks);
            _viewModel.IsBusy = false;
        }
    }
}