using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Finance.Screens;
using Finance.Data;
using System.Diagnostics.CodeAnalysis;

namespace Finance {
	/// <summary>
	/// Hlavní okno aplikace obsahující jednoduché menu a prostor
	/// pro navigaci mezi jednotlivými obrazovkami
	/// </summary>
	public partial class MainWindow: Window {
		public readonly DataInputScreen dataInputScreen = new DataInputScreen();
		public readonly OverviewScreen overviewScreen = new OverviewScreen();
		public readonly AboutScreen aboutScreen = new AboutScreen();

		private UserControl currentScreen;

		public MainWindow() {
			InitializeComponent();
			((DockPanel)Content).Children.Add(overviewScreen);
			currentScreen = overviewScreen;
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

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void datInputMenuItem_Click(object sender, RoutedEventArgs e) {
			SetScreen(dataInputScreen);
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void aboutMenuItem_Click(object sender, RoutedEventArgs e) {
			SetScreen(aboutScreen);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			CategoryManager.Save();
			StatisticsManager.Save();
			RegularTranactionManager.Save();
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void overviewMenuItem_Click(object sender, RoutedEventArgs e) {
			SetScreen(overviewScreen);
		}
	}
}
