using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance {
	/// <summary>
	/// <para>
	/// Třída popisující nastavení grafu. Instance může být sdílena několika
	/// instancemi třídy <see cref="PlotDataSource"/>. Obvykle při vykreslování 
	/// několika zdrojů do jednoho <see cref="PlotControl"/> prvku. Na druhou
	/// stranu sdílení instance <see cref="PlotDataAdapter"/> mezi několika
	/// instancemi <see cref="PlotControl"/> vyvolá problémy kvůli systému 
	/// maximálních a minimálních hodnot.
	/// </para><para>
	/// Správu maximálních a minimálních hodnot mají na starosti instance
	/// třídy <see cref="PlotDataSource"/>, které musí před vyhodnocením 
	/// události <see cref="PlotDataSource.DataChanged"/> prohnat všechna nová
	/// data metodou <see cref="CheckMinMax"/>.
	/// </para>
	/// </summary>
	class PlotDataAdapter {
		/// <summary>
		/// Počáteční datum grafu.
		/// </summary>
		public readonly NodaTime.LocalDate startDate;
		/// <summary>
		/// Koncové datum grafu.
		/// </summary>
		public readonly NodaTime.LocalDate endDate;
		/// <summary>
		/// Interval mezi jednotlivými časovými body na ose X.
		/// Obvykle 1 den/týden/měsíc/rok, ale s pomocí <see cref="NodaTime"/>
		/// by měl umožňovat i 2 týdny, 3 měsíce (1/4 roku) a podobně.
		/// </summary>
		public readonly NodaTime.Period interval;

		/// <summary>
		/// Minimální hodnota v grafu. Používá se při přepočítávání na relativní
		/// hodnotu. (viz <see cref="GetRelativeValue"/>)
		/// </summary>
		public decimal MinValue { get; private set; } = 0;
		/// <summary>
		/// Maximální hodnota v grafu. Používá se při přepočítávání na relativní
		/// hodnotu. (viz <see cref="GetRelativeValue"/>)
		/// </summary>
		public decimal MaxValue { get; private set; } = 0;

		public PlotDataAdapter(NodaTime.LocalDate startDate, NodaTime.LocalDate endDate, NodaTime.Period interval) {
			this.startDate = startDate;
			this.endDate = endDate;
			this.interval = interval;
		}

		/// <summary>
		/// Metoda na upravení hodnot <see cref="MinValue"/> a <see cref="MaxValue"/>.
		/// Volaná instancí <see cref="PlotDataSource"/> před vyhodnocením události
		/// <see cref="PlotDataSource.DataChanged"/>.
		/// </summary>
		/// <param name="value">Hodnota pro ověření.</param>
		public void CheckMinMax(decimal value) {
			if(value < MinValue)
				MinValue = value;

			if(value > MaxValue)
				MaxValue = value;
		}
	}
}
