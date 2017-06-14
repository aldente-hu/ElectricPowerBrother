using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data.MySQL
{

	#region ConnectionProfile構造体
	public struct ConnectionProfile
	{
		/// <summary>
		/// 接続先のMySQLサーバ名です。
		/// </summary>
		public string Server;

		/// <summary>
		/// MySQLサーバのユーザ名です。
		/// </summary>
		public string UserName;

		/// <summary>
		/// MySQLサーバのパスワードです。
		/// </summary>
		public string Password;

		/// <summary>
		/// MySQLサーバのデータベース名です。
		/// </summary>
		public string Database;

		/// <summary>
		/// 接続文字列を取得します。
		/// </summary>
		public string ConnectionString
		{
			get
			{
				return $"server={Server};user={UserName};password={Password};database={Database}";
			}
		}
	}
	#endregion


	namespace New
	{
		using System.Data.Common;
		using MySql.Data.MySqlClient;

		#region ConnectionProfileクラス
		public class ConnectionProfile : IConnectionProfile
		{
			/// <summary>
			/// 接続先のMySQLサーバ名です。
			/// </summary>
			public string Server;

			/// <summary>
			/// MySQLサーバのユーザ名です。
			/// </summary>
			public string UserName;

			/// <summary>
			/// MySQLサーバのパスワードです。
			/// </summary>
			public string Password;

			/// <summary>
			/// MySQLサーバのデータベース名です。
			/// </summary>
			public string Database;

			public ConnectionProfile()
			{ }

			public ConnectionProfile(IDictionary<string, string> parameter)
			{
				this.Server = parameter["Server"];
				this.UserName = parameter["UserName"];
				this.Password = parameter["Password"];
				this.Database = parameter["Database"];
			}

			/// <summary>
			/// 接続文字列を取得します。
			/// </summary>
			public string ConnectionString
			{
				get
				{
					return $"server={Server};user={UserName};password={Password};database={Database}";
				}
			}

			public async Task<DbConnection> GetConnectionAsync()
			{
				var conn = new MySqlConnection(ConnectionString);
				await conn.OpenAsync();
				return conn;
			}

			public DbParameter CreateParameter(string name, object value)
			{
				return new MySqlParameter(name, value);
			}

			public DbCommand CreateCommand()
			{
				return new MySqlCommand();
			}
		}
		#endregion

	}
}
