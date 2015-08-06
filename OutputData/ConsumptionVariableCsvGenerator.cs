using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Data;
	using Helpers;

	// (1.3.1)
	// ↓プラグイン化して無理矢理解決．
	// (1.2.1)
	// グラフが1つだけならいいけど，複数のグラフを出すにはどうする？
	// 呼び出し元の問題では...．いま1つのTickerに関連づけているものをTaskとかスレッドとかプロセスに分けてしまえばいいのでは？
	public class ConsumptionVariableCsvGenerator : ConsumptionData, IPlugin
	{
		#region *コンストラクタ(ConsumptionVariableCsvGenerator)
		public ConsumptionVariableCsvGenerator(string fileName)
			: base(fileName)
		{
			this.Riko2CorrectionFactor = 1.0;
			this.SpanHour = 4.0;
		}
		#endregion


		/// <summary>
		/// 正時で区切るか否かの値を取得／設定します．
		/// </summary>
		public bool SplitByHour { get; set; }

		/// <summary>
		/// 表示する時間範囲をhour単位で取得／設定します．既定値は4.0です．
		/// </summary>
		public double SpanHour { get; set; }

		/// <summary>
		/// ファイルの出力先を取得／設定します．
		/// </summary>
		public string Destination { get; set; }

		/// <summary>
		/// 既定値は1.0です．
		/// </summary>
		public double Riko2CorrectionFactor { get; set; }

		public Func<IDictionary<int, int>, double> RikoCorrection
		{
			get
			{
				return dic => dic[1] + dic[2] * Riko2CorrectionFactor;
			}
		}

		DateTime DecideEndTime(DateTime latestTime)
		{
			if (this.SplitByHour && latestTime.Minute != 0)
			{
				return latestTime.AddMinutes(60 - latestTime.Minute);
			}
			else
			{
				return latestTime;
			}
		}

		// (1.2.1)latestTimeの秒以下は0でなければならない！
		IDictionary<DateTime, double> GetDataForCsv(DateTime latestTime)
		{
			DateTime to = this.DecideEndTime(latestTime);
			DateTime from = to.AddHours(-this.SpanHour);

			var data = GetDetailConsumptions(from, latestTime);
			// ↑toまでとると半端なデータ(ch1がとれているけどch2がとれていない時とか)が入るかもしれないので，とりあえずlatestTimeまでにしておく．

			// とれていない時刻のデータを0とする．
			for (DateTime time = to; time > from; time = time.AddMinutes(-10))
			{
				if (!data.Keys.Contains(time))
				{
					data.Add(time, 0);
				}
			}
			return (from row in data select new KeyValuePair<DateTime, double>(row.Key, row.Value)).ToDictionary(p => p.Key, p => p.Value);
		}

		IDictionary<DateTime, double> GetCorrectedDataForCsv(DateTime latestTime, Func<IDictionary<int, int>, double> correction)
		{
			DateTime to = this.DecideEndTime(latestTime);
			DateTime from_time = to.AddHours(-this.SpanHour);

			var data =  (from row in GetParticularConsumptions(from_time, to)
							select new KeyValuePair<DateTime, double>(row.Key, correction.Invoke(row.Value))).ToDictionary(p => p.Key, p => p.Value);

			// とれていない時刻のデータを0とする．
			for (DateTime time = to; time > from_time; time = time.AddMinutes(-10))
			{
				if (!data.Keys.Contains(time))
				{
					data.Add(time, 0);
				}
			}
			return data;
		}

		// (1.3.2)返り値をDateTimeからvoidに変更．
		public void OutputCsv(DateTime previousDataTime)
		{
			//var new_data_time = this.GetLatestDataTime();
			//if (new_data_time >= previousDataTime)	// (1.3.1.2)これまで不等号が逆だった？
			//{
			//	return new_data_time;
			//}

			IDictionary<DateTime, double> data;
			if (this.Riko2CorrectionFactor == 1)
			{
				// IDictionary<DateTime, int>をIDictionary<DateTime, double>にキャストすることはできなかった．
				data = GetDataForCsv(previousDataTime);
			}
			else
			{
				data = GetCorrectedDataForCsv(previousDataTime, this.RikoCorrection);
			}
			using (StreamWriter writer = new StreamWriter(this.Destination, false, Encoding.GetEncoding("csWindows31J")))
			{
				// 超絶手抜きな決め打ち実装．
				writer.WriteLine("{0} UPDATE", DateTime.Now.ToString());
				writer.WriteLine("デマンド注意報基準値(kW)");
				writer.WriteLine("600");
				writer.WriteLine();
				writer.WriteLine("DATE,TIME,理工学部(kW)");
				foreach (var row in data.OrderBy(r => r.Key))
				{
					writer.WriteLine("{0},{1},{2}", row.Key.ToString("yyyy/MM/dd"), row.Key.ToString("HH:mm"), (row.Value * 6).ToString("##0"));
				}
			}

			//return previousDataTime;
		}

		#region (1.3.1)プラグイン化

		public void Configure(System.Xml.Linq.XElement config)
		{
			foreach (var attribute in config.Attributes())
			{
				switch(attribute.Name.LocalName)
				{
					case "Destination":
						this.Destination = attribute.Value;
						break;
					case "SplitByHour":
						this.SplitByHour = (bool)attribute;
						break;
					case "SpanHour":
						this.SpanHour = (double)attribute;
						break;
					case "Riko2CorrectionFactor":
						this.Riko2CorrectionFactor = (double)attribute;
						break;
				}
			}

			this.UpdateAction = (time) => { OutputCsv(time); };
		}

		#endregion

	}

}


