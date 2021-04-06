using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Data {
	public static class RegularTranactionManager {
		private static readonly string filePath;
		public static ObservableCollection<RegularTransaction> Transactions { get; private set; } = new ObservableCollection<RegularTransaction>();

		public class RegularTransaction {
			public int CategoryID { get; set; }
			public CategoryManager.Category Category { get => CategoryManager.Get(CategoryID); set => CategoryID = value.Id; }
			public decimal Amount { get; set; } = 0;
			public NodaTime.LocalDate StartDate { get; set; }
			public bool IncludeInThePast { get; set; } = false;
			public RepeatPeriod Period { get; set; }
			public readonly List<int> days = new List<int>();
			public string SerializedDays {
				get => string.Join(',', days);
				set {
					var _d = value.Split(',');
					days.Clear();
					foreach(var ds in _d) {
						int v = 0;
						int.TryParse(ds, out v);
						if(v < 1) break;
						days.Add(v);
					}
				}
			}

			public RegularTransaction(RepeatPeriod p, string d) {
				Period = p;
				SerializedDays = d;
			}

			public int GetCountIn(StatisticsManager.StatisticUnit unit, NodaTime.LocalDate begining) {
				//Debug.WriteLine($"{unit.BeginingDate} - {unit.EndDate} / {begining} {Period} {SerializedDays}");
				begining = NodaTime.LocalDate.Max(unit.BeginingDate, begining);
				var end = NodaTime.LocalDate.Max(unit.EndDate, begining + NodaTime.Period.FromDays(1));
				var nodaPeriod = NodaTime.Period.Between(begining, unit.EndDate + NodaTime.Period.FromDays(1), (NodaTime.PeriodUnits)Period | NodaTime.PeriodUnits.Days);
				int count = 0;
				switch(Period) {
					case RepeatPeriod.Day: return nodaPeriod.Days;
					case RepeatPeriod.Week: count = nodaPeriod.Weeks; break;
					case RepeatPeriod.Month: count = nodaPeriod.Months; break;
					case RepeatPeriod.Year: count = nodaPeriod.Years; break;
				}
				int endOffset = 0;
				NodaTime.LocalDate cleanDate = begining + (nodaPeriod - NodaTime.Period.FromDays(nodaPeriod.Days));
				int intervalLength = 0;
				switch(Period) {
					case RepeatPeriod.Week:
						endOffset = (int)cleanDate.DayOfWeek;
						intervalLength = 7;
						break;
					case RepeatPeriod.Month:
						endOffset = cleanDate.Day;
						intervalLength = (((cleanDate - NodaTime.Period.FromDays(cleanDate.Day - 1)) + NodaTime.Period.FromMonths(1)) - NodaTime.Period.FromDays(1)).Day;
						break;
					case RepeatPeriod.Year:
						endOffset = cleanDate.DayOfYear;
						intervalLength = (((cleanDate - NodaTime.Period.FromDays(cleanDate.DayOfYear - 1)) + NodaTime.Period.FromYears(1)) - NodaTime.Period.FromDays(1)).DayOfYear;
						break;
				}
				Debug.Assert(intervalLength != 0);
				foreach(var d in days) {
					Debug.WriteLine($"{d} {endOffset} {endOffset + nodaPeriod.Days} {endOffset - intervalLength} {endOffset + nodaPeriod.Days - intervalLength} {intervalLength}");
					if(
						(d >= endOffset && d < endOffset + nodaPeriod.Days) ||
						(d >= (endOffset - intervalLength) && d < (endOffset + nodaPeriod.Days - intervalLength))
					)
						count++;
				}
				return count;
			}
		}

		public enum RepeatPeriod{
			Day = NodaTime.PeriodUnits.Days,
			Week = NodaTime.PeriodUnits.Weeks,
			Month = NodaTime.PeriodUnits.Months,
			Year = NodaTime.PeriodUnits.Years,
		}

		static RegularTranactionManager() {
			filePath = PathManager.Get() + "regular-transactions.txt";
			if(File.Exists(filePath)) {
				using(var reader = new StreamReader(filePath)) {
					while(!reader.EndOfStream) {
						var categoryId = int.Parse(reader.ReadLine());
						var amount = decimal.Parse(reader.ReadLine());
						var startDate = NodaTime.Text.LocalDatePattern.FullRoundtrip.Parse(reader.ReadLine()).Value;
						var includeInThePast = bool.Parse(reader.ReadLine());
						var p = (RepeatPeriod)int.Parse(reader.ReadLine());
						var d = reader.ReadLine();
						Transactions.Add(new RegularTransaction(p, d) {
							CategoryID = categoryId,
							Amount = amount,
							StartDate = startDate,
							IncludeInThePast = includeInThePast,
						});
					}
				}
			}
		}

		public static void Save() {
			using(var writer = new StreamWriter(filePath)) {
				foreach(var rt in Transactions) {
					writer.WriteLine(rt.CategoryID);
					writer.WriteLine(rt.Amount);
					writer.WriteLine(rt.StartDate.ToString("r", null));
					writer.WriteLine(rt.IncludeInThePast);
					writer.WriteLine((int)rt.Period);
					writer.WriteLine(rt.SerializedDays);
				}
			}
		}
	}
}
