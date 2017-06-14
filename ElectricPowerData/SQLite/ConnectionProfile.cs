using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data.SQLite
{
	namespace New
	{
		using System.Data.Common;
		using System.Data.SQLite;

		#region SqliteConnectionProfileクラス
		public class ConnectionProfile : IConnectionProfile
		{

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
			public string ConnectionString
			{
				get
				{
					return string.Format("Data source={0}", FileName);
				}
			}
			#endregion

			#region *コンストラクタ(SQLiteData)
			public ConnectionProfile(string fileName)
			{
				this._fileName = fileName;
			}

			public ConnectionProfile(IDictionary<string, string> parameter)
				: this(parameter["FileName"])
			{
			}

			#endregion

			public async Task<DbConnection> GetConnectionAsync()
			{
				var conn = new SQLiteConnection(ConnectionString);
				await conn.OpenAsync();
				return conn;
			}

			public DbParameter CreateParameter(string name, object value)
			{
				return new SQLiteParameter(name, value);
			}

			public DbCommand CreateCommand()
			{
				return new SQLiteCommand();
			}
		}
		#endregion
	}

}