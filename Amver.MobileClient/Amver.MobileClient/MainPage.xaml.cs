using Amver.MobileClient.Views;
using Xamarin.Forms;

namespace Amver.MobileClient
{
    public partial class MainPage : TabbedPage
    {        
        public MainPage()
        {
            Page tab1Page = new FindTripPage { Title = Localization.AppResources.SearchTab, IconImageSource = "search.png"};
            Page tab2Page = new FavoriteTripsPage { Title = Localization.AppResources.FavouritesTab, IconImageSource = "heart.png" };
            Page tab3Page = new AddTripPage { Title = Localization.AppResources.AddTab, IconImageSource = "plus.png" };
            Page tab4Page = new ConversationListPage { Title = Localization.AppResources.MessagesTab, IconImageSource = "message.png" };
            Page tab5Page = new CabinetPage { Title = Localization.AppResources.CabinetTab, IconImageSource = "account.png" };

            // To change tab order, just shuffle these Add calls around.
            Children.Add(tab1Page);
            Children.Add(tab2Page);
            Children.Add(tab3Page);
            Children.Add(tab4Page);
            Children.Add(tab5Page);
            InitializeComponent();
            
            MessagingCenter.Subscribe<object>(this, "NewMessages", (obj) =>
            {
                tab4Page.IconImageSource = "messageExist.png";
            });
                MessagingCenter.Subscribe<object>(this, "NoNewMessages", (obj) =>
            {
                tab4Page.IconImageSource = "message.png";
            });
        }     
    }
}