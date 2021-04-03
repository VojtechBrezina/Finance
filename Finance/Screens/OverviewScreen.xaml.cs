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

using Finance.Data;

namespace Finance.Screens {
	/// <summary>
	/// Interaction logic for OverviewScreen.xaml
	/// </summary>
	public partial class OverviewScreen: UserControl {
		public OverviewScreen() {
			InitializeComponent();

			plotView.Model = new OxyPlot.PlotModel();

			fromDatePicker.SelectedDate = DateTime.Now;
			toDatePicker.SelectedDate = DateTime.Now;
		}

		private void Render() {
			if(renderTypeComboBox.SelectedItem == null)
				return;

			plotView.Visibility = Visibility.Hidden;
			integratedPlotView.Visibility = Visibility.Hidden;
			tableViewScroller.Visibility = Visibility.Hidden;
			if(
				unitTypeComboBox.SelectedItem == null ||
				!fromDatePicker.SelectedDate.HasValue ||
				!toDatePicker.SelectedDate.HasValue
			) return;
			StatisticsManager.UnitInterval unitInterval = new StatisticsManager.UnitInterval(
				NodaTime.LocalDate.FromDateTime(fromDatePicker.SelectedDate.Value),
				NodaTime.LocalDate.FromDateTime(toDatePicker.SelectedDate.Value),
				(StatisticsManager.UnitType)unitTypeComboBox.SelectedItem
			);

			switch(((ComboBoxItem)renderTypeComboBox.SelectedItem).Tag) {
				case "g": RenderPlot(unitInterval); break;
				case "G": RenderIntegratedPlot(unitInterval); break;
				case "t": RenderTable(unitInterval); break;
			}
		}

		private void RenderPlot(StatisticsManager.UnitInterval unitInterval) {
			plotView.Visibility = Visibility.Visible;

			var model = new OxyPlot.PlotModel() {
				LegendPosition = OxyPlot.LegendPosition.RightMiddle,
				LegendPlacement = OxyPlot.LegendPlacement.Outside,
			};
			var axisX = new OxyPlot.Axes.CategoryAxis() {
				Position = OxyPlot.Axes.AxisPosition.Bottom,
				Angle = 50,
				GapWidth = 0.1,
				MajorGridlineStyle = OxyPlot.LineStyle.Automatic
			};
			var nowUnit = ((StatisticsManager.UnitType)unitTypeComboBox.SelectedItem).GetUnit(NodaTime.LocalDate.FromDateTime(DateTime.Now));
			foreach(var u in unitInterval) {
				if(u == nowUnit)
					model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation() {
						Color = OxyPlot.OxyColors.DarkRed,
						Type = OxyPlot.Annotations.LineAnnotationType.Vertical,
						X = axisX.Labels.Count + 0.5,
						StrokeThickness = 2,
						ToolTip = "hranice současnosti",
					});
				axisX.Labels.Add(u.Name);
			}
			model.Axes.Add(axisX);
			var axisY = new OxyPlot.Axes.LinearAxis() {
				Position = OxyPlot.Axes.AxisPosition.Left,
				MajorGridlineStyle = OxyPlot.LineStyle.Automatic,
				MinorGridlineStyle = OxyPlot.LineStyle.Automatic,
			};
			model.Axes.Add(axisY);

			if(totalCheckBox.IsChecked.Value) {
				var totalSeries = new OxyPlot.Series.ColumnSeries() {
					FillColor = OxyPlot.OxyColors.White,
					StrokeColor = OxyPlot.OxyColors.Black,
					StrokeThickness = 2,
					Title = "Celkem",
					LabelPlacement = OxyPlot.Series.LabelPlacement.Outside,
					
				};
				foreach(var u in unitInterval) {
					totalSeries.Items.Add(new OxyPlot.Series.ColumnItem((double)u.Total));
				}
				model.Series.Add(totalSeries);
			}

			foreach(var _c in categoryListView.SelectedItems) {
				var c = (CategoryManager.Category)_c;
				var series = new OxyPlot.Series.ColumnSeries() {
					FillColor = c.Color,
					Title = c.Name,
					LabelPlacement = OxyPlot.Series.LabelPlacement.Outside,
				};
				foreach(var u in unitInterval) {
					series.Items.Add(new OxyPlot.Series.ColumnItem((double)u[c]));
				}
				model.Series.Add(series);
			}
			
			plotView.Model = model;
		}

		public void RenderIntegratedPlot(StatisticsManager.UnitInterval unitInterval) {
			integratedPlotView.Visibility = Visibility.Visible;

			var model = new OxyPlot.PlotModel() {
				LegendPosition = OxyPlot.LegendPosition.RightMiddle,
				LegendPlacement = OxyPlot.LegendPlacement.Outside,
			};
			var axisX = new OxyPlot.Axes.CategoryAxis() {
				Position = OxyPlot.Axes.AxisPosition.Bottom,
				Angle = 50,
				GapWidth = 0.1,
				MajorGridlineStyle = OxyPlot.LineStyle.Automatic
			};
			var nowUnit = ((StatisticsManager.UnitType)unitTypeComboBox.SelectedItem).GetUnit(NodaTime.LocalDate.FromDateTime(DateTime.Now));
			foreach(var u in unitInterval) {
				if(u == nowUnit)
					model.Annotations.Add(new OxyPlot.Annotations.LineAnnotation() {
						Color = OxyPlot.OxyColors.DarkRed,
						Type = OxyPlot.Annotations.LineAnnotationType.Vertical,
						X = axisX.Labels.Count + 0.5,
						StrokeThickness = 2,
						Text = "hranice současnosti",
					});
				axisX.Labels.Add(u.Name);
			}
			model.Axes.Add(axisX);
			var axisY = new OxyPlot.Axes.LinearAxis() {
				Position = OxyPlot.Axes.AxisPosition.Left,
				MajorGridlineStyle = OxyPlot.LineStyle.Automatic,
				MinorGridlineStyle = OxyPlot.LineStyle.Automatic,
			};
			model.Axes.Add(axisY);

			int x = 0;
			decimal runningTotal = 0;
			if(totalCheckBox.IsChecked.Value) {
				var totalSeries = new OxyPlot.Series.LineSeries() {
					Color = OxyPlot.OxyColors.Black,
					StrokeThickness = 2,
					Title = "Celkem",

				};
				foreach(var u in unitInterval) {
					totalSeries.Points.Add(new OxyPlot.DataPoint(x++, (double)(runningTotal += u.Total)));
				}
				model.Series.Add(totalSeries);
			}

			foreach(var _c in categoryListView.SelectedItems) {
				var c = (CategoryManager.Category)_c;
				x = 0;
				runningTotal = 0;
				var series = new OxyPlot.Series.LineSeries() {
					Color = c.Color,
					Title = c.Name,
				};
				foreach(var u in unitInterval) {
					series.Points.Add(new OxyPlot.DataPoint(x++, (double)(runningTotal += u[c])));
				}
				model.Series.Add(series);
			}

			integratedPlotView.Model = model;
		}

		private void RenderTable(StatisticsManager.UnitInterval unitInterval) {
			tableViewScroller.Visibility = Visibility.Visible;
			tableView.Children.Clear();
			tableView.ColumnDefinitions.Clear();
			tableView.RowDefinitions.Clear();

			Border borderElement;

			tableView.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			var nowUnit = ((StatisticsManager.UnitType)unitTypeComboBox.SelectedItem).GetUnit(NodaTime.LocalDate.FromDateTime(DateTime.Now));
			tableView.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
			borderElement = new Border() {
				BorderBrush = Brushes.Black,
				Padding = new Thickness(5),
				BorderThickness = new Thickness(2, 2, 2, 2),
			};
			borderElement.SetValue(Grid.RowProperty, 0);
			borderElement.SetValue(Grid.ColumnProperty, 0);
			tableView.Children.Add(borderElement);
			bool future = false;
			int x = 0;
			foreach(var u in unitInterval) {
				tableView.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
				borderElement = new Border() {
					Child = new TextBlock() {
						HorizontalAlignment = HorizontalAlignment.Center,
						Inlines = {
							new Bold(new Run(u.Name))
						}
					},
					BorderBrush = Brushes.Black,
					BorderThickness = new Thickness(0, 2, 2, 2),
					Background = future ? Brushes.LightCoral : Brushes.Transparent,
					Padding = new Thickness(5),
				};
				if(u == nowUnit)
					future = true;
				borderElement.SetValue(Grid.RowProperty, 0);
				borderElement.SetValue(Grid.ColumnProperty, ++x);
				tableView.Children.Add(borderElement);
			}

			
			int y = 0;
			future = false;
			if(totalCheckBox.IsChecked.Value) {
				y++;
				borderElement = new Border() {
					Child = new TextBlock() {
						HorizontalAlignment = HorizontalAlignment.Center,
						Text = "Celkem",
					},
					BorderBrush = Brushes.Black,
					Padding = new Thickness(5),
					BorderThickness = new Thickness(2, 0, 2, 2),
				};
				borderElement.SetValue(Grid.RowProperty, y);
				borderElement.SetValue(Grid.ColumnProperty, 0);
				tableView.Children.Add(borderElement);
				tableView.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
				x = 0;
				future = false;
				foreach(var u in unitInterval) {
					borderElement = new Border() {
						Child = new TextBlock() {
							Text = u.Total.ToString("0.00"),
							HorizontalAlignment = HorizontalAlignment.Center,
						},
						BorderBrush = Brushes.Black,
						BorderThickness = new Thickness(0, 0, 2, 2),
						Background = future ? Brushes.LightCoral : Brushes.Transparent,
						Padding = new Thickness(5),
					};
					if(u == nowUnit)
						future = true;
					borderElement.SetValue(Grid.RowProperty, y);
					borderElement.SetValue(Grid.ColumnProperty, ++x);
					tableView.Children.Add(borderElement);
				}
			}

			foreach(var _c in categoryListView.SelectedItems) {
				var c = (CategoryManager.Category)_c;
				y++;
				borderElement = new Border() {
					Child = new TextBlock() {
						HorizontalAlignment = HorizontalAlignment.Center,
						Text = c.Name,
					},
					BorderBrush = Brushes.Black,
					Padding = new Thickness(5),
					BorderThickness = new Thickness(2, 0, 2, 2),
				};
				borderElement.SetValue(Grid.RowProperty, y);
				borderElement.SetValue(Grid.ColumnProperty, 0);
				tableView.Children.Add(borderElement);
				tableView.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
				x = 0;
				future = false;
				foreach(var u in unitInterval) {
					borderElement = new Border() { 
						Child = new TextBlock() {
							Inlines = {
								new Bold(new Run(u[c].ToString("0.00")))
							},
							HorizontalAlignment = HorizontalAlignment.Center,
						},
						BorderBrush = Brushes.Black,
						BorderThickness = new Thickness(0, 0, 2, 2),
						Background = future ? Brushes.LightCoral : Brushes.Transparent,
						Padding = new Thickness(5),
					};
					if(u == nowUnit)
						future = true;
					borderElement.SetValue(Grid.RowProperty, y);
					borderElement.SetValue(Grid.ColumnProperty, ++x);
					tableView.Children.Add(borderElement);
				}
			}
		}

		private void unitTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			Render();
		}

		private void fromDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
			Render();
		}

		private void toDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
			Render();
		}

		private void renderTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			Render();
		}

		private void categoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			Render();
		}

		private void totalCheckBox_CheckedChanged(object sender, RoutedEventArgs e) {
			Render();
		}

		private void allCategoriesButton_Click(object sender, RoutedEventArgs e) {
			categoryListView.SelectAll();
		}

		private void noCategoriesButton_Click(object sender, RoutedEventArgs e) {
			categoryListView.SelectedItems.Clear();
		}
	}
}
