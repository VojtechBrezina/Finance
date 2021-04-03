using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Finance.Data.Import {
	class MBankCSVImporter: CSVImporter {
		public MBankCSVImporter() : base(";", "\"'") {
			DisplayName = "Mbank CSV výpis";
			FilterString = "Tabulka ve formátu CSV|*.csv";
		}

		public override void Import(string path, Encoding encoding = null) {
			base.Import(path, encoding ?? Encoding.GetEncoding(1250));
			int dataLength = -1;
			int lineIndex;
			for(lineIndex = 0; lineIndex < lines.Count; lineIndex++) {
				if(lines[lineIndex][0].Equals("Součet")) {
					dataLength = int.Parse(lines[lineIndex][1]);
					break;
				}
			}

			for(; !lines[lineIndex][0].Equals("#Datum uskutečnění transakce"); lineIndex++) 
				;
			lineIndex++;

			var pattern = NodaTime.Text.LocalDatePattern.Create("dd-MM-yyyy", CultureInfo.InvariantCulture);
			for(; lines[lineIndex].Count > 1; lineIndex++) {
				var t = new RawDataBatch.Transaction {
					Date = pattern.Parse(lines[lineIndex][0]).Value,
					Amount = decimal.Parse(lines[lineIndex][9]),
					Description = lines[lineIndex][2] + "; Zpráva: " + lines[lineIndex][3],
					CategoryId = CategoryManager.Assign(new Dictionary<CategoryManager.AutomationField, string>() {
						[CategoryManager.descriptionField] = lines[lineIndex][2],
						[CategoryManager.messageField] = lines[lineIndex][3],
						[CategoryManager.payerRecieverField] = lines[lineIndex][4],
						[CategoryManager.accountNumberField] = lines[lineIndex][5],
						[CategoryManager.ksField] = lines[lineIndex][6],
						[CategoryManager.vsField] = lines[lineIndex][7],
						[CategoryManager.ssField] = lines[lineIndex][8],
						[CategoryManager.amountField] = lines[lineIndex][9]
					}).Id
				};
				Batch.Add(t);
			}
			Debug.Assert(Batch.transactions.Count == dataLength);
		}
	}
}
