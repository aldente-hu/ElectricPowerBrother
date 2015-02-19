using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.TenkiChecker
{
	using Data;

	public class TemperatureCsvGenerator : TemperatureData
	{

		/// <summary>
		/// ヘッダ行を#でコメントアウトするかどうかの値を取得／設定します．
		/// </summary>
		public bool CommentOutHeader { get; set; }

		/// <summary>
		/// ヘッダ行に日付を入れるかどうかの値を取得／設定します．
		/// </summary>
		public bool UseDateOnHeader { get; set; }

		public TemperatureCsvGenerator(string fileName) : base(fileName) { }

		public void OutputTodayCsv(DateTime date, string destination)
		{
			var data = GetOneDayTemperatures(date);

			DateTime from = date - date.TimeOfDay;
			DateTime to = from.AddDays(1);
	
			// 超絶手抜き．
			using (StreamWriter writer = new StreamWriter(destination, false, new UTF8Encoding(false)))
			{
				// ヘッダ部の書き込み
				//writer.WriteLine(string.Format("# {0}", date.ToString("yyyy-MM-dd")));
				writer.WriteLine(string.Format("{0}時刻,気温{1}", CommentOutHeader ? "# " : string.Empty, UseDateOnHeader ? from.ToString("(MM月dd日)") : string.Empty));
				// データ部の書き込み
				foreach (var onedata in data)
				{
					writer.WriteLine(
						string.Join(",", new string[] { (onedata.Key - from).TotalHours.ToString("F3"), onedata.Value.ToString("F1") })
					);
				}
			}

		}


	}


}
