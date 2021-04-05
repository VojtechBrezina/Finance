using System.Windows;
using System.Windows.Controls;

using Finance.Data;

namespace Finance {
	/// <summary>
	/// Interaction logic for SelectCategoryCombobox.xaml
	/// </summary>
	public partial class CategoryPicker: UserControl {
		private readonly static DependencyProperty SelectedCategoryProperty =
			DependencyProperty.Register("SelectedCategory", typeof(CategoryManager.Category), typeof(CategoryPicker),
				new PropertyMetadata(new PropertyChangedCallback(SelectedCategory_Changed)));
		public CategoryManager.Category SelectedCategory {
			get => (CategoryManager.Category)GetValue(SelectedCategoryProperty);
			set {
				SetValue(SelectedCategoryProperty, value);
			}
		}

		private readonly static DependencyProperty IsUnassignedSelectedProperty =
			DependencyProperty.Register("IsUnassignedSelected", typeof(bool), typeof(CategoryPicker));
		public bool IsUnassignedSelected {
			get => (bool)GetValue(IsUnassignedSelectedProperty);
		}

		public CategoryPicker() {
			InitializeComponent();
			//SetValue(SelectedCategoryProperty, SelectedCategory ?? CategoryManager.Get(-1));
		}

		private static void SelectedCategory_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			/*if(e.NewValue == null) {
				((CategoryPicker)sender).SetValue(SelectedCategoryProperty, CategoryManager.Get(-1));
				return;
			}
			((CategoryPicker)sender).SetValue(IsUnassignedSelectedProperty, e.NewValue == null || ((CategoryManager.Category)e.NewValue).Id == -1);
		*/}
	}
}
