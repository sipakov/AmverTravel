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
    public partial class AddTripPage
    {
        private readonly INetwork _network;
        private readonly AddTripViewModel _viewModel;
        const string Add = "Add";
        public AddTripPage()
        {
            InitializeComponent();
            _network = App.Container.Resolve<INetwork>();
            _viewModel = new AddTripViewModel();
            BindingContext = _viewModel;
        }
        
        private async void FromCity_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var suggest = sender as AutoSuggestBox;

            if (args == null || args.Reason != AutoSuggestionBoxTextChangeReason.UserInput || string.IsNullOrEmpty(suggest?.Text))
            {
                suggest.ItemsSource = null;
                _viewModel.FromCity = new CityDto();
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
            var suggest = sender as AutoSuggestBox;

            if (args == null || args.Reason != AutoSuggestionBoxTextChangeReason.UserInput || string.IsNullOrEmpty(suggest.Text))
            {
                suggest.ItemsSource = null;
                _viewModel.ToCountry = new CountryDto();
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
            var suggest = sender as AutoSuggestBox;

            if (args == null || args.Reason != AutoSuggestionBoxTextChangeReason.UserInput || string.IsNullOrEmpty(suggest.Text))
            {
                suggest.ItemsSource = null;
                _viewModel.ToCity = new CityDto();
                return;
            }

            var namePart = new NamePartDto
            {
                Part = suggest.Text,
                CountryId = _viewModel.ToCountry.Id
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
            _viewModel.ToCity = new CityDto();
        }

        private void ToCity_SuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            _viewModel.ToCity = args.SelectedItem as CityDto;
        }
        
        private async void OnAddTripButtonClicked(object sender, EventArgs e)
        {
            var (baseResult, _) = await _viewModel.AddTripAsync();
            
            switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    await DisplayAlert(AppResources.Notification, AppResources.TravelSuccessfullyAdded, AppResources.Ok);
                    break;
                case StatusCode.Unauthorized when _viewModel.IsModalAuthWasOpen:
                    return;
                case StatusCode.Unauthorized:
                    await Navigation.PushAsync(new ModalAuthenticationPage(Add));
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    _viewModel.IsModalAuthWasOpen = true;
                    break;
            }
        }
        
        protected override void OnAppearing()
        {
            _viewModel.IsBusy = true;
            base.OnAppearing();
            AddTripViewModel.GetNamesPreferredGendersEnum();
            _viewModel.IsBusy = false;
        }
    }
}