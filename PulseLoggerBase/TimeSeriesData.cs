using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Base
{
	// (0.0.1)
	#region TimeSeriesDataクラス
	public class TimeSeriesData<T> where T : struct
	{
		public DateTime Time { get; set; }
		public IDictionary<int, T> Data { get; set; }

		public TimeSeriesData()
		{
			// とりあえず．
			this.Data = new Dictionary<int, T>();
		}
	}
	#endregion

}
