using Facebook.CoreKit;
using Foundation;
using ImageCircle.Forms.Plugin.iOS;
using UIKit;
using System;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Autofac;
using Firebase.CloudMessaging;
using Newtonsoft.Json;
using UserNotifications;
using Xamarin.Forms;
using Amver.Domain.Enums;

namespace Amver.MobileClient.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate,
        IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication,
            NSObject annotation)
        {
            return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            Messaging.SharedInstance.ApnsToken = deviceToken;
        }
        private INetwork _network;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Firebase.Core.App.Configure();
            Firebase.Crashlytics.Crashlytics.Configure();
            Forms.Init();
            ImageCircleRenderer.Init();
            _network = App.Container.Resolve<INetwork>();
            
            // iOS 10 or later
            const UNAuthorizationOptions authOptions =
                UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
            UNUserNotificationCenter.Current.RequestAuthorization(authOptions,
                (granted, error) => { Console.WriteLine(granted); });

            UIApplication.SharedApplication.RegisterForRemoteNotifications();

            LoadApplication(application: new App());

            Firebase.InstanceID.InstanceId.Notifications.ObserveTokenRefresh((sender, e) =>
            {
                var fcmTokenReload = Messaging.SharedInstance.FcmToken;
                if (!string.IsNullOrEmpty(fcmTokenReload))
                {
                    var isSuccess = Xamarin.Forms.Application.Current.Properties.TryGetValue("fcmTokenAmver", out var fcmTokenObj);
                    if (isSuccess)
                    {
                        Xamarin.Forms.Application.Current.Properties.Remove("fcmTokenAmver");
                    }
                    Xamarin.Forms.Application.Current.Properties.Add("fcmTokenAmver", fcmTokenReload);

                    LoadFcmAsync(fcmTokenReload);
                }
             
            });
            var fcmTokenReload = Messaging.SharedInstance.FcmToken;
            if (!string.IsNullOrEmpty(fcmTokenReload))
            {
                var isSuccess = Xamarin.Forms.Application.Current.Properties.TryGetValue("fcmTokenAmver", out var fcmTokenObj);
                if (isSuccess)
                {
                    Xamarin.Forms.Application.Current.Properties.Remove("fcmTokenAmver");
                }
                Xamarin.Forms.Application.Current.Properties.Add("fcmTokenAmver", fcmTokenReload);

                LoadFcmAsync(fcmTokenReload);
            }
         
            return base.FinishedLaunching(app, options);
        }
        
        private async void LoadFcmAsync(string token)
        {
            var isSuccess = Xamarin.Forms.Application.Current.Properties.TryGetValue("accessTokenAmver", out var bearerTokenObj);
            if (!isSuccess)
                return;
            if (!(bearerTokenObj is string bearerToken))
                return;
            if (string.IsNullOrEmpty(bearerToken))
                return;
           
            var fcmTokenModel = new FcmTokenModel { FcmToken = token };
            if (Device.RuntimePlatform == Device.iOS)
            {
                fcmTokenModel.Os = (int)OperationSystems.iOs;
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                fcmTokenModel.Os = (int)OperationSystems.Android;
            }
            var serializedFcmToken = JsonConvert.SerializeObject(fcmTokenModel);

            var fullUrl = $"{Amver.Domain.Constants.Url.AddUserFcmToken}";
            await _network.LoadDataPostAsync(fullUrl, serializedFcmToken, bearerToken);
        }

        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo,
            Action<UIBackgroundFetchResult> completionHandler)
        {
            //var conversationIdStr = userInfo.ValueForKey(new NSString("conversation_id"))?.ToString();
            MessagingCenter.Send<object>(this, "NewMessages");
            //MessagingCenter.Send<object, string>(this, "NewConversationWithNewMessage", conversationIdStr);
        }

        public override void WillEnterForeground(UIApplication uiApplication)
        {
            //handle your event here 
            base.WillEnterForeground(uiApplication);
        }       
    }
}