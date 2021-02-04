using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Finance {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow: Window {
		private DockPanel contentPanel;

		public readonly TitleScreen titleScreen;

		public MainWindow() {
			InitializeComponent();
			contentPanel = (DockPanel)Content;

			titleScreen = new TitleScreen(this);

			SwitchScreens(titleScreen);
		}

		private void SwitchScreens(UIElement screenElement) {
			if(contentPanel.Children.Count == 2)
				contentPanel.Children.RemoveAt(1);
			contentPanel.Children.Add(screenElement);
		}

		private void ExitCallBack(object sender, RoutedEventArgs e) {
			Close();
		}
	}
}
