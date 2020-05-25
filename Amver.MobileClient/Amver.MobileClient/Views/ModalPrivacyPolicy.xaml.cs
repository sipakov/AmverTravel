using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModalPrivacyPolicy : ContentPage
    {
        public ModalPrivacyPolicy()
        {
            InitializeComponent();
        }
        private async void OnCloseButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Navigation.PopModalAsync();
        }
    }
}