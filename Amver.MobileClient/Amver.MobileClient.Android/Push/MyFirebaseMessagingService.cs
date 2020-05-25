using System.Collections.Generic;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Android.App;
using Android.Content;
using Android.Graphics;
using Autofac;
using Firebase.Messaging;
using Newtonsoft.Json;
using Android.Support.V4.App;
using static Amver.MobileClient.Android.Resource;
using Notification = Android.App.Notification;

namespace Amver.MobileClient.Android.Push
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "MyFirebaseMsgService";
        private INetwork _network;
        public override void OnNewToken(string token)
        {
            var isSuccess = Xamarin.Forms.Application.Current != null;
            if (isSuccess)
            {
                var isSuccessToken = Xamarin.Forms.Application.Current.Properties.TryGetValue("fcmTokenAmver", out _);
                if (isSuccessToken)
                {
                    Xamarin.Forms.Application.Current.Properties.Remove("fcmTokenAmver");
                }
                Xamarin.Forms.Application.Current.Properties.Add("fcmTokenAmver", token);
                LoadFcmAsync(token);
            }
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            var body = message.GetNotification().Body;
            var title = message.GetNotification().Title;
            SendNotification(title, body, message.Data);
        }

        private void SendNotification(string title, string messageBody, IDictionary<string, string> data)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            foreach (var key in data.Keys)
            {
                intent.PutExtra(key, data[key]);
            }

            var pendingIntent = PendingIntent.GetActivity(this,
                MainActivity.NOTIFICATION_ID,
                intent,
                PendingIntentFlags.OneShot);
            var bitMap = BitmapFactory.DecodeResource(Resources, Mipmap.ic_launcher_notification);
            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                .SetSmallIcon(Mipmap.ic_launcher)
                .SetLargeIcon(bitMap)
                .SetContentTitle(title)
                .SetContentText(messageBody)
                .SetContentIntent(pendingIntent)
                .SetVisibility((int) NotificationVisibility.Public)
                .SetDefaults(NotificationCompat.DefaultAll)
                .SetPriority(NotificationCompat.PriorityHigh);

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NOTIFICATION_ID, notificationBuilder.Build());
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
