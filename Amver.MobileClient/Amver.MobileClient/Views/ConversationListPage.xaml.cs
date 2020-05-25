using System;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.MobileClient.Localization;
using Amver.MobileClient.Models;
using Amver.MobileClient.ViewModels;
using Plugin.Badge;
using Xamarin.Forms;

namespace Amver.MobileClient.Views
{
    public partial class ConversationListPage : ContentPage
    {
        private readonly ConversationsViewModel _viewModel;
        const string Conversation = "Conversation";
        public ConversationListPage()
        {
            InitializeComponent();
            _viewModel = new ConversationsViewModel();
            BindingContext = _viewModel;
            MessagingCenter.Subscribe<object>(this, "ClosedModalAuthConversation", (sender) => {
                OnAppearing();
            });
        }
        
        private async void OnListViewItemSelected(object sender, MR.Gestures.TapEventArgs e)
        {
            
            var item = (ConversationForList)((ViewCell)sender).BindingContext;
            
            if (!(item is ConversationForList selectedConversation))
                return;
            _viewModel.Item = item;
            
            await Navigation.PushAsync(new ChatPage(selectedConversation.TripId,selectedConversation.UserId, selectedConversation.Id, selectedConversation.UserName));
        }

        private async Task LoadData()
        {
            var (baseResult, content) = await _viewModel.LoadConversationListAsync();

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.FillConversationList(content);
                    _viewModel.IsBusy = false;
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.Conversations = null;
                    _viewModel.IsBusy = false;
                    return;
                case StatusCode.Unauthorized:
                    await Navigation.PushAsync(new ModalAuthenticationPage(Conversation));
                    _viewModel.Conversations = null;
                    _viewModel.IsBusy = false;
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    _viewModel.Conversations = null;
                    _viewModel.IsBusy = false;
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
            }
        }

        private async void OnListViewItemLongPressing(object sender, MR.Gestures.LongPressEventArgs e)
        {
            var item = (ConversationForList)((ViewCell)sender).BindingContext;
            if (!(item is ConversationForList selectedConversation))
                return;
            var blocker = await _viewModel.CheckBlockUserAsync(selectedConversation.UserId);
            var toDisplay = blocker ? AppResources.Unblock : AppResources.Block;

            var action = await DisplayActionSheet(null, AppResources.Cancel, toDisplay, AppResources.Report, AppResources.GoToTravel, AppResources.GoToProfile, AppResources.Delete);


            if (action == AppResources.GoToTravel)
            {
                var tripId = selectedConversation.TripId;
                await Navigation.PushAsync(new TripPage(tripId));
            }
            if (action == AppResources.GoToProfile)
            {
                var userId = selectedConversation.UserId;
                await Navigation.PushAsync(new ModalUserProfilePage(userId));
            }
            if (action == AppResources.Report)
            {
                var targetUserId = selectedConversation.UserId;
                await Navigation.PushAsync(new ModalObjectionableReasonsPage(targetUserId, null));
            }
            if (action == AppResources.Block)
            {
                var targetUserName = item.UserName;
                var resultBlock = await DisplayAlert($"{AppResources.Block} {targetUserName}?", $"{targetUserName} {AppResources.BlockUser}", AppResources.Block, AppResources.Cancel);
                if (resultBlock)
                    await BlockUser(selectedConversation.UserId);
            }
            if (action == AppResources.Unblock)
            {
                await UnblockUser(selectedConversation.UserId);
            }
            if (action == AppResources.Delete)
            {
                var result = await DisplayAlert(AppResources.ConfirmAction, AppResources.QuestionDoYouWantToDeleteTheConversation, AppResources.ConfirmActionYes, AppResources.ConfirmActionNo);
                if (result)             
                    await RemoveConversationData(selectedConversation.Id);              
            }
            if (action == AppResources.Cancel)
            {
                return;
            }
        }
        
        private async Task RemoveConversationData(Guid conversationId)
        {
            var (baseResult, _) = await _viewModel.RemoveConversationAsync(conversationId);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    await DisplayAlert(AppResources.ConfirmAction, AppResources.ConversationSuccessfullyDeleted, AppResources.Ok); 
                    OnAppearing();
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    return;
                case StatusCode.Unauthorized:
                    await Navigation.PopAsync();
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
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
            _viewModel.IsBusy = true;
            base.OnAppearing();
            CrossBadge.Current.ClearBadge();
            MessagingCenter.Send<object>(this, "NoNewMessages");
            await LoadData();
        }
        protected override void OnDisappearing()
        {
            MessagingCenter.Send<object>(this, "NoNewMessages");
        }
    }
}
