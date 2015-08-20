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
	using Data;
	using Helpers;

	namespace Legacy
	{
		// (1.3.15)
		public class IndexPage : ConsumptionData, IPlugin
		{
			// (1.3.15)
			public IndexPage(string databaseFile)
				: base(databaseFile)
			{
				alerts = new AlertData(databaseFile);
			}
			readonly AlertData alerts;

			// (1.3.15)
			public string Template { get; set; }

			// (1.3.15)
			public string Destination { get; set; }

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


			// (1.3.15)
			#region *出力する(Output)
			public void Output(StreamWriter writer)
			{
				var current_month = this.GetLatestDataTime();
				if (current_month.Day == 1 && current_month.Hour < 1)
				{
					current_month = current_month.AddMonths(-1);
				}

				var pattern = new Regex(@"#\{([a-z][0-9a-z]*)\}");
				using (var reader = new StreamReader(File.Open(this.Template, FileMode.Open, FileAccess.Read), this.CharacterEncoding))
				{
					while (!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						pattern.Replace(line, m => { return Replace(m.Value, current_month); });
					}
				}
			}

			// (1.3.15)
			private string Replace(string key, DateTime month)
			{
				switch(key)
				{
					case "transition1":
						return DisplayTransition(1);
					case "transition2":
						return DisplayTransition(2);
					case "alert_transition":
						return AlertTransition();
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
			string DisplayTransition(int ch, int count = 4)
			{
				var sorted = this.GetRecentData(count, ch).OrderByDescending(d => d.Key);
				return string.Format("[{1}]{0}[{2}]",
						string.Join("←", sorted.Select(d => d.Value)),
						sorted.First().Key.ToString("HH:mm"),
						sorted.Last().Key.AddMinutes(-10).ToString("HH:mm"));
			}

			// (1.3.15)
			string AlertTransition()
			{
				var rank = alerts.GetCurrentRank();
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


			public void Generate()
			{
				using (StreamWriter writer = new StreamWriter(File.Open(this.Destination, FileMode.Create, FileAccess.Write), CharacterEncoding))
				{
					this.Output(writer);
				}
			}

			public void Configure(System.Xml.Linq.XElement config)
			{
				throw new NotImplementedException();
			}
		}
	}
}
