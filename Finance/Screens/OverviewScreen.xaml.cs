using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using Finance.Data;

using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;

namespace Finance.Screens {
	/// <summary>
	/// Interaction logic for OverviewScreen.xaml
	/// </summary>
	public partial class OverviewScreen: UserControl {

		private class OverviewTableCell {
			public string Content { get; init; }
			public bool Bold { get; init; }
			public bool Future { get; init; }
			public bool LeftBorder { get; init; }
			public bool TopBorder { get; init; }
			public bool RightBorder { get; init; }
			public bool BottomBorder { get; init; }

		}
		private readonly List<List<OverviewTableCell>> preparedTable = new List<List<OverviewTableCell>>(16);

		private class OverviewItem {
			public double Total { get; init; }
			public double IntegratedTotal { get; init; }
			public string Label { get; init; }
			public Dictionary<CategoryManager.Category, double> Values { get; init; }
			public Dictionary<CategoryManager.Category, double> IntegratedValues { get; init; }
			public bool Empty { get; init; }
			public bool Future { get; init; }
		}
		public OverviewScreen() {
			InitializeComponent();

			plotView.Model = new OxyPlot.PlotModel();

			fromDatePicker.SelectedDate = DateTime.Now;
			toDatePicker.SelectedDate = DateTime.Now;
		}

		IEnumerable<OverviewItem> EnumerateOverviewItems(){
			if(renderTypeComboBox.SelectedItem == null)
				yield break ;
			if(
				unitTypeComboBox.SelectedItem == null ||
				!fromDatePicker.SelectedDate.HasValue ||
				!toDatePicker.SelectedDate.HasValue
			) yield break;
			var unitInterval = new StatisticsManager.UnitInterval(
				NodaTime.LocalDate.FromDateTime(fromDatePicker.SelectedDate.Value),
				NodaTime.LocalDate.FromDateTime(toDatePicker.SelectedDate.Value),
				(StatisticsManager.UnitType)unitTypeComboBox.SelectedItem
			);


			var nowUnit = ((StatisticsManager.UnitType)unitTypeComboBox.SelectedItem).GetUnit(NodaTime.LocalDate.FromDateTime(DateTime.Now));
			var integratedValues = new Dictionary<CategoryManager.Category, double>();
			var averageValues = new Dictionary<CategoryManager.Category, double>();
			foreach(CategoryManager.Category c in categoryListView.SelectedItems) 
				integratedValues[c] = 0;
			bool future = false;
			decimal integratedTotal = 0;
			decimal averageTotal = 0;
			int countSoFar = 0;
			foreach(var u in unitInterval) {
				decimal total = future ? averageTotal + u.Total : u.Total;
				var values = new Dictionary<CategoryManager.Category, double>();
				foreach(CategoryManager.Category c in categoryListView.SelectedItems) {
					values[c] = future ? (averageValues[c] + (double)u[c]) : (double)u[c] ;
					integratedValues[c] += values[c]; 
				}
				integratedTotal += total;
				yield return new OverviewItem {
					Total = (double)total,
					IntegratedTotal = (double)integratedTotal,
					Values = values,
					IntegratedValues = integratedValues,
					Future = future,
					Empty = u.Count == 0,
					Label = u.Name,
				};

				if(!future) {
					countSoFar++;
				}
				if(u == nowUnit)
					future = true;
				if(u == nowUnit && countSoFar != 0) {
					averageTotal = integratedTotal / countSoFar;
					foreach(CategoryManager.Category c in categoryListView.SelectedItems)
						averageValues[c] = integratedValues[c] / countSoFar;
				}
			}
		}

		public void Render() {
			if(renderTypeComboBox.SelectedItem == null)
				return;
			
			plotView.Visibility = Visibility.Hidden;
			integratedPlotView.Visibility = Visibility.Hidden;
			tableViewScroller.Visibility = Visibility.Hidden;

			switch(((ComboBoxItem)renderTypeComboBox.SelectedItem).Tag) {
				case "g": RenderPlot(); break;
				case "G": RenderIntegratedPlot(); break;
				case "t": RenderTable(); break;
			}
		}

		private void RenderPlot() {
			plotView.Visibility = Visibility.Visible;

			var model = new OxyPlot.PlotModel() {
				LegendPosition = OxyPlot.LegendPosition.RightMiddle,
				LegendPlacement = OxyPlot.LegendPlacement.Outside,
			};
			var axisX = new OxyPlot.Axes.CategoryAxis() {
				Position = OxyPlot.Axes.AxisPosition.Bottom,
				Angle = 50,
				GapWidth = 0.1,
				MajorGridlineStyle = OxyPlot.LineStyle.Solid,
				MinorGridlineStyle = OxyPlot.LineStyle.Solid,
			};
			model.Axes.Add(axisX);
			var axisY = new OxyPlot.Axes.LinearAxis() {
				Position = OxyPlot.Axes.AxisPosition.Left,
				MajorGridlineStyle = OxyPlot.LineStyle.Solid,
				MinorGridlineStyle = OxyPlot.LineStyle.Solid,
			};
			model.Axes.Add(axisY);
			var series = new Dictionary<CategoryManager.Category, OxyPlot.Series.ColumnSeries>();
			foreach(var _c in categoryListView.SelectedItems) {
				var c = (CategoryManager.Category)_c;
				series[c] = new OxyPlot.Series.ColumnSeries() {
					FillColor = c.Color,
					Title = c.Name,
					LabelPlacement = OxyPlot.Series.LabelPlacement.Outside,
				};

				model.Series.Add(series[c]);
			}
			OxyPlot.Series.ColumnSeries totalSeries = null;
			if(totalCheckBox.IsChecked.Value) {
				totalSeries = new OxyPlot.Series.ColumnSeries() {
					FillColor = OxyPlot.OxyColors.White,
					StrokeColor = OxyPlot.OxyColors.Black,
					StrokeThickness = 2,
					Title = "Celkem",
					LabelPlacement = OxyPlot.Series.LabelPlacement.Outside,

				};
				model.Series.Add(totalSeries);
			}
			bool future = false;
			foreach(var oi in EnumerateOverviewItems()) {
				if(!future && oi.Future) {
					model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation() {
						Color = OxyPlot.OxyColors.DarkRed,
						Type = OxyPlot.Annotations.LineAnnotationType.Vertical,
						X = axisX.Labels.Count - 0.5,
						StrokeThickness = 2,
						Text = "hranice současnosti",
					});
					future = true;
				}
				if(!oi.Empty || (includeAverageCheckBox.IsChecked.Value) && future) {
					axisX.Labels.Add(oi.Label);
					if(totalSeries != null) {
						totalSeries.Items.Add(new OxyPlot.Series.ColumnItem(oi.Total));
					}
					foreach(var _c in categoryListView.SelectedItems) {
						var c = (CategoryManager.Category)_c;
						series[c].Items.Add(new OxyPlot.Series.ColumnItem(oi.Values[c]));
					}
				}
			}
			
			plotView.Model = model;
		}

		public void RenderIntegratedPlot() {
			integratedPlotView.Visibility = Visibility.Visible;

			var model = new OxyPlot.PlotModel() {
				LegendPosition = OxyPlot.LegendPosition.RightMiddle,
				LegendPlacement = OxyPlot.LegendPlacement.Outside,
			};
			var axisX = new OxyPlot.Axes.CategoryAxis() {
				Position = OxyPlot.Axes.AxisPosition.Bottom,
				Angle = 50,
				GapWidth = 0.1,
				MajorGridlineStyle = OxyPlot.LineStyle.Solid,
				MinorGridlineStyle = OxyPlot.LineStyle.Solid,
			}; 
			model.Axes.Add(axisX);
			var axisY = new OxyPlot.Axes.LinearAxis() {
				Position = OxyPlot.Axes.AxisPosition.Left,
				MajorGridlineStyle = OxyPlot.LineStyle.Solid,
				MinorGridlineStyle = OxyPlot.LineStyle.Solid,
			};
			model.Axes.Add(axisY);
			var series = new Dictionary<CategoryManager.Category, OxyPlot.Series.LineSeries>();
			foreach(var _c in categoryListView.SelectedItems) {
				var c = (CategoryManager.Category)_c;
				series[c] = new OxyPlot.Series.LineSeries() {
					Color = c.Color,
					Title = c.Name,
				};

				model.Series.Add(series[c]);
			}
			OxyPlot.Series.LineSeries totalSeries = null;
			if(totalCheckBox.IsChecked.Value) {
				totalSeries = new OxyPlot.Series.LineSeries() {
					Color = OxyPlot.OxyColors.Black,
					StrokeThickness = 2,
					Title = "Celkem",
				};
				model.Series.Add(totalSeries);
			}
			bool future = false;
			int x = 0;
			foreach(var oi in EnumerateOverviewItems()) {
				if(!future && oi.Future) {
					model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation() {
						Color = OxyPlot.OxyColors.DarkRed,
						Type = OxyPlot.Annotations.LineAnnotationType.Vertical,
						X = axisX.Labels.Count - 0.5,
						StrokeThickness = 2,
						Text = "hranice současnosti",
					});
					future = true;
				}
				axisX.Labels.Add(oi.Empty ? string.Empty : oi.Label);
				if(totalSeries != null) {
					totalSeries.Points.Add(new OxyPlot.DataPoint(x, oi.IntegratedTotal));
				}
				foreach(var _c in categoryListView.SelectedItems) {
					var c = (CategoryManager.Category)_c;
					series[c].Points.Add(new OxyPlot.DataPoint(x, oi.IntegratedValues[c]));
				}
				x++;
			}

			integratedPlotView.Model = model;
		}

		private void RenderTable() {
			#region prepare table
			int x = 0;
			bool total = totalCheckBox.IsChecked.Value;
			int y = total ? 2 : 1;
			int width = 15;
			Dictionary<CategoryManager.Category, int> categoryRows = new Dictionary<CategoryManager.Category, int>();
			preparedTable.Clear();
			foreach(CategoryManager.Category c in categoryListView.SelectedItems) {
				categoryRows[c] = y++;
			}
			y = 0;
			int firstChunk = 2;
			foreach(OverviewItem oi in EnumerateOverviewItems()) {
				Debug.WriteLine($"{x} {y}");

				if(x == 0) {
					if(firstChunk == 1)
						firstChunk = 0;
					y = preparedTable.Count;
					if(firstChunk == 0) {
						preparedTable.Add(new List<OverviewTableCell>(width));
						preparedTable[y].Add(new OverviewTableCell() {
							Content = "",
							Bold = false,
							Future = false,
							LeftBorder = false,
							BottomBorder = false,
							TopBorder = false,
							RightBorder = false,
						});
						y++;
					}
					preparedTable.Add(new List<OverviewTableCell>(width));
					preparedTable[y].Add(new OverviewTableCell() {
						Content = "",
						Bold = false,
						Future = false,
						LeftBorder = true,
						BottomBorder = true,
						TopBorder = true,
						RightBorder = true,
					});
					if(total) {
						preparedTable.Add(new List<OverviewTableCell>(width));
						preparedTable[y + 1].Add(new OverviewTableCell() {
							Content = "Celkem",
							Bold = true,
							Future = false,
							LeftBorder = true,
							BottomBorder = true,
							TopBorder = false,
							RightBorder = true,
						});
					}
					for(int i = 0; i < categoryRows.Count; i++) {
						preparedTable.Add(new List<OverviewTableCell>(width));
					}
					foreach(var c in categoryRows.Keys) {
						preparedTable[y + categoryRows[c]].Add(new OverviewTableCell() {
							Content = c.Name,
							Bold = true,
							Future = false,
							LeftBorder = true,
							BottomBorder = true,
							TopBorder = false,
							RightBorder = true,
						});
					}
					x++;
					if(firstChunk == 2)
						firstChunk = 1;
				}
				if(firstChunk == 0) {
					preparedTable[y - 1].Add(new OverviewTableCell() {
						Content = "",
						Bold = false,
						Future = false,
						LeftBorder = false,
						BottomBorder = false,
						TopBorder = false,
						RightBorder = false,
					});
				}
				preparedTable[y].Add(new OverviewTableCell() {
					Content = oi.Label,
					Bold = false,
					Future = oi.Future,
					LeftBorder = false,
					BottomBorder = true,
					TopBorder = true,
					RightBorder = true,
				});
				if(total) {
					preparedTable[y + 1].Add(new OverviewTableCell() {
						Content = oi.Total.ToString("0.00"),
						Bold = false,
						Future = oi.Future,
						LeftBorder = false,
						BottomBorder = true,
						TopBorder = false,
						RightBorder = true,
					});
				}
				foreach(var c in categoryRows.Keys) {
					preparedTable[y + categoryRows[c]].Add(new OverviewTableCell() {
						Content = oi.Values[c].ToString("0.00"),
						Bold = false,
						Future = oi.Future,
						LeftBorder = false,
						BottomBorder = true,
						TopBorder = false,
						RightBorder = true,
					});
				}
				x++;
				if(x >= width)
					x = 0;
			}
			#endregion

			#region render table
			tableViewScroller.Visibility = Visibility.Visible;
			tableView.Children.Clear();
			tableView.ColumnDefinitions.Clear();
			tableView.RowDefinitions.Clear();

			Border borderElement;

			if(preparedTable.Count == 0)
				return;
			for(int i = 0; i < preparedTable.Count; i++)
				tableView.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			for(int i = 0; i < preparedTable[0].Count; i++)
				tableView.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

			y = 0;
			foreach(var row in preparedTable) {
				x = 0;
				foreach(var cell in row) {
					borderElement = new Border() {
						Child = new TextBlock() {
							HorizontalAlignment = HorizontalAlignment.Center,
							Inlines = { cell.Bold ? new Bold(new Run(cell.Content)) : new Run(cell.Content) },
						},
						Background = cell.Future ? Brushes.LightCoral : Brushes.Transparent,
						BorderBrush = Brushes.Black,
						Padding = new Thickness(5),
						BorderThickness = new Thickness(cell.LeftBorder ? 2 : 0, cell.TopBorder ? 2 : 0, cell.RightBorder ? 2 : 0, cell.BottomBorder ? 2 : 0),
					};
					borderElement.SetValue(Grid.RowProperty, y);
					borderElement.SetValue(Grid.ColumnProperty, x);
					tableView.Children.Add(borderElement);
					x++;
				}
				y++;
			}
			#endregion
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void unitTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			Render();
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void fromDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
			Render();
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void toDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
			Render();
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void renderTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			Render();
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void categoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			Render();
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void totalCheckBox_CheckedChanged(object sender, RoutedEventArgs e) {
			Render();
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void allCategoriesButton_Click(object sender, RoutedEventArgs e) {
			categoryListView.SelectAll();
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void noCategoriesButton_Click(object sender, RoutedEventArgs e) {
			categoryListView.SelectedItems.Clear();
		}

		private void WriteReport(Stream s) {
			switch(((ComboBoxItem)renderTypeComboBox.SelectedItem).Tag) {
				case "g": {
					var writer = new OxyPlot.Reporting.HtmlReportWriter(s);
					var report = new OxyPlot.Reporting.Report();
					report.AddPlot(plotView.Model, "", 1000, 600);
					writer.WriteReport(report, new OxyPlot.Reporting.ReportStyle());
					writer.Close();
					break;
				}
				case "G": {
					var writer = new OxyPlot.Reporting.HtmlReportWriter(s);
					var report = new OxyPlot.Reporting.Report();
					report.AddPlot(integratedPlotView.Model, "", 1000, 600);
					writer.WriteReport(report, new OxyPlot.Reporting.ReportStyle());
					writer.Close();
					break;
				}
				case "t": {
					using(var writer = new StreamWriter(s)) {
						writer.Write("<!DOCTYPE html><html><head></head><body><table style=\"border-collapse:collapse;\">");
						foreach(var row in preparedTable) {
							writer.Write("<tr>");
							foreach(var cell in row) {
								writer.Write(
									$"<td style=\"" +
									$"background-color:{(cell.Future ? "coral" : "transparent")};" +
									$"border: 0px solid black;" +
									$"border-width:{(cell.TopBorder ? 2 : 0)}px {(cell.RightBorder ? 2 : 0)}px {(cell.BottomBorder ? 2 : 0)}px {(cell.LeftBorder ? 2 : 0)}px;" +
									$"padding:5px;text-align: center;margin:0px;" +
									$"\">" +
									$"{(cell.Bold ? "<b>" + cell.Content + "</b>" : cell.Content)}" +
									$"</td>"
								);
							}
							writer.Write("</tr>");
						}
						writer.Write("</table></body></html>");
					}
					break;
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void saveHTMLButton_Click(object sender, RoutedEventArgs e) {
			SaveFileDialog dialog = new SaveFileDialog() {
				Filter = "Soubor HTML (*.html)|*.html",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
			};
			if(!dialog.ShowDialog().Value)
				return;
			using(var stream = new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Create, FileAccess.Write))
				WriteReport(stream);
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void printButton_Click(object sender, RoutedEventArgs e) {
			var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Temp\\finance-report.html";
			using(var stream = new FileStream(path, FileMode.Create))
				WriteReport(stream);
			using(var fileWriter = new StreamWriter(path, append: true))
				fileWriter.WriteLine("<script>try{document.getElementsByClassName('figureText')[0].remove()}catch(err){};window.print()</script>");
			Process.Start(new ProcessStartInfo() {
				FileName = path,
				UseShellExecute = true,
			});
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void includeAverageCheckBox_CheckedChanged(object sender, RoutedEventArgs e) {
			Render();
		}
	}
}
