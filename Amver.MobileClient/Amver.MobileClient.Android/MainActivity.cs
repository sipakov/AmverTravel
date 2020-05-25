using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Autofac;
using Firebase.Iid;
using Newtonsoft.Json;
using Plugin.CurrentActivity;
using Plugin.Permissions;

namespace Amver.MobileClient.Android
{
    [Activity(Label = "Amver Travel", Theme = "@style/MainTheme", Exported = true, Icon = "@mipmap/ic_launcher", 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        static readonly string TAG = "MainActivity";
        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;
        private INetwork _network;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);
            
            LoadApplication(new App());

            var token = FirebaseInstanceId.Instance.Token;

            if (!string.IsNullOrWhiteSpace(token))
            {
                var isSuccess = Xamarin.Forms.Application.Current.Properties.TryGetValue("fcmTokenAmver", out _);
                if (isSuccess)
                {
                    Xamarin.Forms.Application.Current.Properties.Remove("fcmTokenAmver");
                }
                Xamarin.Forms.Application.Current.Properties.Add("fcmTokenAmver", token);
                LoadFcmAsync(token);
            }        

            CreateNotificationChannel();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID, "FCM Notifications", NotificationImportance.Default)
            {
                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager) GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        private async void LoadFcmAsync(string token)
        {
            _network = App.Container.Resolve<INetwork>();
            var isSuccess = Xamarin.Forms.Application.Current.Properties.TryGetValue("accessTokenAmver", out var bearerTokenObj);
            if (!isSuccess)
                return;
            if (!(bearerTokenObj is string bearerToken))
                return;
            if (string.IsNullOrEmpty(bearerToken))
                return;

            var fcmTokenModel = new FcmTokenModel { FcmToken = token };
            var serializedFcmToken = JsonConvert.SerializeObject(fcmTokenModel);

            var fullUrl = $"{Amver.Domain.Constants.Url.AddUserFcmToken}";
            await _network.LoadDataPostAsync(fullUrl, serializedFcmToken, bearerToken);
        }
    }
}