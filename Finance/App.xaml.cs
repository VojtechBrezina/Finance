using System.Windows;
using System.Text;

namespace Finance {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App: Application {
		public App() {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
	}
}
