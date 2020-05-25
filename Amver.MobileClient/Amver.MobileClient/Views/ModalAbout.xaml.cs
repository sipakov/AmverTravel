using System;
using Amver.Domain.Constants;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModalAbout : ContentPage
    {
        public ModalAbout()
        {
            InitializeComponent();
        }

        [Obsolete]
        private void OnAboutButtonClicked(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri(Url.MyContactMail));
        }
        
        private async void OnCloseButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Send<object>(this, "ClosedModalAbout");
            Navigation.PopModalAsync();
        }
    }
}