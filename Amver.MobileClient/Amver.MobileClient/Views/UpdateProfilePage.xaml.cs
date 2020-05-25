using System;
using System.Collections.Generic;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Localization;
using Amver.MobileClient.ViewModels;
using Autofac;
using dotMorten.Xamarin.Forms;
using Newtonsoft.Json;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateProfilePage
    {
        private readonly INetwork _network;
        private readonly UpdateProfileViewModel _viewModel;
        private const string UpdateProfile = "UpdateProfile";
        public UpdateProfilePage()
        {
            InitializeComponent();
            _network = App.Container.Resolve<INetwork>();
            _viewModel = new UpdateProfileViewModel();
            BindingContext = _viewModel;
        }
        private async void FromCity_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            
            var suggest = sender as AutoSuggestBox;

            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput || string.IsNullOrEmpty(suggest?.Text))
            {
                if (suggest != null) suggest.ItemsSource = null;
                _viewModel.FromCity = null;
                return;
            }

            var namePart = new NamePartDto
            {
                Part = suggest.Text
            };
            var serializedNamePart = JsonConvert.SerializeObject(namePart);
            var (_, content) = await _network.LoadDataPostAsync(Url.CityList, serializedNamePart, null);
            if (string.IsNullOrEmpty(content))
                return;

            var cityList = JsonConvert.DeserializeObject<List<CityDto>>(content);

            suggest.ItemsSource = cityList;
            if (suggest.Text.Length == 0)
            {
                suggest.ItemsSource = null;
            }
        }
        private void FromCity_SuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            _viewModel.FromCity = args.SelectedItem as CityDto;
        }
        
        private async void LoadProfileData()
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
                    await Navigation.PushAsync(new ModalAuthenticationPage(UpdateProfile));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    _viewModel.IsModalAuthWasOpen = false;
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }
        
        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            _viewModel.IsBusy = true;
            var (baseResult, _) = await _viewModel.UpdateProfile();
            
            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    await DisplayAlert(AppResources.Notification, AppResources.ProfileUpdated, AppResources.Ok);           
                    _viewModel.IsBusy = false;
                    await Navigation.PopAsync();
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.IsBusy = false;
                    return;
                case StatusCode.Unauthorized:
                    await Navigation.PushAsync(new ModalAuthenticationPage(UpdateProfile));
                    _viewModel.IsBusy = false;
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    _viewModel.IsBusy = false;
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.IsBusy = true;
            LoadProfileData();
        }
    }
}