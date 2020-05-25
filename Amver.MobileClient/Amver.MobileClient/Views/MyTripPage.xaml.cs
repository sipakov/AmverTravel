using System;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.MobileClient.Localization;
using Amver.MobileClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyTripPage : ContentPage
    {
        private readonly MyTripViewModel _viewModel;
        private readonly int _tripId;
        private const string MyTrip = "MyTrip";
        public MyTripPage(int tripId)
        {
            InitializeComponent();
            
            _viewModel = new MyTripViewModel();
            BindingContext = _viewModel;
            _tripId = tripId;
        }
        private async void OnUpdateButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UpdateTripPage(_tripId));
        }
        
        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert(AppResources.ConfirmAction, AppResources.QuestionDoYouWantToDeleteTheTravel, AppResources.ConfirmActionYes, AppResources.ConfirmActionNo);

            if (result)
            {
              await RemoveTripData(); 
            }
        }
        private async Task LoadTripData()
        {
            var (baseResult, content) = await _viewModel.LoadTripAsync(_tripId);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.FillTrip(content);
                    _viewModel.IsBusy = false;
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    return;
                case StatusCode.Unauthorized:
                    await Navigation.PushAsync(new ModalAuthenticationPage(MyTrip));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }
        
        private async Task RemoveTripData()
        {
            var (baseResult, _) = await _viewModel.RemoveTripAsync(_tripId);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    await DisplayAlert(AppResources.Notification, AppResources.TravelSuccessfullyDeleted, AppResources.Ok);
                    await Navigation.PopAsync();
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    return;
                case StatusCode.Unauthorized:
                    await Navigation.PushAsync(new ModalAuthenticationPage(MyTrip));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }
        private async void OnCompleteButtonClicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert(AppResources.ConfirmAction, AppResources.QuestionDidTheTravelTakePlace, AppResources.ConfirmActionYes, AppResources.ConfirmActionNo);
            if (result) await CompleteTripData();
        }
        
        private async Task CompleteTripData()
        {
            var (baseResult, _) = await _viewModel.CompleteFileAsync(_tripId);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    await DisplayAlert(AppResources.Notification, AppResources.TravelSuccessfullyCompleted, AppResources.Ok);
                    await Navigation.PopAsync();
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    return;
                case StatusCode.Unauthorized:
                    await Navigation.PushAsync(new ModalAuthenticationPage(MyTrip));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }
        
        protected override async void OnAppearing()
        {
            _viewModel.IsBusy = true;
            base.OnAppearing();
           await LoadTripData();
        }
    }
}