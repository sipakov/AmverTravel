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
    public partial class ModalUserProfilePage : ContentPage
    {
        private readonly UserProfileViewModel _viewModel;
        private readonly int _userId;

        public ModalUserProfilePage(int userId)
        {
            InitializeComponent();
            _viewModel = new UserProfileViewModel();
            BindingContext = _viewModel;
            _userId = userId;
        }

        private async Task LoadProfileData()
        {
            var (baseResult, content) = await _viewModel.LoadProfileAsync(_userId);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.FillProfile(content);
                    _viewModel.IsBusy = false;
                    break;
                case StatusCode.NotAcceptable:
                    await DisplayAlert(AppResources.Notification, AppResources.YouHaveBeenBlocked, AppResources.Ok);
                    _viewModel.IsBusy = false;
                    await Navigation.PopAsync();
                    break;
                default:
                    _viewModel.IsBusy = false;
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }
        
        private async void OnToolbarItemClicked(object sender, EventArgs e)
        {
            var blocker = await _viewModel.CheckBlockUserAsync(_userId);
            var toDisplay = blocker ? AppResources.Unblock : AppResources.Block;

            var action = await DisplayActionSheet(null, AppResources.Cancel, toDisplay, AppResources.Report);

            if (action == AppResources.Report)
            {
                var targetUserId = _userId;
                await Navigation.PushAsync(new ModalObjectionableReasonsPage(targetUserId, null));
            }
            if (action == AppResources.Block)
            {
                var targetUserName = _viewModel.Profile.FirstName;
                var resultBlock = await DisplayAlert($"{AppResources.Block} {targetUserName}?", $"{targetUserName} {AppResources.BlockUser}", AppResources.Block, AppResources.Cancel);
                if (resultBlock)            
                    await BlockUser(_userId);              
            }
            if (action == AppResources.Unblock)
            {
                await UnblockUser(_userId);
            }
            if (action == AppResources.Cancel) return;
        }
        
        private async Task BlockUser(int targetUserIdToBlock)
        {
            var (baseResult, _) = await _viewModel.BlockUserAsync(targetUserIdToBlock);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    await DisplayAlert(AppResources.Notification, AppResources.UserSuccessfullyBlocked, AppResources.Ok); 
                    break;
                case StatusCode.Unauthorized:
                    await DisplayAlert(AppResources.Notification, AppResources.LoginBeforeBlocking, AppResources.Ok);
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }  
        }
        
        private async Task UnblockUser(int targetUserIdToBlock)
        {
            var (baseResult, _) = await _viewModel.UnblockUserAsync(targetUserIdToBlock);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    await DisplayAlert(AppResources.Notification, AppResources.UserSuccessfullyUnblocked, AppResources.Ok); 
                    break;
                case StatusCode.Unauthorized:
                    await DisplayAlert(AppResources.Notification, AppResources.LoginBeforeBlocking, AppResources.Ok);
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }  
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.IsBusy = true;
             await LoadProfileData();
        }
    }
}