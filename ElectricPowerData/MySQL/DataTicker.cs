using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data.MySQL
{

	#region DataTickerクラス
	public class DataTicker : Data.DataTicker
	{

		#region *Profileプロパティ
		protected ConnectionProfile Profile
		{
			get
			{
				return _profile;
			}
		}
		ConnectionProfile _profile;
		#endregion

		#region *コンストラクタ(DataTicker)
		public DataTicker(ConnectionProfile profile)
		{
			this._profile = profile;
		}
		#endregion

	}
	#endregion

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

}
