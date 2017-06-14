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

}
