using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Localization;
using Amver.MobileClient.Models;
using Amver.MobileClient.Services;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Amver.MobileClient.ViewModels
{
    public class CabinetViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly INetwork _network;

        private bool _isLogin;
        public bool IsLogin
        {
            get => _isLogin;
            set
            {
                _isLogin = value;
                OnPropertyChanged(nameof(IsLogin));
            }
        }

        private string _loginLogout;
        public string LoginLogout
        {
            get => _loginLogout;
            set
            {
                _loginLogout = value;
                OnPropertyChanged(nameof(LoginLogout));
            }
        }
        
        private List<TripForList> _myTrips;
        public List<TripForList> MyTrips
        {
            get => _myTrips;
            set
            {
                _myTrips = value;
                OnPropertyChanged(nameof(MyTrips));
            }
        }

        private List<TripForList> _activeTrips;
        public List<TripForList> ActiveTrips
        {
            get => _activeTrips;
             set
            {
                _activeTrips = value;
                OnPropertyChanged(nameof(ActiveTrips));
            }
        }
        
        private List<TripForList> _completedTrips;
        
        public List<TripForList> CompletedTrips
        {
            get => _completedTrips;
             set
            {
                _completedTrips = value;
                OnPropertyChanged(nameof(CompletedTrips));
            }
        }

        private bool _isCompletedNotEmpty; 
        
        public bool IsCompletedNotEmpty
        {
            get => _isCompletedNotEmpty; 
            set
            {
                _isCompletedNotEmpty = value;
                OnPropertyChanged(nameof(IsCompletedNotEmpty));
            }
        }
        private bool _isMyTripsNotEmpty;
        public bool IsMyTripsNotEmpty
        {
            get => _isMyTripsNotEmpty; 
            set
            {
                _isMyTripsNotEmpty = value;
                OnPropertyChanged(nameof(IsMyTripsNotEmpty));
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
        
        private ImageSource _userImage;

        public ImageSource UserImage
        {
            get => _userImage;
            set
            {
                _userImage = value;
                OnPropertyChanged(nameof(UserImage));
            }
        }
        
        private UriImageSource _userImageUri;

        public UriImageSource UserImageUri
        {
            get => _userImageUri;
            set
            {
                _userImageUri = value;
                OnPropertyChanged(nameof(UserImageUri));
            }
        }
        
        private List<TripForList> _inCompletedTrips;
        
        public List<TripForList> InCompletedTrips
        {
            get => _inCompletedTrips;
            set
            {
                _inCompletedTrips = value;
                OnPropertyChanged(nameof(InCompletedTrips));
            }
        }

        private bool _isInCompletedNotEmpty; 
        
        public bool IsInCompletedNotEmpty
        {
            get => _isInCompletedNotEmpty; 
            set
            {
                _isInCompletedNotEmpty = value;
                OnPropertyChanged(nameof(IsInCompletedNotEmpty));
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

        private UserDto _user;
        public UserDto User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged(nameof(User));
            }
        }
        public bool IsModalAuthWasOpen { get; set; }

        public CabinetViewModel()
        {
            _network = App.Container.Resolve<INetwork>();
        }
        
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public async Task<(BaseResult baseResult, string content)> LoadTripListAsync()
        {
            var fullUrl = $"{Url.GetMyTripList}";
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken) || string.IsNullOrEmpty(bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);
            
            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);
            return result;
        }

        public void FillTripLists(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));
            
            var tripList = JsonConvert.DeserializeObject<TripResponse>(content);
            
            var trips = new List< TripForList>();
            ActiveTrips = tripList.Trips.Where(x=>!x.IsCompleted && !x.IsDeleted && x.DateTo > DateTime.UtcNow).Select(x => new TripForList
            {
                Id = x.Id,
                UserName = x.UserName,
                FromCity = $"{x.FromCity} -> ",
                ToCity = string.IsNullOrEmpty(x.ToCity) ? $"{x.ToCountry}" : $"{x.ToCity}",
                DateFromDateTo = $"{x.DateFrom:D}",
                Status = AppResources.ActiveTravel,
                ImageUri = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(x.ImageUri)
                }
            }).ToList();
            trips.AddRange(ActiveTrips);
            
            CompletedTrips = tripList.Trips.Where(x=>x.IsCompleted && !x.IsDeleted).Select(x => new TripForList
            {
                Id = x.Id,
                UserName = x.UserName,
                FromCity = $"{x.FromCity} -> ",
                ToCity = string.IsNullOrEmpty(x.ToCity) ? $"{x.ToCountry}" : $"{x.ToCity}",
                DateFromDateTo = $"{x.DateFrom:D}",
                Status = AppResources.CompletedTravel,
                ImageUri = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(x.ImageUri)
                }
            }).ToList();
            trips.AddRange(CompletedTrips);

            InCompletedTrips = tripList.Trips.Where(x=>!x.IsCompleted && !x.IsDeleted && x.DateTo < DateTime.UtcNow).Select(x => new TripForList
            {
                Id = x.Id,
                UserName = x.UserName,
                FromCity = $"{x.FromCity} -> ",
                ToCity = string.IsNullOrEmpty(x.ToCity) ? $"{x.ToCountry}" : $"{x.ToCity}",
                DateFromDateTo = $"{x.DateFrom:D}",
                Status = AppResources.IncompletedTravel,
                ImageUri = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(x.ImageUri)
                }
            }).ToList();
            trips.AddRange(InCompletedTrips);

            MyTrips = trips.OrderBy(x => x.Status).ToList();
            
            IsBusy = false;
        }
        
        public async Task<(BaseResult baseResult, string content)> LoadUserNameAsync()
        {
            var fullUrl = $"{Url.GetUserIcon}";
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);

            if (!(bearerTokenObj is string bearerToken) || string.IsNullOrEmpty(bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);
            
            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken).ConfigureAwait(false);
            return result;
        }
        
        public void FillUserName(string content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            var userDto = JsonConvert.DeserializeObject<UserDto>(content);
            if (userDto == null)
                return;

            if (string.IsNullOrEmpty(userDto.FirstName))
                return;

            User = userDto;
            
            UserName = userDto.FirstName + " >";
            UserImageUri = new UriImageSource
            {
                CachingEnabled = true,
                CacheValidity = new TimeSpan(30, 0, 0, 0),
                Uri = new Uri(userDto.UserImageUri)
            };
        }
        
        public async Task SignOut()
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return;

            var isSuccessFcmToken = Application.Current.Properties.TryGetValue(AuthOptions.FcmTokenAmver, out var fcmTokenAmver);

            var fcmTokenModel = new FcmTokenModel { FcmToken = isSuccessFcmToken ? fcmTokenAmver.ToString() : null };
            var serializedModel = JsonConvert.SerializeObject(fcmTokenModel);

            if (bearerTokenObj is string bearerToken)
            {
                 await _network.LoadDataPostAsync(Url.SignOut, serializedModel, bearerToken).ConfigureAwait(false);
            }

            Application.Current.Properties[AuthOptions.AccessTokenAmver] = string.Empty;
            FacebookLoginButton.OnCancelProperty = null;
        }
    }
}