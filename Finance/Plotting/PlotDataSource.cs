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

		/// <summary>
		/// Cache na data z tohoto zdroje. Automaticky se naplní před
		/// vyhodnocením události <see cref="PlotDataSource.DataChanged"/>.
		/// Obsahuje <see langword="null"/> pro místa, pro která nebyla
		/// nalezena žádná data.
		/// </summary>
		public decimal?[] Data { get; private set; } = null;

		protected PlotDataSource() {

		}

		protected virtual void OnDataChanged() {
			PopulateDataCache();
			DataChanged?.Invoke(this, new EventArgs());
		}

		protected abstract void PopulateDataCache();
	}
}
