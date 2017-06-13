using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.TenkiChecker
{
	interface ITemperatureData
	{
		/// <summary>
		/// 気温データを追加します。
		/// </summary>
		/// <param name="time">時刻</param>
		/// <param name="temperature">気温(摂氏単位)</param>
		void InsertTemperature(DateTime time, decimal temperature);
	}
}
