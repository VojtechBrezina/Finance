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

namespace Finance {
	/// <summary>
	/// Hlavní okno aplikace obsahující jednoduché menu a prostor
	/// pro navigaci mezi jednotlivými obrazovkami
	/// </summary>
	public partial class MainWindow: Window {

		public MainWindow() {
			InitializeComponent();
		}

		#region příkaz Close
		private void Close_CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
			Close();
		}

		private void Close_CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute = true;
		}
		#endregion
	}
}
