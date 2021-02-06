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
	/// Ovládací prvek pro zobrazování nastavitelných grafů.
	/// Pro některé konkrétní věci bude pohodlnější použít
	/// třídy, které z něj dědí.
	/// </summary>
	public partial class PlotControl: UserControl {
		/// <summary>
		/// Barva pozadí je uložená v proměnné, pro případnou budoucí rimplementaci umožňující uživatelská nastavení.
		/// </summary>
		private Color backgroundColor = Colors.White;

		public PlotControl() {
			InitializeComponent();
		}


	}
}
