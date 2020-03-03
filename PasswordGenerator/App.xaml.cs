
using System.Globalization;
using System.Threading;
using System.Windows;

namespace PasswordGenerator
{
    
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string language = PasswordGenerator.Properties.Settings.Default.Language;
            if (!string.IsNullOrEmpty(language))
            {
                CultureInfo ci = CultureInfo.GetCultureInfo(language);
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag)));
            }
        }
    }
}
