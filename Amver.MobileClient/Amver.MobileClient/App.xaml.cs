using Amver.Libraries.Network.Implementations;
using Amver.Libraries.Network.Interfaces;
using Autofac;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Amver.MobileClient
{
    public partial class App : Application
    {
        public static IContainer Container{ get; set; }
        
        static App()  
        {
            InitializeIocContainer();  
        } 

        public App()
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.Android)
            {
                MainPage = new NavigationPage(new MainPage()) { BarBackgroundColor = Color.FromHex("#ffffff"), BarTextColor = Color.Gray};
            }
            else
            {
                MainPage = new NavigationPage(new MainPage()) { BarBackgroundColor = Color.FromHex("#ffffff"), BarTextColor = Color.FromHex("#4d5050")};
            }
        }
        protected override void OnStart()
        {
                           
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
        private static void InitializeIocContainer() 
        {    
            var builder = new ContainerBuilder();    
            builder.RegisterType<Network>().As<INetwork>();
            Container = builder.Build();

            // Don't stick view models in the container unless you need them globally, which is almost never true !!!

            // Don't Resolve the service variable until you need it !!!! }
        }
    }
}