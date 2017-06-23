using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Data.New;
	using Helpers;

	namespace New
	{

		public class ConsumptionCsvGenerator : ConsumptionData, Helpers.New.IPlugin
		{
			public Data.IConnectionProfile ConnectionProfile
			{
				get
				{
					return this.profile;
				}
				set
				{
					this.profile = value;
				}
			}

			public ConsumptionCsvGenerator() : this(null)
			{

			}

			public ConsumptionCsvGenerator(Data.IConnectionProfile profile) : base(profile)
			{ }

			/// <summary>
			/// ヘッダ行を#でコメントアウトするかどうかの値を取得／設定します．
			/// </summary>
			public bool CommentOutHeader { get; set; }


			public async Task OutputTrinityCsvAsync(DateTime time, string destination)
			{
				var trinity = await DefineTrinityAsync(time);

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
					switch (id)
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
					//Console.WriteLine("{0} - {1}", from, to);

					foreach (var data in await GetDetailConsumptionsAsync(from, to))
					{
						// 面倒だから時刻はTotalHoursを実数でそのまま出してしまおうか．
						TimeSpan i_time = data.Key - from;
						consumptionData[index].Add(i_time.TotalHours, data.Value);
					}
				}

				// 超絶手抜き．
				using (var writer = new StreamWriter(destination, false, new UTF8Encoding(false)))
				{
					// ヘッダ部の書き込み
					await writer.WriteLineAsync(string.Format("{3}時刻,{0},{1},{2}", todayTitle, maxTitle, stdTitle, CommentOutHeader ? "# " : string.Empty));
					// データ部の書き込み
					for (int i = 1; i <= 6 * 24; i++)
					{
						double h = TimeSpan.FromMinutes(i * 10).TotalHours;
						//double h = i / 6.0;
						await writer.WriteLineAsync(string.Join(",",
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

			#region (1.3.1)プラグイン実装

			// Destinationは必須．
			// CommentOutHeaderは任意．

			public void Configure(System.Xml.Linq.XElement config)
			{
				foreach (var attribute in config.Attributes())
				{
					switch (attribute.Name.LocalName)
					{
						case "CommentOutHeader":
							this.CommentOutHeader = (bool)attribute;
							break;
					}
				}
				this.UpdateAction = async (date) =>
				{
					var path = config.Attribute("Destination").Value;
					var destination = Path.IsPathRooted(path) ? path : Path.Combine(config.Document.Root.Attribute("DataRootPath").Value, path);
					await OutputTrinityCsvAsync(date, destination);
				};
			}

			#endregion
		}

	}

}
