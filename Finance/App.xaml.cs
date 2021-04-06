using System.Windows;
using System.Text;
using System.Windows.Markup;
using System.Globalization;

namespace Finance {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App: Application {
		public App() {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			FrameworkElement.LanguageProperty.OverrideMetadata(
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(
			XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
		}
	}
}
