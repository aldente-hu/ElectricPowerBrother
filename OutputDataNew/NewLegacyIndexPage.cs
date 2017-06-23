using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Data.New;
	using Helpers.New;

	namespace Legacy
	{

		namespace New
		{
			// (1.3.15)
			#region IndexPageクラス
			public class IndexPage : ConsumptionData, IPlugin, IIndexpage
			{

				// (1.3.15)
				#region *コンストラクタ(IndexPage)
				public IndexPage(Data.IConnectionProfile profile)
					: base(profile)
				{
					alerts = new AlertData(profile);
				}
				readonly AlertData alerts;
				#endregion


				#region プロパティ

				// (1.3.15)
				#region *Templateプロパティ
				/// <summary>
				/// テンプレートが記述されたファイルへのパスを取得／設定します．
				/// </summary>
				public string Template { get; set; }
				#endregion

				// (1.3.15)
				#region *Destinationプロパティ
				/// <summary>
				/// 生成されたhtmlドキュメントの出力先を取得／設定します．
				/// </summary>
				public string Destination { get; set; }
				#endregion

				// (1.3.15)
				#region *CharacterEncodingプロパティ
				/// <summary>
				/// CSVファイルの文字エンコーディングを取得／設定します．デフォルトはEncoding.UTF8です．
				/// </summary>
				public Encoding CharacterEncoding
				{
					get { return this._encoding; }
					set
					{
						this._encoding = value;
					}
				}
				Encoding _encoding = Encoding.UTF8;
				#endregion

				#endregion


				// (1.3.15)
				#region *出力する(Output)
				/// <summary>
				/// Templateをもとにhtmlドキュメントを生成し，指定されたStreamWriterに出力します．
				/// </summary>
				/// <param name="writer"></param>
				public async Task OutputAsync(StreamWriter writer)
				{
					var current_month = await this.GetLatestDataTimeAsync();
					if (current_month.Day == 1 && current_month.Hour < 1)
					{
						current_month = current_month.AddMonths(-1);
					}

					var pattern = new Regex(@"#\{([a-z][0-9a-z_]*)\}");
					using (var reader = new StreamReader(File.Open(this.Template, FileMode.Open, FileAccess.Read), this.CharacterEncoding))
					{
						while (!reader.EndOfStream)
						{
							var line = await reader.ReadLineAsync();
							await writer.WriteLineAsync(pattern.Replace(line, m => { return ReplaceAsync(m.Groups[1].Value, current_month).Result; }));
						}
					}
				}

				// (1.3.15)
				private async Task<string> ReplaceAsync(string key, DateTime month)
				{
					switch (key)
					{
						case "transition1":
							return await DisplayTransitionAsync(1);
						case "transition2":
							return await DisplayTransitionAsync(2);
						case "alert_transition":
							return await AlertTransitionAsync();
						case "month":
							return OutputMonth(month);
						case "chart_riko":
							return ChartDestination(month, "riko");
						case "chart_riko1":
							return ChartDestination(month, "riko1");
						case "chart_riko2":
							return ChartDestination(month, "riko2");
						default:
							throw new ArgumentException("不適切なkeyです．");
					}
				}

				// (1.3.15)
				async Task<string> DisplayTransitionAsync(int ch, int count = 4)
				{
					var sorted = (await this.GetRecentDataAsync(count, ch)).OrderByDescending(d => d.Key);
					return string.Format("[{1}]{0}[{2}]",
							string.Join("←", sorted.Select(d => d.Value)),
							sorted.First().Key.ToString("HH:mm"),
							sorted.Last().Key.AddMinutes(-10).ToString("HH:mm"));
				}

				// (1.3.15)
				async Task<string> AlertTransitionAsync()
				{
					var rank = await alerts.GetCurrentRankAsync();
					if (rank >= 10)
					{
						return "warn_transition";
					}
					else if (rank >= 1)
					{
						return "alert_transition";
					}
					else
					{
						return "transition";
					}
				}

				// (1.3.15)
				string ChartDestination(DateTime month, string name)
				{
					return MonthlyChartDestinationGenerator.Generate(month, name);
				}

				// (1.3.15)
				string OutputMonth(DateTime month)
				{
					string[] m_names = new string[] {"January", "February", "March", "April", "May", "June",
						"July", "August", "September", "October", "November", "December"};
					return string.Format("{0} {1}", m_names[month.Month - 1], month.Year);
				}
				#endregion


				// (1.3.15)
				#region *更新する(Update)
				/// <summary>
				/// Templateをもとに新しいhtmlドキュメントを作成し，Destinationに出力します．
				/// </summary>
				public async Task UpdateAsync()
				{
					using (StreamWriter writer = new StreamWriter(File.Open(this.Destination, FileMode.Create, FileAccess.Write), CharacterEncoding))
					{
						await this.OutputAsync(writer);
					}
				}
				#endregion


				// <Config template="B:\index_template.html" destination="B:\index.html" encoding="Shift_JIS" />

				// (1.3.16)
				public void Configure(System.Xml.Linq.XElement config)
				{
					this.Template = (string)config.Attribute("template");
					this.Destination = (string)config.Attribute("destination");
					var encodingAttribute = config.Attribute("encoding");
					if (encodingAttribute != null)
					{
						this.CharacterEncoding = Encoding.GetEncoding(encodingAttribute.Value);
					}

					this.UpdateAction = async (time) => { await UpdateAsync(); };
				}
			}
			#endregion

			public interface IIndexpage
			{

				#region プロパティ

				/// <summary>
				/// テンプレートが記述されたファイルへのパスを取得／設定します．
				/// </summary>
				string Template { get; set; }

				/// <summary>
				/// 生成されたhtmlドキュメントの出力先を取得／設定します．
				/// </summary>
				string Destination { get; set; }

				/// <summary>
				/// CSVファイルの文字エンコーディングを取得／設定します．デフォルトはEncoding.UTF8です．
				/// </summary>
				Encoding CharacterEncoding { get; set; }

				#endregion

				Task UpdateAsync();
			}

		}
	}
}
