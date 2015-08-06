using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.RetrieveData
{

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
}
