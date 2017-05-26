using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Base
{
	// (0.0.1)んー，不要になるかも．
	#region IPulseLoggerインターフェイス
	public interface IPulseLogger
	{
		/// <summary>
		/// 通常のソースからデータを取得するときに使用します．
		/// </summary>
		/// <param name="time"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		IEnumerable<TimeSeriesDataDouble> GetActualDataAfter(DateTime time, int max = -1);
	}
	#endregion

}
