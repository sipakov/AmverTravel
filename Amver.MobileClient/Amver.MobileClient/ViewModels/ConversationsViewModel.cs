using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Amver.Domain.Constants;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Models;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Amver.MobileClient.ViewModels
{
    public class ConversationsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly INetwork _network;

        private IEnumerable<ConversationForList> _conversations;

        public IEnumerable<ConversationForList> Conversations
        {
            get => _conversations;
            set
            {
                _conversations = value;
                OnPropertyChanged(nameof(Conversations));
            }
        }

        private ConversationForList _item;

        public ConversationForList Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged(nameof(Item));
            }
        }

        public bool IsModalAuthWasOpen { get; set; }

        private bool _isNotEmptyList;

        public bool IsNotEmptyList
        {
            get => _isNotEmptyList;
            set
            {
                _isNotEmptyList = value;
                OnPropertyChanged(nameof(IsNotEmptyList));
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
        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    BaseResult baseResult;
                    string content;
                    (baseResult, content) = await LoadConversationListAsync();
                    IsRefreshing = true;
                    switch (baseResult.Result)
                    {
                        case StatusCode.Ok:
                            FillConversationList(content);
                            break;
                        case StatusCode.Error:
                            break;
                    }
                    IsRefreshing = false;
                });
            }
        }
        public ConversationsViewModel()
        {
            _network = App.Container.Resolve<INetwork>();
        }

        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        //ToDo implement date for current culture correct 
        public async Task<(BaseResult baseResult, string content)> LoadConversationListAsync()
        {
            var fullUrl = $"{Url.GetConversationList}";
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized }, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized }, null);

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);
            return result;
        }

        public void FillConversationList(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));

            var conversations = JsonConvert.DeserializeObject<ConversationResponse>(content);

            Conversations = conversations.Conversations.Select(x => new ConversationForList
            {
                Id = x.Id,
                UserName = x.UserName,
                ToCity = string.IsNullOrEmpty(x.ToCity) ? $"{x.FromCity} -> {x.ToCountry}" : $" {x.FromCity} -> {x.ToCity}, {x.ToCountry}",
                DateFromDateTo = $"{x.DateFrom:D}",
                TripId = x.TripId,
                UserId = x.UserId,
                LastMessage = x.LastMessage + "...",
                ImageUri = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(x.ImageUri != null ? x.ImageUri : null)
                },
                IsNewMessage = x.IsUnreadMessages
            });
            if (_conversations != null && _conversations.Any()) IsNotEmptyList = true;
            IsBusy = false;

        }

        public async Task<(BaseResult baseResult, string content)> RemoveConversationAsync(Guid conversationId)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized }, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized }, null);

            var fullUrl = $"{Url.RemoveConversation}{conversationId}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }

        public async Task<(BaseResult baseResult, string content)> BlockUserAsync(int targetUserIdToBlock)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized }, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized }, null);

            var fullUrl = $"{Url.BlockUser}{targetUserIdToBlock}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }

        public async Task<(BaseResult baseResult, string content)> UnblockUserAsync(int targetUserIdToBlock)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized }, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized }, null);

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
            }
            return false;
        }
    }
}