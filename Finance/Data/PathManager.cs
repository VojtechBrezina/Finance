using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Data {
	static class PathManager {
		public static string Get(string path) {
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VB\\Finance\\" + path + "\\";
		}
		public static string Get() {
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VB\\Finance\\";
		}
	}
}
