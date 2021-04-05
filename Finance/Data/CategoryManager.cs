using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;

namespace Finance.Data {
	public static class CategoryManager {
		public static readonly string filePath = PathManager.Get() + "categories.txt";
		private static readonly Dictionary<int, Category> lookup = new Dictionary<int, Category>();

		public static readonly Dictionary<int, AutomationField> automationFieldLookup = new Dictionary<int, AutomationField>();
		public static readonly ObservableCollection<AutomationField> fields = new ObservableCollection<AutomationField>();

		public static readonly AutomationField descriptionField = new AutomationField("Popis transakce", 0);
		public static readonly AutomationField messageField = new AutomationField("Zpráva pro příjemce", 1);
		public static readonly AutomationField payerRecieverField = new AutomationField("Plátce/Příjemce", 2);
		public static readonly AutomationField accountNumberField = new AutomationField("Číslo účtu plátce/příjemce", 3);
		public static readonly AutomationField ksField = new AutomationField("Konstantí symbol", 4);
		public static readonly AutomationField vsField = new AutomationField("Variabilní symbol", 5);
		public static readonly AutomationField ssField = new AutomationField("Specifický symbol", 6);
		public static readonly AutomationField amountField = new AutomationField("Částka transakce", 7);

		public static ObservableCollection<Category> Categories { get; private set; }
		private static int highestId = 0;
		private static readonly bool ready = false;

		static CategoryManager() {
			Categories = new ObservableCollection<Category>();

			if(File.Exists(filePath)) {
				using(var reader = new StreamReader(filePath)) {
					while(!reader.EndOfStream) {
						var c = Set(int.Parse(reader.ReadLine()), reader.ReadLine(), OxyPlot.OxyColor.Parse(reader.ReadLine()));
						highestId = Math.Max(highestId, c.Id);
						int ruleCount = int.Parse(reader.ReadLine());
						for(int i = 0; i < ruleCount; i++) {
							c.Rules.Add(new AutomationRule {
								FieldId = int.Parse(reader.ReadLine()),
								ExactMatch = bool.Parse(reader.ReadLine()),
								Value = reader.ReadLine()
							}) ;
						}
					}
				}
			} else {
				Set(-1, "Nezařazené", OxyPlot.OxyColor.Parse("#777777"));
			}

			Categories.CollectionChanged += Categories_CollectionChanged;

			ready = true;
		}

		private static void Categories_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			Save();
		}

		public static void Save() {
			using(var writer = new StreamWriter(filePath)) {
				foreach(var c in lookup.Values) {
					writer.WriteLine(c.Id);
					writer.WriteLine(c.Name);
					writer.WriteLine(c.Color.ToString());
					writer.WriteLine(c.Rules.Count);
					foreach(var r in c.Rules) {
						writer.WriteLine(r.FieldId);
						writer.WriteLine(r.ExactMatch);
						writer.WriteLine(r.Value);
					}
				}
			}
		}


		public static Category Set(int id, string name, OxyPlot.OxyColor color) {
			if(lookup.ContainsKey(id))
				Categories.Remove(lookup[id]);
			lookup[id] = new Category { Id = id, Name = name, Color = color, Rules = new ObservableCollection<AutomationRule>() };
			if(ready)
				Save();
			Categories.Add(lookup[id]);
			return lookup[id];
		}

		public static Category Add(string name, OxyPlot.OxyColor color) {
			return Set(++highestId, name, color);
		}

		public static Category Get(int id) {
			if(lookup.ContainsKey(id))
				return lookup[id];
			return Get(-1);
		}

		public static void Remove(Category c) {
			Remove(c.Id);
		}

		public static void Remove(int id) {
			Categories.Remove(lookup[id]);
			lookup.Remove(id);
		}

		public static Category Assign(Dictionary<AutomationField, string> fields) {
			var scores = new Dictionary<Category, float>();
			foreach(var c in Categories) {
				scores[c] = 0;
				foreach(var r in c.Rules) {
					if(!fields.ContainsKey(r.Field))
						continue;
					if(
						(r.ExactMatch && fields[r.Field].Equals(r.Value)) ||
						fields[r.Field].Contains(r.Value, StringComparison.CurrentCultureIgnoreCase)
					) {
						scores[c] += 1f / c.Rules.Count;
					}
				}
			}
			var maxScore = 0f;
			Category result = lookup[-1];
			foreach(var c in Categories) {
				if(scores[c] > maxScore) {
					maxScore = scores[c];
					result = c;
				}
			}
			return maxScore == 0f ? lookup[-1] : result;
		}

		public class Category {
			public int Id { get; set; }
			public string Name { get; set; }
			public OxyPlot.OxyColor Color { get; set; }
			public ObservableCollection<AutomationRule> Rules { get; init; } = new ObservableCollection<AutomationRule>();
		}

		public class AutomationRule {
			public AutomationField Field { get => automationFieldLookup[FieldId]; set => FieldId = value.Id; }
			public int FieldId { get; set; } = 0;
			public bool ExactMatch { get; set; } = false;
			public string Value { get; set; } = string.Empty;
		}

		public class AutomationField {
			public string Name { get; init; }
			public int Id { get; init; }

			public AutomationField(string name, int id) {
				Name = name;
				Id = id;
				automationFieldLookup[Id] = this;
				fields.Add(this);
			}
		}
	}
}
