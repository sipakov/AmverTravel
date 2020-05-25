using System;
using Amver.MobileClient.Styles;
using Android.Content;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ContentPage), typeof(Amver.MobileClient.Android.Renderers.ContentPageRenderer))]
namespace Amver.MobileClient.Android.Renderers
{
    public class ContentPageRenderer : PageRenderer
    {
        public ContentPageRenderer(Context context) : base(context)
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
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
                App.Current.Resources = new LightTheme();
            }

         
    }
}
