using System;
using System.Threading.Tasks;
using Amver.MobileClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Amver.MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModalAuthenticationBirthDayAddPage : ContentPage
    {
        private readonly ModalAuthenticationBirthDayAddViewModel _viewModel;
        private readonly string _userName;

        public ModalAuthenticationBirthDayAddPage(string userName)
        {
            InitializeComponent();
            
            _viewModel = new ModalAuthenticationBirthDayAddViewModel();
            BindingContext = _viewModel;
            _userName = userName;

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<object>(this, "ClosedModalAuthUserEmail", async (sender) =>
            {
                await Task.Delay(10);
                MessagingCenter.Send<object>(this, "ClosedModalAuthUserBirthday");
                await Navigation.PopAsync();
            });
        }

        private async void OnNextButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ModalAuthenticationEmailAddPage(_userName , _viewModel.CurrentDate, _viewModel.UserGender));
        }
    }
}