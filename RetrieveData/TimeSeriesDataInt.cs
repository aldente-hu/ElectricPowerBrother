using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.RetrieveData
{
	public class TimeSeriesDataInt : TimeSeriesData<int>
	{

		public static TimeSeriesDataInt operator +(TimeSeriesDataInt that, TimeSeriesDataInt other)
		{
			if (that.Time != other.Time)
			{
				throw new ArgumentException("Timeの異なるデータは足し算できません．");
			}

			var sum = new TimeSeriesDataInt { Time = that.Time };
			var data = new Dictionary<int, int>();
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