using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Finance.Data.Import {
	abstract class CSVImporter : FileImporter{
		protected List<List<string>> lines = new List<List<string>>();
		protected readonly string fieldDelimiters, fieldWrappers;

		public CSVImporter(string d, string w) {
			fieldDelimiters = d;
			fieldWrappers = w;
		}

		public override void Import(string path, Encoding encoding) {
			base.Import(path, encoding);
			while(!fileReader.EndOfStream) {
				lines.Add(new List<string>());
				string line = fileReader.ReadLine();
				string field = string.Empty;
				char? wrapped = null;
				for(int i = 0; i < line.Length; i++) {
					if(i == 0 && wrapped == null) {
						for(int j = 0; j < fieldWrappers.Length; j++) {
							if(line[i] == fieldWrappers[j]) {
								wrapped = fieldWrappers[j];
								break;
							}
						}
						if(wrapped != null)
							continue;
					}
					
					if (wrapped != null) {
						for(int j = 0; j < fieldWrappers.Length; j++) {
							if(line[i] == fieldWrappers[j]) {
								wrapped = null;
								break;
							}
						}
						if(wrapped == null)
							continue;
					}

					bool delimiter = false;
					for(int j = 0; j < fieldDelimiters.Length; j++) {
						if(line[i] == fieldDelimiters[j]) {
							lines[^1].Add(field);
							delimiter = true;
							field = string.Empty;
							break;
						}
					}
					if(!delimiter) {
						field += line[i];
					}
				}
				lines[^1].Add(field);
				field = string.Empty;
			}

			#region debug
			/*foreach(var l in lines) {
				foreach(var f in l) {
					Debug.WriteLine(f);
				}
				Debug.WriteLine("");
			}*/
			#endregion

		}

		public override void CleanUp() {
			base.CleanUp();
			lines.Clear();
		}
	}
}
