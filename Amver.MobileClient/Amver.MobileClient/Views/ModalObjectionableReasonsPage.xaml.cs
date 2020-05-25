using System.Collections.Generic;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Enums;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Localization;
using Amver.MobileClient.ViewModels;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ObjectionableReason = Amver.Domain.Models.ObjectionableReason;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModalObjectionableReasonsPage : ContentPage
    {
        private readonly int? _objectionableUserId;
        private readonly int? _tripId;
        private readonly INetwork _network;
        public ModalObjectionableReasonsPage(int? objectionableUserId, int? objectionableTripId)
        {
            InitializeComponent();
            _network = App.Container.Resolve<INetwork>();
            _objectionableUserId = objectionableUserId;
            _tripId = objectionableTripId;
            FillObjectionableReasonList();
        }

        private async void OnListViewItemSelected(object sender, ItemTappedEventArgs e)
        {
            if (!(e.Item is ObjectionableReason selectedObjectionableReason))
                return;

            ObjectionableReasonList.SelectedItem = null;
            var objectionableReasonId = selectedObjectionableReason.Id;
            var result = await DisplayAlert(AppResources.ConfirmAction, AppResources.AreYouSure,
                AppResources.ConfirmActionYes, AppResources.ConfirmActionNo);

            if (result)
            {
                var banDto = new BanDto
                {
                    ObjectionableUserId = _objectionableUserId,
                    ObjectionableTripId = _tripId,
                    ObjectionableReasonId = objectionableReasonId,
                    Comment = EditorCommentReason.Text
                };

                var serializableObj = JsonConvert.SerializeObject(banDto);
                var fullUrl = Url.BanUser;
                var (baseResult, content) = await _network.LoadDataPostAsync(fullUrl, serializableObj, null);

                switch (baseResult.Result)
                {
                    case StatusCode.Ok:
                        await DisplayAlert(AppResources.Notification, AppResources.YourReportHasBeenSentSuccessfully,
                            AppResources.Ok);
                        await Navigation.PopAsync(true);
                        break;
                    default:
                        await DisplayAlert(AppResources.Notification, baseResult.Message, AppResources.Ok);
                        await Navigation.PopAsync(true);
                        break;
                }
            }
        }
        
         private void FillObjectionableReasonList()
        {
            var list = new List<ObjectionableReason>
            {
                new ObjectionableReason
                {
                    Id = (int) ObjectionableReasons.NudityOrSexualActivity,
                    Title = AppResources.NudityOrSexualActivity
                },
                new ObjectionableReason
                {
                    Id = (int) ObjectionableReasons.HateSpeechOrSymbols,
                    Title = AppResources.HateSpeechOrSymbols
                },
                new ObjectionableReason
                {
                    Id = (int) ObjectionableReasons.ViolenceOrDangerousOrganizations,
                    Title = AppResources.ViolenceOrDangerousOrganizations
                },
                new ObjectionableReason
                {
                    Id = (int) ObjectionableReasons.SaleOfIllegalOrRegularGoods,
                    Title = AppResources.SaleOfIllegalOrRegularGoods
                },
                new ObjectionableReason
                {
                    Id = (int) ObjectionableReasons.BullyingOrHarassment,
                    Title = AppResources.BullyingOrHarassment
                },
                new ObjectionableReason
                {
                    Id = (int) ObjectionableReasons.IntellectualPropertyViolation,
                    Title = AppResources.IntellectualPropertyViolation
                },
                new ObjectionableReason
                {
                    Id = (int) ObjectionableReasons.SuicideOrSelfInjury,
                    Title = AppResources.SuicideOrSelfInjury
                },
                new ObjectionableReason
                {
                    Id = (int) ObjectionableReasons.ScamOrFraud,
                    Title = AppResources.ScamOrFraud
                },
                new ObjectionableReason
                {
                    Id = (int) ObjectionableReasons.FalseInformation,
                    Title = AppResources.FalseInformation
                }
            };
            ObjectionableReasonList.ItemsSource = list;
        }
    }
}