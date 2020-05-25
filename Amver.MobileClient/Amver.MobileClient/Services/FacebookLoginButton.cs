using Xamarin.Forms;

namespace Amver.MobileClient.Services
{
    public class FacebookLoginButton : View
    {
        public string[] Permissions
        {
            get { return (string[])GetValue(PermissionsProperty); }
            set { SetValue(PermissionsProperty, value); }
        }

        public static readonly BindableProperty PermissionsProperty =
            BindableProperty.Create(
                nameof(Permissions),
                typeof(string[]),
                typeof(FacebookLoginButton),
                // This permission is set by default, even if you don't add it, but FB recommends to add it anyway
                defaultValue: new string[] { "public_profile", "email" });

        public Command<string> OnSuccess
        {
            get => (Command<string>)GetValue(OnSuccessProperty);
            set => SetValue(OnSuccessProperty, value);
        }

        public static readonly BindableProperty OnSuccessProperty =
            BindableProperty.Create(nameof(OnSuccess), typeof(Command<string>), typeof(FacebookLoginButton));

        public Command<string> OnError
        {
            get { return (Command<string>)GetValue(OnErrorProperty); }
            set { SetValue(OnErrorProperty, value); }
        }

        public static readonly BindableProperty OnErrorProperty =
            BindableProperty.Create(nameof(OnError), typeof(Command<string>), typeof(FacebookLoginButton));

        public Command OnCancel
        {
            get { return (Command)GetValue(OnCancelProperty); }
            set { SetValue(OnCancelProperty, value); }
        }

        public static BindableProperty OnCancelProperty =
            BindableProperty.Create(nameof(OnCancel), typeof(Command), typeof(FacebookLoginButton));

    }
}