using System;
using System.IO;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.MobileClient.ViewModels;
using Plugin.Media;
using Plugin.Media.Abstractions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Xamarin.Forms;
using SixLabors.ImageSharp.Processing;
using Amver.MobileClient.Localization;

namespace Amver.MobileClient.Views
{
    public partial class ProfilePage
    {
        private readonly MyProfileViewModel _viewModel;
        private const string MyProfile = "MyProfile";

        public ProfilePage()
        {
            InitializeComponent();
            _viewModel = new MyProfileViewModel();
            BindingContext = _viewModel;
            _viewModel.ImageStatus = ImageStatusDefault;
        }
        
        private string ImageStatusDefault = AppResources.SelectProfileImageButton;
        private string ImageStatusPreparing = AppResources.ImageProcessing;

        private async void OnUpdateButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UpdateProfilePage());
        }

        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert(AppResources.ConfirmAction, AppResources.ConfirmDeleteProfile, AppResources.ConfirmActionYes, AppResources.ConfirmActionNo);
            if (result)
               await RemoveProfileData();
        }

        private async Task RemoveProfileData()
        {
            var (baseResult, _) = await _viewModel.RemoveProfileAsync();

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    var isSuccess =
                        Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out _);
                    if (isSuccess) Application.Current.Properties[AuthOptions.AccessTokenAmver] = string.Empty;
                    await DisplayAlert(AppResources.Notification, AppResources.NotificationProfileSuccessfullyDeleted, AppResources.Ok);
                    await Navigation.PopAsync();
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    return;
                case StatusCode.Unauthorized:
                    await Navigation.PushAsync(new ModalAuthenticationPage(MyProfile));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }

        private async Task LoadProfileData()
        {
            var (baseResult, content) = await _viewModel.LoadProfileAsync();

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.FillProfile(content);
                    _viewModel.IsBusy = false;
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.IsBusy = false;
                    return;
                case StatusCode.Unauthorized:
                    _viewModel.IsBusy = false;
                    await Navigation.PushAsync(new ModalAuthenticationPage(MyProfile));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    _viewModel.IsBusy = false;
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }

        private async void OnImageTapped(object sender, EventArgs args)
        {
            var action = await DisplayActionSheet(null, AppResources.Cancel, AppResources.Delete, AppResources.Camera, AppResources.Gallery);
            if (action == AppResources.Cancel)
                return;
            if (action == AppResources.Delete)
                await RemoveTappedAsync();
            if (action == AppResources.Camera)
                await CameraTappedAsync();
            if (action == AppResources.Gallery)
                await GalleryTappedAsync();          
        }

        private async Task GalleryTappedAsync()
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert(AppResources.Notification, AppResources.NotificationDeviceDoesNotSupportedThisFunction, AppResources.Ok);
                return;
            }
            var mediaOptions = new PickMediaOptions();
            var selectedImageFile = await CrossMedia.Current.PickPhotoAsync(mediaOptions);
            if (selectedImageFile == null)
            {
                await DisplayAlert(AppResources.Notification, AppResources.NotificationCouldNotGetTheImage, AppResources.Ok);
                return;
            }
            
            _viewModel.IsBusy = true;
            _viewModel.ImageStatus = ImageStatusPreparing;
            _viewModel.PrepareImage = AppResources.ImageProcessing;

            Device.BeginInvokeOnMainThread(async () =>
            {
                var stream = selectedImageFile.GetStream();
                var imageByteArray = await GetResizedImageByteArray(stream);
                await UploadFileAsync(imageByteArray);
            });
                
            _viewModel.PrepareImage = string.Empty;
        }

        private async Task CameraTappedAsync()
        { 
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsTakePhotoSupported || !CrossMedia.Current.IsCameraAvailable)
            {
                await DisplayAlert(AppResources.Notification, AppResources.NotificationDeviceDoesNotSupportedThisFunction, AppResources.Ok);
                return;
            }
            var mediaOptions = new StoreCameraMediaOptions
            {
                AllowCropping = true,
            };
            var selectedImageFile = await CrossMedia.Current.TakePhotoAsync(mediaOptions);
            if (selectedImageFile == null)
            {
                await DisplayAlert(AppResources.Notification, AppResources.NotificationCouldNotGetTheImage, AppResources.Ok);
                return;
            }
            _viewModel.ImageStatus = ImageStatusPreparing;
            _viewModel.IsBusy = true;
            _viewModel.PrepareImage = AppResources.ImageProcessing;

            Device.BeginInvokeOnMainThread(async () =>
            {
                var stream = selectedImageFile.GetStreamWithImageRotatedForExternalStorage();
                var imageByteArray = GetResizedImageByteArray(stream);
                await UploadFileAsync(imageByteArray.Result);
            });
          
            _viewModel.PrepareImage = string.Empty;
        }

        private async Task UploadFileAsync(byte[] mediaFileStream)
        {
            if (mediaFileStream == null) throw new ArgumentNullException(nameof(mediaFileStream));

            _viewModel.MediaFileStream = mediaFileStream;

            var (baseResult, _) = await _viewModel.UploadFileAsync();

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.ImageStatus = ImageStatusDefault;
                    _viewModel.IsBusy = false;
                    OnAppearing();
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.ImageStatus = ImageStatusDefault;
                    _viewModel.IsBusy = false;
                    return;
                case StatusCode.Unauthorized:
                    _viewModel.IsBusy = false;
                    await Navigation.PushAsync(new ModalAuthenticationPage(MyProfile));
                    _viewModel.ImageStatus = ImageStatusDefault;
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    _viewModel.IsBusy = false;
                    _viewModel.ImageStatus = ImageStatusDefault;
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }

        private async Task RemoveTappedAsync()
        {
            _viewModel.IsBusy = true;
            var (baseResult, _) = await _viewModel.RemoveFileAsync();

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    await LoadProfileData();
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.IsBusy = false;
                    return;
                case StatusCode.Unauthorized:
                    _viewModel.IsBusy = false;
                    await Navigation.PushAsync(new ModalAuthenticationPage(MyProfile));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    _viewModel.IsBusy = false;
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }

        private static async Task<byte[]> GetResizedImageByteArray(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            await using var outputStream = new MemoryStream();
            using (var image = SixLabors.ImageSharp.Image.Load(stream))
            {
                var currentWidth = image.Width;
                if (currentWidth > 900)
                {
                    var ratioX = (double) 900 / image.Width;
                    var ratioY = (double) 900 / image.Height;
                    var ratio = Math.Max(ratioX, ratioY);
                    var newWidth = (int) (image.Width * ratio);
                    var newHeight = (int) (image.Height * ratio);
                    image.Mutate(c => c.Resize(newWidth, newHeight));
                    var encoder = new JpegEncoder
                    {
                        Quality = 25
                    };
                    image.SaveAsJpeg(outputStream, encoder);
                }
                else
                {
                    image.SaveAsJpeg(outputStream);
                }
            }
            return outputStream.ToArray();
        }
        
        private async void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            await _viewModel.SignOut();
            await Navigation.PopAsync();
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.IsBusy = true;
           await LoadProfileData();
        }
    }
}