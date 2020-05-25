using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Autofac;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xamarin.Forms;
using Amver.MobileClient.Localization;

namespace Amver.MobileClient.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsModalAuthWasOpen { get; set; }
        private string _message;
        private ObservableCollection<MessageModel> _messageList;
        private bool _isConnected;
        private Guid _conversationId;
        private readonly INetwork _network;

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<MessageModel> MessageList
        {
            get => _messageList;
            set
            {
                _messageList = value;
                OnPropertyChanged();
            }
        }
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }
        public Guid ConversationId
        {
            get => _conversationId;
            set
            {
                _conversationId = value;
                OnPropertyChanged();
            }
        }
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
        private string _statusName;
        public string StatusName
        {
            get => _statusName;
            set
            {
                _statusName = value;
                OnPropertyChanged(nameof(StatusName));
            }
        }
        private string _userName;
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
        private readonly HubConnection _hubConnection;
        private readonly int userId;

        public ChatViewModel(int _tripId, int _userId, Guid _conversationId, string userName)
        {
            MessagingCenter.Send<object, Guid>(this, "RemoveConversationWithNewMessage", _conversationId);
            _userName = userName;
            userId = _userId;
            _network = App.Container.Resolve<INetwork>();
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);

            if (isSuccess && bearerTokenObj is string bearerToken)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl("https://amver.net/chat", options => { options.AccessTokenProvider = () => Task.FromResult(bearerToken); })
                    //.WithAutomaticReconnect()
                    .ConfigureLogging(logging =>{
                        logging.AddConsole();
                    })
                    .Build();
                _hubConnection.Closed += async (error) =>
                {
                    await Task.Delay(new Random().Next(0,5) * 1000);
                    await Connect(_tripId, _conversationId);
                };
            }
            MessageList = new ObservableCollection<MessageModel>();
            
            _hubConnection.On<Guid>("JoinChat", (conversationId) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ConversationId = conversationId;
                });
            });
            _hubConnection.On<string,Message>("ReceiveMessage", (userId, message) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var isValidUserId = int.TryParse(userId, out var validUserId);
                    if (isValidUserId && _userId == validUserId)
                    {
                        MessageList.Add(new MessageModel { UserId = validUserId, Message = message.MessageStr, IsSystemMessage = false, IsOwnMessage = _userId != validUserId });
                        MessageIsRead(message);
                    }                 
                    MessagingCenter.Send<object>(this, "MessageReceived");
                });              
            });
        }

        private void MessageIsRead(Message message)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return;

            if (!(bearerTokenObj is string bearerToken))
                return;

            var fullUrl = $"{Url.MessageIsRead}";
            
            message.MessageStr = string.Empty;
            
            var serializedObj = JsonConvert.SerializeObject(message);
            
            _ = _network.LoadDataPostAsync(fullUrl, serializedObj, bearerToken);
        }

        public async Task<(BaseResult baseResult, string content)> LoadMessageListAsync(int tripId, Guid conversationId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));
            
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);
            
            var fullUrl = $"{Url.GetMessageList}";

            var conversationRequest = new ConversationRequest
            {
                Guid = conversationId,
                TripId = tripId
            };
            
            var serializedObj = JsonConvert.SerializeObject(conversationRequest);
            
            var messageListTask = _network.LoadDataPostAsync(fullUrl, serializedObj, bearerToken);
            
            return await messageListTask;
        }

        public async Task<(BaseResult baseResult, string content)> CheckBlock()
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);
            
            var blockers = await _network.LoadDataGetAsync($"{Url.CheckForMyAndMeBlocked}{userId}", bearerToken);
            return blockers;
        }

        public void FillMessageList(string content)
        {
            if (string.IsNullOrEmpty(content)) throw new ArgumentException("Value cannot be null or empty.", nameof(content));
            var messageList = JsonConvert.DeserializeObject<ObservableCollection<MessageModel>>(content);
            _messageList = messageList;
            MessageList = messageList;
        }

        public async Task Connect(int tripId, Guid conversationId)
        {
            StatusName = AppResources.Connecting;
            await _hubConnection.StartAsync();
            await _hubConnection.InvokeAsync("JoinChat", tripId, conversationId);
            IsConnected = true;
            StatusName = string.Empty;
        }

        public async Task SendMessageAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            var sendModel = new SendModel
            {
                TripId = tripId,
                SendDate = DateTime.UtcNow,
                ConversationId = ConversationId,
            };
            if (string.IsNullOrWhiteSpace(Message))
                return;
            var messageToSend = Message;
            Message = string.Empty;
            await _hubConnection.InvokeAsync("SendMessage", sendModel, messageToSend);
            MessageList.Add(new MessageModel { UserId = userId, Message = messageToSend, IsSystemMessage = false, IsOwnMessage = true });
            MessagingCenter.Send<object>(this, "MessageReceived");
        }

        public async Task Disconnect()
        {
            _hubConnection.Remove("ReceiveMessage");
            await _hubConnection.StopAsync();
            //await _hubConnection.InvokeAsync("LeaveChat", Name);
            IsConnected = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<(BaseResult baseResult, string content)> RemoveConversationAsync(Guid conversationId)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized}, null);

            var fullUrl = $"{Url.RemoveConversation}{conversationId}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }
        
        public async Task<(BaseResult baseResult, string content)> BlockUserAsync(int targetUserIdToBlock)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized}, null);

            var fullUrl = $"{Url.BlockUser}{targetUserIdToBlock}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }

        public async Task<(BaseResult baseResult, string content)> UnblockUserAsync(int targetUserIdToBlock)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized}, null);

            var fullUrl = $"{Url.UnblockUser}{targetUserIdToBlock}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }

        public async Task<bool> CheckBlockUserAsync(int targetUserId)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess || string.IsNullOrEmpty(bearerTokenObj.ToString()))
                return false;

            if (!(bearerTokenObj is string bearerToken))
                return false;

            var fullUrl = $"{Url.CheckMyBlocked}{targetUserId}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);
            switch (result.baseResult.Result)
            {
                case StatusCode.Ok:
                    var blocker = JsonConvert.DeserializeObject<UserToBlockedUser>(result.response);
                    if (blocker != null)
                        return true;                    
                    break;
                case StatusCode.Unauthorized:
                    return false;
                default:
                    break;
            }
            return false;
        }
    }
}