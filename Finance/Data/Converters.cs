using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

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
	public class NodaTimeLocalDateConverter: IValueConverter {
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return ((NodaTime.LocalDate)value).ToDateTimeUnspecified();
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return NodaTime.LocalDate.FromDateTime((DateTime)value);
		}
	}

	public class RepeatPeriodConverter: IValueConverter {
		private static Dictionary<RegularTranactionManager.RepeatPeriod, string> lookup = new Dictionary<RegularTranactionManager.RepeatPeriod, string>();
		private static Dictionary<string, RegularTranactionManager.RepeatPeriod> inverseLookup = new Dictionary<string, RegularTranactionManager.RepeatPeriod>();
		public static ICollection<string> Names { get => lookup.Values; }

		static RepeatPeriodConverter() {
			lookup[RegularTranactionManager.RepeatPeriod.Day] = "Den";
			lookup[RegularTranactionManager.RepeatPeriod.Week] = "Týden";
			lookup[RegularTranactionManager.RepeatPeriod.Month] = "Měsíc";
			lookup[RegularTranactionManager.RepeatPeriod.Year] = "Rok";
			foreach(var kvp in lookup) {
				inverseLookup[kvp.Value] = kvp.Key;
			}
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return lookup[(RegularTranactionManager.RepeatPeriod)value];
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return inverseLookup[(string)value];
		}
	}
}