using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.TenkiChecker
{
	using Data;

	public class TemperatureCsvGenerator : TemperatureData, Helpers.IPlugin
	{

		#region プロパティ

		/// <summary>
		/// ヘッダ行を#でコメントアウトするかどうかの値を取得／設定します．
		/// </summary>
		public bool CommentOutHeader { get; set; }

		/// <summary>
		/// ヘッダ行に日付を入れるかどうかの値を取得／設定します．
		/// </summary>
		public bool UseDateOnHeader { get; set; }

		#endregion

		#region *コンストラクタ(TemperatureCsvGenerator)
		public TemperatureCsvGenerator(string fileName) : base(fileName) { }
		#endregion

		#region *本日分のCSVを出力(OutputTodayCsv)
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
		#endregion

		#region  (1.1.1)以下プラグイン用．

		public void Configure(System.Xml.Linq.XElement config)
		{
			// config.Name.LocalNameをチェックしますか？

			var comment_out_header = (bool?)config.Attribute("CommentOutHeader");
			if (comment_out_header.HasValue)
			{
				this.CommentOutHeader = comment_out_header.Value;
			}

			var use_date_header = (bool?)config.Attribute("UseDateHeader");
			if (use_date_header.HasValue)
			{
				this.UseDateOnHeader = use_date_header.Value;
			}

			this.UpdateAction = (current) =>
			{ this.OutputTodayCsv(current, (string)config.Attribute("Destination")); };

			// あれ，Invokeはいらない？
			// むしろUpdateをプラグイン化する必要がある．

		}

		// ※未使用？
		// プラグイン用に共通のメソッドを与える．
		public void Invoke(DateTime date, params string[] options)
		{
			// 最初のパラメータが出力先を与える．
			this.OutputTodayCsv(date, options[0]);
		}

		#endregion

	}


}
