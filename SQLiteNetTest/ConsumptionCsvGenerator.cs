using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using ElectricPowerData;

namespace SQLiteNetTest
{

	public class ConsumptionCsvGenerator : ConsumptionData
	{
		public ConsumptionCsvGenerator(string fileName) : base(fileName)
		{ }

		public void OutputTrinityCsv(DateTime time, string destination)
		{
			var trinity = DefineTrinity(time);

			Dictionary<double, int> todayData = new Dictionary<double, int>();
			Dictionary<double, int> maxData = new Dictionary<double, int>();
			Dictionary<double, int> stdData = new Dictionary<double, int>();

			string todayTitle = "本日";
			string maxTitle = "最大";
			string stdTitle = "標準";

			var consumptionData = new Dictionary<double, int>[] { todayData, maxData, stdData };


			foreach (var series in trinity)
			{
				var id = series.Key;
				string date = series.Value.ToString("(MM月dd日)");
				int index;
				switch(id)
				{
					case "本日":
						index = 0;
						todayTitle += date;
						break;
					case "最大":
						index = 1;
						maxTitle += date;
						break;
					case "標準":
						index = 2;
						stdTitle += date;
						break;
					default:
						throw new ApplicationException(string.Format("{0} とはtrinityにふさわしくない系列名ですよ．", id));
				}

				var series_current = series.Value;
				DateTime from = series_current - series_current.TimeOfDay;
				DateTime to = from.AddDays(1);
				Console.WriteLine("{0} - {1}", from, to);

				foreach (var data in GetDetailConsumptions(from, to))
				{
					// 面倒だから時刻はTotalHoursを実数でそのまま出してしまおうか．
					TimeSpan i_time = data.Key - from;
					consumptionData[index].Add(i_time.TotalHours, data.Value);
				}
			}
			
			// 超絶手抜き．
			using (StreamWriter writer = new StreamWriter(destination, false, new UTF8Encoding(false)))
			{
				// ヘッダ部の書き込み
				writer.WriteLine(string.Format("# 時刻,{0},{1},{2}", todayTitle, maxTitle, stdTitle));
				// データ部の書き込み
				for (int i = 1; i <= 6 * 24; i++)
				{
					double h = TimeSpan.FromMinutes(i * 10).TotalHours;
					//double h = i / 6.0;
					writer.WriteLine(string.Join(",",
						new string[] {
							h.ToString("F3"),
							todayData.ContainsKey(h) ? todayData[h].ToString() : string.Empty,
							maxData.ContainsKey(h) ? maxData[h].ToString() : string.Empty,
							stdData.ContainsKey(h) ? stdData[h].ToString() : string.Empty
						}
					));
				}
			}
		}


	}


}
