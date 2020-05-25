using System;
using Amver.MobileClient.Styles;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ContentPage), typeof(Amver.MobileClient.iOS.Renderers.PageRenderer))]
namespace Amver.MobileClient.iOS.Renderers
{
    public class PageRenderer : Xamarin.Forms.Platform.iOS.PageRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
                return;


            try
            {
                SetTheme();
            }
            catch (Exception ex) { }
            }
            private void SetTheme()
            {
                if (TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark)
                    App.Current.Resources = new DarkTheme(); // needs using DarkMode.Styles;
                else
                    App.Current.Resources = new LightTheme();
            }

            public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if (TraitCollection.UserInterfaceStyle != previousTraitCollection.UserInterfaceStyle)
                SetTheme();
        }
    }
}
