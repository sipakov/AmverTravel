using System;
using UserNotifications;

namespace Amver.MobileClient.iOS.Push
{
    public class YourOwnUNUserNotificationCenterDelegate : UNUserNotificationCenterDelegate
    {
        public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
          
        }
    }
}