using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EulaPage : ContentPage
    {
        public EulaPage(bool isModal)
        {
            InitializeComponent();
            OnButtonAgree.IsEnabled = false;
            CloseButton.IsVisible = isModal;
            OnButtonAgree.IsVisible = !isModal;
        }

        private async void OnButtonAgreeClicked(object sender, EventArgs e)
        {
            MessagingCenter.Send<object>(this, "EulaIsSuccessAgree");
            await Navigation.PopModalAsync();
        }

        private void OnScrolled(object sender, ScrolledEventArgs e)
        {
            var scrollingSpace = scrollView.ContentSize.Height - scrollView.Height;
            if (scrollingSpace <= e.ScrollY + 50)
                OnButtonAgree.IsEnabled = true;
        }

        private async void OnCloseButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}