using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.MobileClient.Localization;
using Amver.MobileClient.Models;
using Amver.MobileClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FavoriteTripsPage
    {
        private readonly FavouriteTripListViewModel _viewModel;
        private const string Favourite = "Favourite";
        public FavoriteTripsPage()
        {
            InitializeComponent();

            _viewModel = new FavouriteTripListViewModel();
            BindingContext = _viewModel;
            MessagingCenter.Subscribe<object>(this, "ClosedModalAuthFavourite", (sender) => {
                OnAppearing();
            });
        }

        private async void OnListViewItemSelected(object sender, ItemTappedEventArgs e)
        {
            if (!(e.Item is TripForList selectedTrip))
                return;

            TripList.SelectedItem = null;
            var tripId = selectedTrip.Id;
            await Navigation.PushAsync(new TripPage(tripId));
        }

        private async Task LoadData()
        {
            var (baseResult, content) = await _viewModel.LoadFavouriteTripListAsync();

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.FillFavouriteTripLists(content);
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.FavouriteTrips = null;
                    _viewModel.IsBusy = false;
                    return;
                case StatusCode.Unauthorized:
                    await Navigation.PushAsync(new ModalAuthenticationPage(Favourite));
                    _viewModel.FavouriteTrips = null;
                    _viewModel.IsBusy = false;
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    _viewModel.FavouriteTrips = null;
                    _viewModel.IsBusy = false;
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
            _viewModel.IsBusy = false;
        }
    }
}