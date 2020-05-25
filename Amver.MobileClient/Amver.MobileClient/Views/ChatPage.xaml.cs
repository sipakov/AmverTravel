using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.MobileClient.Localization;
using Amver.MobileClient.ViewModels;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        private readonly ChatViewModel _viewModel;
        private readonly int _tripId;
        private readonly int _userId;
        private readonly Guid _conversationId;
        const string Chat = "Chat";
        private readonly string _userName;
        public ChatPage(int tripId, int userId, Guid conversationId, string userName)
        {
            InitializeComponent();
            _userName = userName;
            _tripId = tripId;
            _userId = userId;
            _conversationId = conversationId;
            _viewModel = new ChatViewModel(tripId, userId, conversationId, userName);
            BindingContext = _viewModel;
            Appearing += (object sender, EventArgs e) => EntrySend.Focus();
            
            MessagingCenter.Subscribe<object>(this, "MessageReceived", (sender) => {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MainScreenMessagesListView.ScrollTo(_viewModel.MessageList.LastOrDefault(), ScrollToPosition.Start, false);
                });
            });
            MessagingCenter.Subscribe<object>(this, "ClosedModalAuthChat", (sender) => {
                OnAppearing();
            });
        }

        private async Task LoadData()
        {
            var rawBlocks = await _viewModel.CheckBlock();
            var blocks = JsonConvert.DeserializeObject<List<UserToBlockedUser>>(rawBlocks.content);
            ButtonSend.IsEnabled = true;
            EntrySend.IsEnabled = true;
            EntrySend.Placeholder = AppResources.ChatMessagePlaceholder;
            if (blocks.Any())
            {
                ButtonSend.IsEnabled = false;
                EntrySend.IsEnabled = false;
                EntrySend.Placeholder = AppResources.ChatIsBlockedPlaceholder;
            }

            var connectTask = _viewModel.Connect(_tripId, _conversationId);
            var getMessagesTask = _viewModel.LoadMessageListAsync(_tripId, _conversationId);
            var (baseResult, content) = (new BaseResult(), string.Empty);
            if (_conversationId == Guid.Empty)
            {
                await connectTask;
                (baseResult, content) = await _viewModel.LoadMessageListAsync(_tripId, _viewModel.ConversationId);
            }
            else
            {
                await Task.WhenAny(connectTask, getMessagesTask);
                (baseResult, content) = await getMessagesTask;
            }
                      
            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.IsBusy = false;
                    _viewModel.FillMessageList(content);
                    
                    Device.BeginInvokeOnMainThread(() =>
                    {                      
                            MainScreenMessagesListView.ScrollTo(_viewModel.MessageList.LastOrDefault(), ScrollToPosition.MakeVisible, false);
                    });               
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    return;
                case StatusCode.Unauthorized:
                    await Navigation.PushAsync(new ModalAuthenticationPage(Chat));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
            }
        }
        
        private async void OnSendButtonClicked(object sender, EventArgs e)
        {
            EntrySend.Focus();
            await _viewModel.SendMessageAsync(_tripId);
        }
        private void OnSizeChanged(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {                      
                MainScreenMessagesListView.ScrollTo(_viewModel.MessageList.LastOrDefault(), ScrollToPosition.MakeVisible, false);
            });      
        }

        private async void OnRemoveItemClicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert(AppResources.ConfirmAction, AppResources.QuestionDoYouWantToDeleteTheConversation, AppResources.ConfirmActionYes, AppResources.ConfirmActionNo);
            if (result)
            {
                await RemoveConversationData();
            }
        }

        private async Task RemoveConversationData()
        {
            var (baseResult, _) = await _viewModel.RemoveConversationAsync(_conversationId);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    await DisplayAlert(AppResources.Notification, AppResources.ConversationSuccessfullyDeleted, AppResources.Ok);
                    await Navigation.PopAsync();
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
                var targetUserName = _userName;
                var resultBlock = await DisplayAlert($"{AppResources.Block} {targetUserName}?", $"{targetUserName} {AppResources.BlockUser}", AppResources.Block, AppResources.Cancel);
                if (resultBlock)
                    await BlockUser(_userId);
            }
            if (action == AppResources.Unblock)
            {
                await UnblockUser(_userId);
            }
            if (action == AppResources.Cancel)
            {
                return;
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
            base.OnAppearing();
            _viewModel.IsBusy = true;
            await LoadData();
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
           await _viewModel.Disconnect();
        }
    }
}