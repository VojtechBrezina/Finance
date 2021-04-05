using System.Text;
using System.IO;

namespace Finance.Data.Import {
	abstract class FileImporter {
		protected StreamReader fileReader = null;
		public Data.RawDataBatch Batch { get; private set; } = null;
		public string DisplayName { get; protected set; }
		public string FilterString { get; protected set; }

		public virtual void Import(string path, Encoding encoding) {
			fileReader = new StreamReader(path, encoding);
			Batch = RawDataBatch.Create();
		}

		public virtual void CleanUp() {
			fileReader.Close();
			fileReader.Dispose();
			fileReader = null;
			Batch = null;
		}
	}
}
 