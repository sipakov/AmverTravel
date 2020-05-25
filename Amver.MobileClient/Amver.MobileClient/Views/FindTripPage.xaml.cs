using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Localization;
using Amver.MobileClient.Models;
using Amver.MobileClient.ViewModels;
using Autofac;
using dotMorten.Xamarin.Forms;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FindTripPage
    {
        private readonly INetwork _network;
        private readonly TripListViewModel _viewModel;
        private int countReloadPage = 0;

        public FindTripPage()
        {
            _network = App.Container.Resolve<INetwork>();
            InitializeComponent();
            _viewModel = new TripListViewModel();
            BindingContext = _viewModel;
        }

        private async void FromCity_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var suggest = sender as AutoSuggestBox;
            if (args == null || args.Reason != AutoSuggestionBoxTextChangeReason.UserInput ||
                string.IsNullOrEmpty(suggest?.Text))
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
            if(suggest.Text.Length == 0)
            {
                suggest.ItemsSource = null;
            }
        }

        private async void ToCountry_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var suggest = sender as AutoSuggestBox;

            if (args == null || args.Reason != AutoSuggestionBoxTextChangeReason.UserInput ||
                string.IsNullOrEmpty(suggest?.Text))
            {
                suggest.ItemsSource = new List<CountryDto>();
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

            if (args == null || args.Reason != AutoSuggestionBoxTextChangeReason.UserInput ||
                string.IsNullOrEmpty(suggest?.Text))
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
        }

        private void ToCity_SuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            _viewModel.ToCity = args.SelectedItem as CityDto;
        }

        private async void OnListViewItemSelected(object sender, ItemTappedEventArgs e)
        {
            if (!(e.Item is TripForList selectedTrip))
                return;

            TripList.SelectedItem = null;
            var tripId = selectedTrip.Id;
            await Navigation.PushAsync(new TripPage(tripId));
        }

        private async Task LoadData()
        {
            BaseResult baseResult;
            string content;
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            
                if (!isSuccess)
                {
                    try
                    {
                        (baseResult, content) = await _viewModel.LoadTripListAsync(null);
                    }
                    catch (Exception e)
                    {
                        if (countReloadPage == 0)
                        {
                            countReloadPage++;
                            (baseResult, content) = await _viewModel.LoadTripListAsync(null);
                        }
                        else
                        {
                            countReloadPage = 0;
                            await DisplayAlert(AppResources.Notification, AppResources.NotificationError, AppResources.Ok);
                            throw;
                        }
                    }
                }
                else
                {
                    if (!(bearerTokenObj is string bearerToken))
                        return;
                    (baseResult, content) = await _viewModel.LoadTripListAsync(bearerToken);
                }

                switch (baseResult.Result)
            {
                case StatusCode.Ok:
                    _viewModel.FillTripLists(content);
                    break;
                default:
                    await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                    break;
            }
        }
        
        private async void OnSearchTripsButtonClicked(object sender, EventArgs e)
        {
            _viewModel.IsVisibleSearch = !_viewModel.IsVisibleSearch;
            SearchButton.Text = _viewModel.IsVisibleSearch ? AppResources.SearchNow + " >" : AppResources.SearchButton + " >";
            SearchButtonFrame.HasShadow = !_viewModel.IsVisibleSearch;
            SearchButtonFrame.BorderColor =
                _viewModel.IsVisibleSearch ? Color.FromHex("#4ba5c0") : Color.White;
            await LoadData();
        }

        protected override async void OnAppearing()
        {
            _viewModel.IsBusy = true;
            base.OnAppearing();
            await LoadData();
        }
    }
}