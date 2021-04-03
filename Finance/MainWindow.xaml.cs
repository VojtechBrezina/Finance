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

using Microsoft.Windows.Input;

using Finance.Screens;
using Finance.Data;

namespace Finance {
	/// <summary>
	/// Hlavní okno aplikace obsahující jednoduché menu a prostor
	/// pro navigaci mezi jednotlivými obrazovkami
	/// </summary>
	public partial class MainWindow: Window {
		public readonly TitleScreen titleScreen = new TitleScreen();
		public readonly DataInputScreen dataInputScreen = new DataInputScreen();
		public readonly OverviewScreen overviewScreen = new OverviewScreen();
		public readonly AboutScreen aboutScreen = new AboutScreen();

		private UserControl currentScreen;

		public MainWindow() {
			InitializeComponent();
			((DockPanel)Content).Children.Add(titleScreen);
			currentScreen = titleScreen;
		}

		private void SetScreen(UserControl screen) {
			((DockPanel)Content).Children.Remove(currentScreen);
			((DockPanel)Content).Children.Add(screen);
			currentScreen = screen;
		}

		#region příkaz Close
		private void Close_CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
			Close();
		}

		private void Close_CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}

		#endregion

		private void datInputMenuItem_Click(object sender, RoutedEventArgs e) {
			SetScreen(dataInputScreen);
		}

		private void aboutMenuItem_Click(object sender, RoutedEventArgs e) {
			SetScreen(aboutScreen);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			CategoryManager.Save();
			StatisticsManager.Save();
		}

		private void overviewMenuItem_Click(object sender, RoutedEventArgs e) {
			SetScreen(overviewScreen);
		}
	}
}
