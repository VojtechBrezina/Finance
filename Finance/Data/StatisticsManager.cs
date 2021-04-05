using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;


namespace Finance.Data {
	public static class StatisticsManager {
		private static readonly string dataPath;
		private static readonly Queue<string> cacheQueue = new Queue<string>();
		private static readonly Dictionary<string, StatisticUnit> cachedUnits = new Dictionary<string, StatisticUnit>();
		
		static StatisticsManager() {
			dataPath = PathManager.Get("stats");

			foreach(var ut in unitTypes)
				ut.InitPath();
				
		}

		public static void Add(RawDataBatch.Transaction t) {
			foreach(var ut in unitTypes)
				ut.GetUnit(t.Date).Add(t.CategoryId, t.Amount);
		}
		public static void Add(RawDataBatch b) {
			foreach(var t in b.transactions)
				Add(t);
		}

		public static void Substract(RawDataBatch.Transaction t) {
			foreach(var ut in unitTypes)
				ut.GetUnit(t.Date).Substract(t.CategoryId, t.Amount);
		}
		public static void Substract(RawDataBatch b) {
			foreach(var t in b.transactions)
				Substract(t);
		}

		public static readonly ObservableCollection<UnitType> unitTypes = new ObservableCollection<UnitType>();

		public static readonly UnitType dayType = new UnitType("Den", "day",
			(NodaTime.LocalDate date) => $"{date.ToString("dd-MM-yyyy", null)}.txt",
			(NodaTime.LocalDate date) => date.ToString("d/ M/ yyyy", null),
			(NodaTime.LocalDate date) => date,
			NodaTime.Period.FromDays(1)
		);
		public static readonly UnitType weekType = new UnitType("Týden", "week",
			(NodaTime.LocalDate date) => $"{NodaTime.Calendars.WeekYearRules.Iso.GetWeekOfWeekYear(date)}-{date.ToString("yyyy", null)}.txt",
			(NodaTime.LocalDate date) => $"{NodaTime.Calendars.WeekYearRules.Iso.GetWeekOfWeekYear(date)}. týden r. {date.ToString("yyyy", null)}",
			(NodaTime.LocalDate date) => date - NodaTime.Period.FromDays((int)date.DayOfWeek - 1),
			NodaTime.Period.FromWeeks(1)
		);
		public static readonly UnitType monthType = new UnitType("Měsíc", "month",
			(NodaTime.LocalDate date) => $"{date.ToString("MM-yyyy", null)}.txt",
			(NodaTime.LocalDate date) => date.ToString("MMMM yyyy", null),
			(NodaTime.LocalDate date) => date - NodaTime.Period.FromDays(date.Day - 1),
			NodaTime.Period.FromMonths(1)
		);
		public static readonly UnitType yearType = new UnitType("Rok", "year",
			(NodaTime.LocalDate date) => $"{date.ToString("yyyy", null)}.txt",
			(NodaTime.LocalDate date) => date.ToString("yyyy", null),
			(NodaTime.LocalDate date) => date - NodaTime.Period.FromDays(date.DayOfYear - 1),
			NodaTime.Period.FromYears(1)
		);
		public static readonly UnitType allTimeType = new UnitType("Všechen čas", "",
			(NodaTime.LocalDate date) => $"all-time.txt",
			(NodaTime.LocalDate date) => "Celkem",
			(NodaTime.LocalDate date) => NodaTime.LocalDate.MinIsoValue,
			NodaTime.Period.Between(NodaTime.LocalDate.MinIsoValue, NodaTime.LocalDate.MaxIsoValue, NodaTime.PeriodUnits.Ticks)
		);

		public class UnitType {
			public string DisplayName { get; init; }
			public string Path { get; private set; }
			private Func<NodaTime.LocalDate, string> GetPath { get; init; }
			private Func<NodaTime.LocalDate, string> GetName { get; init; }
			public Func<NodaTime.LocalDate, NodaTime.LocalDate> GetBegining { get; init; }
			public NodaTime.Period Period { get; init; }
			public UnitType(string dn, string p, Func<NodaTime.LocalDate, string> gp, Func<NodaTime.LocalDate, string> gn,
				Func<NodaTime.LocalDate, NodaTime.LocalDate> gb, NodaTime.Period pe
			) {
				DisplayName = dn;
				Path = p + "/";
				GetPath = gp;
				GetName = gn;
				GetBegining = gb;
				Period = pe;
				unitTypes.Add(this);
			}
			public StatisticUnit GetUnit(NodaTime.LocalDate date) {
				string path = GetPath(date);
				if(cachedUnits.ContainsKey(path))
					return cachedUnits[path];
				if(cacheQueue.Count >= 50) {
					string toRemove = cacheQueue.Dequeue();
					cachedUnits[toRemove].Dispose();
					cachedUnits.Remove(toRemove);
				}
				cachedUnits[path] = new StatisticUnit(this, path, GetName(date), date);
				return cachedUnits[path];
			}

			internal void InitPath() {
				Path = dataPath + Path;
				Directory.CreateDirectory(Path);
			}
		}

		public sealed class StatisticUnit: IDisposable {
			private readonly Dictionary<int, decimal> categorySums = new Dictionary<int, decimal>();
			public decimal Total { get; private set; }

			public decimal this[int c] => categorySums.ContainsKey(c) ?  categorySums[c] : 0;

			public decimal this[CategoryManager.Category c] => this[c.Id];

			public int Count { get; private set; }

			public NodaTime.LocalDate BeginingDate { get; init; }
			public NodaTime.LocalDate EndDate { get => BeginingDate + Type.Period - NodaTime.Period.FromDays(1); }

			public void Add(int c, decimal a) {
				if(!categorySums.ContainsKey(c))
					categorySums[c] = 0;
				categorySums[c] += a;
				Total += a;
				Count++;
			}

			public void Substract(int c, decimal a) {
				if(!categorySums.ContainsKey(c))
					categorySums[c] = 0;
				categorySums[c] -=a;
				Total -= a;
				Count--;
			}

			public UnitType Type { get; init; }

			private string FilePath { get; init; }
			public string Name { get; init; }

			public StatisticUnit(UnitType type, string path, string name, NodaTime.LocalDate date) {
				FilePath = type.Path + path;
				Name = name;
				Type = type;
				BeginingDate = type.GetBegining(date);

				if(File.Exists(FilePath)) {
					using(var reader = new StreamReader(FilePath)) {
						Total = decimal.Parse(reader.ReadLine());
						Count = int.Parse(reader.ReadLine());
						while(!reader.EndOfStream) {
							int c = int.Parse(reader.ReadLine());
							c = CategoryManager.Get(c).Id;
							categorySums[c] = decimal.Parse(reader.ReadLine());
						}
					}
				} else {
					Total = 0;
					Count = 0;
				}
			}

			public void Save() {
				using(var writer = new StreamWriter(FilePath)) {
					writer.WriteLine(Total);
					writer.WriteLine(Count);
					foreach(var c in categorySums.Keys) {
						writer.WriteLine(c);
						writer.WriteLine(categorySums[c]);
					}
				}
			}

			public static bool operator ==(StatisticUnit u1, StatisticUnit u2) {
				return u1.FilePath == u2.FilePath;
			}
			public static bool operator !=(StatisticUnit u1, StatisticUnit u2) {
				return u1.FilePath != u2.FilePath;
			}

			public void Dispose() {
				Save();
			}

			public override bool Equals(object obj) {
				if(obj.GetType() != typeof(StatisticUnit))
					return false;
				return this == (StatisticUnit)obj;
			}

			public override int GetHashCode() {
				return HashCode.Combine(FilePath);
			}
		}

		public class UnitInterval: IEnumerable<StatisticUnit> {
			private readonly NodaTime.LocalDate from;
			private readonly NodaTime.LocalDate to;
			private readonly UnitType type;

			public UnitInterval(NodaTime.LocalDate from, NodaTime.LocalDate to, UnitType type) {
				this.from = from;
				this.to = to;
				this.type = type;
			}

			public IEnumerator<StatisticUnit> GetEnumerator() {
				var date = from;
				while(date <= to) {
					yield return type.GetUnit(date);
					date += type.Period;
				}
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}
		}

		public static void Save() {
			foreach(var v in cachedUnits) {
				v.Value.Save();
			}
		}
	}
}
