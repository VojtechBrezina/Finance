using System;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;

namespace Finance.Data {
	public class IdToCategoryConverter: IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return CategoryManager.Get((int)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return ((CategoryManager.Category)value).Id;
		}
	}

	public class MyOxyColorConverter: IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var r = ((OxyPlot.OxyColor)value).R;
			var g = ((OxyPlot.OxyColor)value).G;
			var b = ((OxyPlot.OxyColor)value).B;
			return System.Windows.Media.Color.FromRgb(r, g, b);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			var r = ((System.Windows.Media.Color)value).R;
			var g = ((System.Windows.Media.Color)value).G;
			var b = ((System.Windows.Media.Color)value).B;
			return OxyPlot.OxyColor.FromRgb(r, g, b);
		}
	}
	public class InverseBooleanConverter: IValueConverter {

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return !(bool)value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return !(bool)value;
		}
	}
}