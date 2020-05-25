using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Localization;
using Amver.MobileClient.ViewModels;
using Autofac;
using dotMorten.Xamarin.Forms;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateTripPage : ContentPage
    {
        private readonly INetwork _network;
        private readonly UpdateTripViewModel _viewModel;
        private readonly int _tripId;
        const string UpdateTrip = "UpdateTrip";

        public UpdateTripPage(int tripId)
        {
            InitializeComponent();
            _network = App.Container.Resolve<INetwork>();
            _viewModel = new UpdateTripViewModel(tripId);
            BindingContext = _viewModel;
            _tripId = tripId;
        }

        private async void FromCity_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var suggest = sender as AutoSuggestBox;

            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput || string.IsNullOrEmpty(suggest.Text))
            {
                suggest.ItemsSource = null;
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

        private async void ToCountry_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var suggest = sender as AutoSuggestBox;

            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput || string.IsNullOrEmpty(suggest.Text))
            {
                suggest.ItemsSource = null;
                _viewModel.ToCountry = null;
                return;
            }

            var namePart = new NamePartDto
            {
                Part = suggest.Text
            };
            var serializedNamePart = JsonConvert.SerializeObject(namePart);
            var (_, content) = await _network.LoadDataPostAsync(Url.CountryList, serializedNamePart, null);
            if (string.IsNullOrEmpty(content))
                return;

            var countryList = JsonConvert.DeserializeObject<List<CountryDto>>(content);

            suggest.ItemsSource = countryList;
            if (suggest.Text.Length == 0)
            {
                suggest.ItemsSource = null;
            }
        }

        private async void ToCity_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var suggest = sender as AutoSuggestBox;

            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput || string.IsNullOrEmpty(suggest.Text))
            {
                suggest.ItemsSource = null;
                _viewModel.ToCity = null;
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

        private void ToCountry_SuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            _viewModel.ToCountry = args.SelectedItem as CountryDto;
        }

        private void ToCity_SuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            _viewModel.ToCity = args.SelectedItem as CityDto;
        }

        private async void OnUpdateButtonClicked(object sender, EventArgs e)
        {
            _viewModel.IsBusy = true;
            var (baseResult, content) = await _viewModel.UpdateTrip();

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.IsBusy = false;
                    await Navigation.PopAsync();
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.IsBusy = false;
                    return;
                case StatusCode.Unauthorized:
                    _viewModel.IsBusy = false;
                    await Navigation.PushAsync(new ModalAuthenticationPage(UpdateTrip));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    _viewModel.IsBusy = false;
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }

        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert(AppResources.ConfirmAction, AppResources.QuestionDoYouWantToDeleteTheTravel,
                AppResources.ConfirmActionYes, AppResources.ConfirmActionNo);
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
                    _viewModel.IsBusy = false;
                    return;
                case StatusCode.Unauthorized:
                    _viewModel.IsBusy = false;
                    await Navigation.PushAsync(new ModalAuthenticationPage(UpdateTrip));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }

        private async Task RemoveTripData()
        {
            _viewModel.IsBusy = true;
            var (baseResult, _) = await _viewModel.RemoveTripAsync(_tripId);

            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.IsBusy = false;
                    await Navigation.PopAsync();
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    _viewModel.IsBusy = false;
                    return;
                case StatusCode.Unauthorized:
                    _viewModel.IsBusy = false;
                    await Navigation.PushAsync(new ModalAuthenticationPage(UpdateTrip));
                    _viewModel.IsModalAuthWasOpen = true;
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
            await LoadTripData();
        }
    }
}