using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using Finance.Data;

using Finance.Data.Import;
using System.IO;

namespace Finance.Screens {
	/// <summary>
	/// Interaction logic for DataInputScreen.xaml
	/// </summary>
	public partial class DataInputScreen: UserControl {
		private OpenFileDialog OpenFileDialog { get => (OpenFileDialog)Resources["OpenFileDialog"]; }

		private readonly List<Data.Import.FileImporter> importers;

		private FileImporter SelectedImporter {
			get => (Data.Import.FileImporter) fileFormatComboBox.SelectedItem; 
			set => fileFormatComboBox.SelectedItem = value;
		}

		private RawDataBatch batch = null;

		public DataInputScreen() {
			InitializeComponent();
			
			importers = new List<Data.Import.FileImporter>{
				new Data.Import.MBankCSVImporter(),
			};

			fileFormatComboBox.ItemsSource = importers;

			OpenFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var f = "";
			foreach(var i in importers) {
				f += i.FilterString + "|";
			}
			f += "Všechny soubory|*.*";
			OpenFileDialog.Filter = f;
			OpenFileDialog.FilterIndex = importers.Count + 1;
		}

		private void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
			filePathTextBox.Text = OpenFileDialog.FileName;

			if(OpenFileDialog.FilterIndex <= importers.Count) {
				fileFormatComboBox.SelectedIndex = OpenFileDialog.FilterIndex - 1;
			}
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void fileSelectionButton_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog.ShowDialog();
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification="Event handler")]
		private void fileInputConfirmButton_Click(object sender, RoutedEventArgs e) {
			if(!File.Exists(filePathTextBox.Text)) {
				System.Windows.MessageBox.Show($"Zvolený soubor neexistuje, nebo nebyl vyplněn.", "Zadání transakce", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			if(fileFormatComboBox.SelectedItem == null) {
				System.Windows.MessageBox.Show($"Nebyl zvolen formát souboru.", "Zadání transakce", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			try {
				SelectedImporter.Import(filePathTextBox.Text, Encoding.GetEncoding(1250));
				batch = SelectedImporter.Batch;
				SelectedImporter.CleanUp();

				filePathTextBox.Text = string.Empty;
				fileFormatComboBox.SelectedIndex = -1;

				fileInputConfirmButton.IsEnabled = false;

				fileImportTransactionListView.ItemsSource = batch.transactions;

				fileInputNextBatchButton.IsEnabled = true;
			} catch(Exception ex) {
				System.Windows.MessageBox.Show($"Při importu došlo k chybě.\n{ex.Message}", "Import ze souboru", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void fileFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void addCategoryButton_Click(object sender, RoutedEventArgs e) {
			if(addCategoryNameTextBox.Text.Trim().Length == 0 || !addCategoryColorPicker.SelectedColor.HasValue) {
				System.Windows.MessageBox.Show("Vlastnosti kategorie nejsou správně vyplněné.", "Vytvořit kategorii", MessageBoxButton.OK, MessageBoxImage.Error);
			} else {
				var r = addCategoryColorPicker.SelectedColor.Value.R;
				var g = addCategoryColorPicker.SelectedColor.Value.G;
				var b = addCategoryColorPicker.SelectedColor.Value.B;
				CategoryManager.Add(addCategoryNameTextBox.Text, OxyPlot.OxyColor.FromRgb(r, g, b));
				addCategoryNameTextBox.Text = string.Empty;
				addCategoryColorPicker.SelectedColor = null;
			}
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void removeCategoryButton_Click(object sender, RoutedEventArgs e) {
			if(editCategoryPicker.SelectedCategory != null)
				CategoryManager.Remove(editCategoryPicker.SelectedCategory);
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void fileInputNextBatchButton_Click(object sender, RoutedEventArgs e) {
			batch.Save();
			StatisticsManager.Add(batch);
			fileInputNextBatchButton.IsEnabled = false;
			fileInputConfirmButton.IsEnabled = true;
			fileImportTransactionListView.ItemsSource = null;
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void transactionInputConfirmButton_Click(object sender, RoutedEventArgs e) {
			if(transactionInputDatePicker.SelectedDate == null) {
				System.Windows.MessageBox.Show("Nebylo zadáno datum transakce.", "Zadání transakce", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			decimal amount;
			if(!decimal.TryParse(transactionInputAmountTextBox.Text, out amount)) {
				System.Windows.MessageBox.Show("Částka buď nebyly vyplněna, nebo nemá číselnou hodnotu.", "Zadání transakce", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			var manualBatch = RawDataBatch.Create();
			manualBatch.Add(new RawDataBatch.Transaction {
				Date = NodaTime.LocalDate.FromDateTime(transactionInputDatePicker.SelectedDate.Value),
				Amount = amount,
				Description = transactionInputDescriptionTextBox.Text,
				CategoryId = transactionInputCategoryPicker.SelectedCategory.Id,
			});
			manualBatch.Save();
			StatisticsManager.Add(manualBatch);
			transactionInputDatePicker.SelectedDate = null;
			transactionInputAmountTextBox.Text = string.Empty;
			transactionInputDescriptionTextBox.Text = string.Empty;
			transactionInputCategoryPicker.SelectedCategory = null;
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void addAutomationRuleButton_Click(object sender, RoutedEventArgs e) {
			editCategoryPicker.SelectedCategory.Rules.Add(new CategoryManager.AutomationRule());
		}

		[SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Event handler")]
		private void removeAutomationRuleButton_Click(object sender, RoutedEventArgs e) {
			if(automationuleListView.SelectedItem != null)
				editCategoryPicker.SelectedCategory.Rules.Remove((CategoryManager.AutomationRule)automationuleListView.SelectedItem);
		}
	}
}
