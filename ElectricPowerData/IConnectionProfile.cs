using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

using System.Data.SQLite;
using MySql.Data.MySqlClient;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data
{

	public interface IConnectionProfile
	{
		string ConnectionString { get; }
		/// <summary>
		/// OpenされたDbConnectionを返します。
		/// </summary>
		/// <returns></returns>
		Task<DbConnection> GetConnectionAsync();
		DbParameter CreateParameter(string name, object value);
		//DbCommand CreateCommand();
	}

	public static class ConnectionProfileFactory
	{
		public static IConnectionProfile Create(string dbType, IDictionary<string, string> parameter)
		{
			switch (dbType.ToLower())
			{
				case "mysql":
					return new MySQL.New.ConnectionProfile(parameter);
				case "sqlite":
					return new SQLite.New.ConnectionProfile(parameter);
				default:
					// 例外を返した方がよい？
					return null;
			}
		}
	}

}