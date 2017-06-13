using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;

using System.Diagnostics;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.MySQL
{
	using Data.MySQL;
	using Helpers;

	// 今は使わないかなぁ．

	// for gnuplot
	#region GnuplotChartクラス
	public class GnuplotChart : ConsumptionData, IPlugin
	{
		#region プロパティ

		public string TemplatePath { get; set; }
		public string OutputPath { get; set; }
		public string GnuplotBinaryPath { get; set; }

		#endregion

		#region *定番コンストラクタ(GnuplotChart)
		public GnuplotChart(ConnectionProfile profile)
			: base(profile)
		{ }
		#endregion

		public void GenerateGraph(DateTime current)
		{
			if (!string.IsNullOrEmpty(TemplatePath))
			{
				GeneratePltFile(DefineTrinity(current));
				if (!string.IsNullOrEmpty(GnuplotBinaryPath))
				{
					// 非同期で実行する．
					using (var process = new Process())
					{
						process.StartInfo.FileName = GnuplotBinaryPath;
						process.StartInfo.Arguments = OutputPath;
						process.StartInfo.CreateNoWindow = true;
						process.StartInfo.UseShellExecute = false;	// これを設定しないと，CreateNoWindowは無視される．
						process.Start();
					}
				}
			}

		}

		public void GeneratePltFile(IDictionary<string, DateTime> trinity)
		{
			using (StreamReader reader = new StreamReader(TemplatePath))
			{
				using (StreamWriter writer = new StreamWriter(OutputPath, false, new UTF8Encoding(false)))
				{
					string line;
					while (true)
					{
						line = reader.ReadLine();
						if (line == null)
						{ break; }

						if (line.StartsWith("#{PLOT}"))
						{
							writer.WriteLine(GeneratePlotLine(trinity));
						}
						else
						{
							writer.WriteLine(line);
						}
					}
				}
			}
		}

		protected string GeneratePlotLine(IDictionary<string, DateTime> trinity)
		{
			return "plot " + string.Join(", ", trinity.Select(t => GetSeriesFormat(t.Value, t.Key)));
		}

		protected string GetSeriesFormat(DateTime date, string attribute)
		{
			return string.Format("'{0}' using 1:2 w linespoints {1} title '{2}({3})'",
				DailySourceFilePath(date), GetFormat(attribute), date.ToString("MM月dd日"), attribute);
		}

		// virtualにして実装先でoverrideさせればいいと思う．
		protected string DailySourceFilePath(DateTime date)
		{
			return date.ToString(@"public/\da\ta/Yyyyy_MM/Ddd_01.c\sv");
		}

		protected string GetFormat(string attribute)
		{
			switch (attribute)
			{
				case "本日":
					return "pt 5 lw 3 lc rgbcolor \"#FF0000\"";
				case "最大":
					return "pt 1 lc rgbcolor \"#FF99CC\"";
				default:
					return "pt 1 lc rgbcolor \"#9999FF\"";
			}
		}


		#region (1.3.3)プラグイン化
		public void Configure(System.Xml.Linq.XElement config)
		{
			foreach (var attribute in config.Attributes())
			{
				switch(config.Name.LocalName)
				{
					case "TemplatePath":
						// グラフのファイルの出力先なども，このTemplate内で指定する．
						this.TemplatePath = attribute.Value;
						break;
					case "PltOutputPath":
						this.OutputPath = attribute.Value;
						break;
					case "GnuplotBinaryPath":
						// こんなところで指定するよりも，パスを通してしまった方が早い気がする．
						this.GnuplotBinaryPath = attribute.Value;
						break;
				}
			}

			this.UpdateAction = (time) => { GenerateGraph(time); };

		}
		#endregion

	}
	#endregion

}
