using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Text;

namespace Finance {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App: Application {
		public App() {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			#region CSV testing
			/*Debug.WriteLine("test");
			var stuff = new Data.Import.MBankCSVImporter();
			foreach(var ei in Encoding.GetEncodings()) {
				Debug.WriteLine(ei.Name);
			}
			stuff.Import(@"C:\Users\vojta\Documents\test.csv", Encoding.GetEncoding(1250));*/
			#endregion

			#region report testing
			/*
			var s = new System.IO.FileStream("test.html", System.IO.FileMode.OpenOrCreate);
			var e = new OxyPlot.Reporting.HtmlReportWriter(s);
			var r = new OxyPlot.Reporting.Report();
			var h = new OxyPlot.Reporting.Header();
			var t = new OxyPlot.Reporting.Table();
			var tr = new OxyPlot.Reporting.TableRow();
			var tc = new OxyPlot.Reporting.TableCell();
			tc.Content = "test";
			tr.Cells.Add(tc);
			t.Rows.Add(tr);
			h.Text = "Test";
			r.AddHeader(1,"Test");
			r.AddParagraph("Longer test");

			var b = new System.Windows.Controls.WebBrowser();

			e.WriteReport(r, new OxyPlot.Reporting.ReportStyle());
			e.Close();
			s.Close();

			new Process {
				StartInfo = new ProcessStartInfo("test.html") {
					UseShellExecute = true
				}
			}.Start();
			*/
			#endregion
		}
	}
}
