using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
			public CategoryManager.Category Category { get => CategoryManager.Get(CategoryID); }
			public decimal Amount { get; set; }
			public NodaTime.LocalDate StartDate { get; set; }
			public bool IncludeInThePast { get; set; }
			public readonly RepeatPeriod period;
			public readonly List<int> days;

			public RegularTransaction(RepeatPeriod p, string d) {
				period = p;
				var _d = d.Split(',');
				foreach(var ds in _d) {
					int v = int.Parse(ds);
					if(v < 1) throw new ArgumentOutOfRangeException();
					days.Add(v);
				}
			}

			public int GetCountIn(StatisticsManager.StatisticUnit unit, NodaTime.LocalDate begining) {
				begining = NodaTime.LocalDate.Max(unit.BeginingDate, begining);
				var nodaPeriod = NodaTime.Period.Between(begining, unit.EndDate, (NodaTime.PeriodUnits)period | NodaTime.PeriodUnits.Days);
				int count = 0;
				switch(period) {
					case RepeatPeriod.Day: return nodaPeriod.Days;
					case RepeatPeriod.Week: count = nodaPeriod.Weeks; break;
					case RepeatPeriod.Month: count = nodaPeriod.Months; break;
					case RepeatPeriod.Year: count = nodaPeriod.Years; break;
				}
				int endOffset = 0;
				NodaTime.LocalDate cleanDate = begining + (nodaPeriod - NodaTime.Period.FromDays(nodaPeriod.Days));
				int intervalLength = 0;
				switch(period) {
					case RepeatPeriod.Week:
						endOffset = (int)cleanDate.DayOfWeek;
						intervalLength = 7;
						break;
					case RepeatPeriod.Month:
						endOffset = cleanDate.Day;
						intervalLength = (begining - NodaTime.Period.FromDays(begining.Day) + NodaTime.Period.FromMonths(1) - NodaTime.Period.FromDays(1)).Day;
						break;
					case RepeatPeriod.Year:
						endOffset = cleanDate.DayOfYear;
						intervalLength = (begining - NodaTime.Period.FromDays(begining.DayOfYear) + NodaTime.Period.FromYears(1) - NodaTime.Period.FromDays(1)).DayOfYear;
						break;
				}
				foreach(var d in days) {
					if(
						(d > endOffset && d <= endOffset + nodaPeriod.Days) ||
						(d > (endOffset - intervalLength) && d <= (endOffset + nodaPeriod.Days - intervalLength))
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
			foreach(var rt in Transactions) {
				using(var writer = new StreamWriter(filePath)) {
					writer.WriteLine(rt.CategoryID);
					writer.WriteLine(rt.Amount);
					writer.WriteLine(rt.StartDate.ToString("r", null));
					writer.WriteLine(rt.IncludeInThePast);
					writer.WriteLine((int)rt.period);
					writer.WriteLine(string.Join(',', rt.days));
				}
			}
		}
	}
}
