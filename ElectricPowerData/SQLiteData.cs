using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data
{
	public class SQLiteData : DataTicker
	{
		// とりあえず1つのファイルに1つのインスタンスを対応させてみる．

		#region *FileNameプロパティ
		/// <summary>
		/// データベースファイルのファイル名を取得します．
		/// 設定はコンストラクタの引数で行って下さい．
		/// </summary>
		public string FileName
		{
			get { return _fileName; }
		}
		readonly string _fileName;
		#endregion

		#region *ConnectionStringプロパティ
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
		#endregion

		#region *コンストラクタ(SQLiteData)
		public SQLiteData(string fileName)
		{
			this._fileName = fileName;
		}
		#endregion

	}
}
