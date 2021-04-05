using System;

namespace Finance.Data {
	static class PathManager {
		public static string Get(string path) {
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VojtechBrezina\\Finance\\" + path + "\\";
		}
		public static string Get() {
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VojtechBrezina\\Finance\\";
		}
	}
}
