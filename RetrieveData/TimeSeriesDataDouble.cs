using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.RetrieveData
{

	public class TimeSeriesDataDouble : TimeSeriesData<double>
	{

		public static TimeSeriesDataDouble operator +(TimeSeriesDataDouble that, TimeSeriesDataDouble other)
		{
			if (that.Time != other.Time)
			{
				throw new ArgumentException("Timeの異なるデータは足し算できません．");
			}

			var sum = new TimeSeriesDataDouble { Time = that.Time };
			var data = new Dictionary<int, double>();
			foreach (var ch in that.Data)
			{
				data[ch.Key] = ch.Value;
			}
			foreach (var ch in other.Data)
			{
				if (data.Keys.Contains(ch.Key))
				{
					data[ch.Key] += ch.Value;
				}
				else
				{
					data[ch.Key] = ch.Value;
				}
			}
			sum.Data = data;
			return sum;
		}

	}
}
