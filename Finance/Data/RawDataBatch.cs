using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Windows;

namespace Finance.Data {
	public class RawDataBatch : DependencyObject{
		private static readonly string dataPath;
		private static uint lastBatchIndex;

		static RawDataBatch() {
			dataPath = PathManager.Get("raw");

			if(Directory.Exists(dataPath)) {
				using(var reader = new StreamReader(dataPath + "last_index.txt")) {
					lastBatchIndex = uint.Parse(reader.ReadLine());
				}
			} else {
				Directory.CreateDirectory(dataPath);
				lastBatchIndex = 0;
			}
		}

		private static uint NextIndex() {
			lastBatchIndex++;
			using(var writer = new StreamWriter(dataPath + "last_index.txt")) {
				writer.WriteLine(lastBatchIndex);
			}
			return lastBatchIndex;
		}

		public static RawDataBatch Create() {
			return new RawDataBatch(NextIndex());
		}

		public static RawDataBatch Load(uint index) {
			var b = new RawDataBatch(index);
			b.Load();
			return b;
		}

		private readonly uint index;
		public ObservableCollection<Transaction> transactions = new ObservableCollection<Transaction>();
		public readonly Dictionary<int, int> lookup = new Dictionary<int, int>();
		private string FilePath {
			get {
				return dataPath + "batch_" + index + ".txt";
			}
		}

		private RawDataBatch(uint index) {
			this.index = index;
			                                 
		}

		private void Load() {
			using(StreamReader reader = new StreamReader(FilePath)) {
				var pattern = NodaTime.Text.LocalDatePattern.Create("d", CultureInfo.InvariantCulture);
				while(!reader.EndOfStream) {
					Transaction t = new Transaction {
						Date = pattern.Parse(reader.ReadLine()).Value,
						Amount = decimal.Parse(reader.ReadLine()),
						Description = reader.ReadLine(),
						CategoryId = int.Parse(reader.ReadLine()),
						InternalId = transactions.Count
					};
					lookup[transactions.Count] = transactions.Count;
					transactions.Add(t);
				}
			}
		}

		public void Save() {
			StreamWriter writer = new StreamWriter(FilePath);
			foreach(var t in transactions) {
				writer.WriteLine(t.Date.ToString("d", CultureInfo.InvariantCulture));
				writer.WriteLine(t.Amount.ToString());
				writer.WriteLine(t.Description);
				writer.WriteLine(t.CategoryId.ToString());
			}
			writer.Close();
		}

		public void Add(Transaction t) {
			transactions.Add(t);
		}

		public Transaction GetById(int id) {
			return transactions[lookup[id]];
		}

		public class Transaction: DependencyObject {
			public static readonly DependencyProperty dateProperty = 
				DependencyProperty.Register("Date", typeof(NodaTime.LocalDate), typeof(Transaction));
			public static readonly DependencyProperty amountProperty =
				DependencyProperty.Register("Amount", typeof(decimal), typeof(Transaction));
			public static readonly DependencyProperty descriptionProperty =
				DependencyProperty.Register("Description", typeof(string), typeof(Transaction));
			public static readonly DependencyProperty categoryProperty =
				DependencyProperty.Register("Category", typeof(CategoryManager.Category), typeof(Transaction));

			public NodaTime.LocalDate Date { get => (NodaTime.LocalDate)GetValue(dateProperty); set => SetValue(dateProperty, value); }
			public decimal Amount { get => (decimal)GetValue(amountProperty); set => SetValue(amountProperty, value); }
			public string Description { get => (string)GetValue(descriptionProperty); set => SetValue(descriptionProperty, value); }
			public int CategoryId { get => Category.Id; set => Category = CategoryManager.Get(value); }
			public CategoryManager.Category Category { get => (CategoryManager.Category)GetValue(categoryProperty); set => SetValue(categoryProperty, value); }

			public string DisplayDate {
				get {
					return Date.ToString("d. M. yyyy", null);
				}
			}

			public int InternalId { get; init; }
		}
	}
}
