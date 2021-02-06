using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance {
	/// <summary>
	/// Třída pro poskytování dat ovládacímu prvku
	/// <see cref="PlotControl"/>. Je zodpovědná za
	/// optimalizaci přístupu k datům a předávání 
	/// událostí popisujících jejich změny.
	/// </summary>
	abstract class PlotDataSource {
		public event EventHandler DataChanged;
		

		protected virtual void OnDataChanged() {
			DataChanged?.Invoke(this, new EventArgs());
		}
	}
}
