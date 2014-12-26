using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricPowerData
{
	public class SQLiteData
	{
			// とりあえず1つのファイルに1つのインスタンスを対応させてみる．

		public string FileName
		{
			get { return _fileName; }
		}
		readonly string _fileName;

		/// <summary>
		/// データベースへの接続文字列を取得します．
		/// </summary>
		protected string ConnectionString
		{
			get
			{
				return string.Format("Data source={0}", FileName);
			}
		}

		public SQLiteData(string fileName)
		{
			this._fileName = fileName;
		}

	}
}
