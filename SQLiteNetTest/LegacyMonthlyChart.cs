using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Helpers;

	namespace Legacy
	{
		// (1.3.6)
		#region MonthlyChartクラス
		public class MonthlyChart : PltFileGeneratorBase, IUpdatingPlugin
		{

			#region プロパティ

			/// <summary>
			/// グラフの横幅を取得／設定します．
			/// </summary>
			public int Width { get; set; }

			/// <summary>
			/// グラフの縦幅を取得／設定します．
			/// </summary>
			public int Height { get; set; }


			/// <summary>
			/// 描画対象のデータを取得／設定します．
			/// 設定した月の初日から設定した日までが描画対象になります．
			/// </summary>
			public DateTime CurrentDate { get; set; }

			/// <summary>
			/// 縦軸の最大値を取得／設定します．
			/// </summary>
			public int Maximum { get; set; }

			/// <summary>
			/// 縦軸の最小値を取得／設定します．
			/// </summary>
			public int Minimum { get; set; }

			/// <summary>
			/// 表示用の系列名を取得／設定します．
			/// </summary>
			public string SeriesName { get; set; }

			/// <summary>
			/// ソースファイル内の系列の列番号を取得／設定します．
			/// </summary>
			public int SeriesNo { get; set; }

			/// <summary>
			/// CSVファイルのルートとなるディレクトリを，絶対パスまたはRootPathからの相対パスで取得／設定します．
			/// </summary>
			public string SourceRootPath { get; set; }

			#endregion

			#region *[override]pltコマンド列を生成(Generate)

			/*
				cd 'C:\Users\den\Documents\EP-5.5\ElectricPower'
				set terminal png medium size 640,480
				set output 'public/data/Y2015_07/201507_riko.png'
				load 'var/common.plt'
				set ytics border mirror norotate 100.000,100,700.000
				set y2tics border mirror norotate ("" 600)
				set grid y2tics noxtics
				set title 'Power Consumption (2015.7, riko)   Monthly Total 81301 kWh'
				set yrange [ 100.000 : 700.000 ] noreverse nowriteback
				plot 'public/data/Y2015_07/D01_01.csv' using 1:2 w linespoints pt 1 lc 4 title '7/01(Wed)', 'public/data/Y2015_07/D02_01.csv' using 1:2 w linespoints pt 1 lc 3 title '7/02(Thu)', 'public/data/Y2015_07/D03_01.csv' using 1:2 w linespoints pt 1 lc 2 title '7/03(Fri)', ～

			*/

			// 描画対象月(日)はCurrentDateプロパティで与えます．
			public override void Generate(StreamWriter writer)
			{
				writer.WriteLine("set terminal png medium size {0},{1}", this.Width, this.Height);
				writer.WriteLine("set output '{0}'", this.ChartDestination);
				writer.WriteLine("set datafile separator ','");

				// 横軸
				writer.WriteLine("set xlabel 'Time'");
				writer.WriteLine("set xrange [ 1 : 24 ]");
				writer.WriteLine("set xtics border mirror norotate 1,1,24");

				// 縦軸
				writer.WriteLine("set ylabel 'Electric Power [kWh]'");

				var range = this.Maximum - this.Minimum;
				if (range > 0)
				{
					writer.WriteLine("set yrange [{0} : {1}] noreverse nowriteback", this.Minimum, this.Maximum);
					int span = 100;
					if (range < 200)
					{
						span = 20;
					}
					else if (range < 400)
					{
						span = 50;
					}
					writer.WriteLine("set ytics border mirror norotate {0},{2},{1}", this.Minimum, this.Maximum, span);
				}

				// タイトル
				writer.WriteLine("set title 'Power Consumption ({0}, {1})   Monthly Total {2} kWh'",
					CurrentDate.ToString("yyyy.M"),
					this.SeriesName,
					33333	// ★月間トータルをどうやって取得する？？？
				);


				// プロット
				// plot 'public/data/Y2015_07/D01_01.csv' using 1:2 with linespoints pt 1 lc 4 title '7/01(Wed)',
				//      'public/data/Y2015_07/D02_01.csv' using 1:2 w linespoints pt 1 lc 3 title '7/02(Thu)',
				//      'public/data/Y2015_07/D03_01.csv' using 1:2 w lp pt 1 lc 2 title '7/03(Fri)', ～

				// 問題点がたくさん．
				// P1.日付→ソースファイルの変換はどこで行う？
				// P2.曜日ごとのフォーマットはどうする？

				// S1.外部からヘルパか何かで与えるといいんじゃない？CSVと共通だし．
				//   ↑外部のヘルパを使って内部で決め打つことにしてみた．
				// S2.ここでゴリゴリ書くしかないのかな？"Wed"とか出すのも結構面倒だし．(dddだと"水"になってしまう．)

				List<string> source_list = new List<string>();
				var source_root = Path.IsPathRooted(this.SourceRootPath) ? this.SourceRootPath : Path.Combine(this.RootPath, this.SourceRootPath);

				var date = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
				while (date.Date <= CurrentDate.Date)
				{
					source_list.Add(
						string.Format("'{0}' using 1:{2} w lp {1}", 
							 Path.Combine(source_root, DailyCsvDestinationGenerator.Generate(date)),
							 GenerateFormatString(date),
							 this.SeriesNo));
					date = date.AddDays(1);
				}
				writer.WriteLine("plot " + string.Join(", ", source_list.ToArray()));

			}
			#endregion


			// (1.3.6)とりあえず決め打ちだらけ．
			#region *系列の書式文字列を生成(GenerateFromatString)
			public string GenerateFormatString(DateTime date)
			{
				// マーカを週毎にする．
				// [＋，＊，△，○，□](環境依存)
				// MARKERS = [1, 3, 8, 6, 4].freeze
				int marker = 4;
				if (date.Date == this.CurrentDate.Date)
				{
					marker = 5;	//当日用マーカ('■')；環境依存
				}
				else
				{
					switch ((date.Day - 1) / 7)
					{
						case 0:
							marker = 1;
							break;
						case 1:
							marker = 3;
							break;
						case 2:
							marker = 8;
							break;
						case 3:
							marker = 6;
							break;
					}
				}

				// 線の色を曜日毎にする．
				// [金，茶，赤，紫，青，緑，灰](環境依存)
				// LINE_COLORS = [7, 6, 1, 4, 3, 2, 24].freeze

				int color = 0;
				string day_of_week = string.Empty;
				switch(date.DayOfWeek)
				{
					case DayOfWeek.Sunday:
						color = 7;
						day_of_week = "Sun";
					break;
					case DayOfWeek.Monday:
					color = 6;
						day_of_week = "Mon";
					break;
					case DayOfWeek.Tuesday:
					color = 1;
						day_of_week = "Tue";
					break;
					case DayOfWeek.Wednesday:
					color = 4;
						day_of_week = "Wed";
					break;
					case DayOfWeek.Thursday:
					color = 3;
						day_of_week = "Thu";
					break;
					case DayOfWeek.Friday:
					color = 2;
						day_of_week = "Fri";
					break;
					case DayOfWeek.Saturday:
					color = 24;
						day_of_week = "Sat";
					break;
				}

				// pt 1 lc 2 title '7/02(Thu)'
				return string.Format("pt {0} lc {1} title '{2}({3})'", marker, color, date.ToString("M/dd"), day_of_week);
			}
			#endregion

			#region (1.3.3)プラグイン化

			// (1.3.3.2)とりあえずのコンストラクタ．
			public MonthlyChart(string databaseFile)
				: base()
			{ }
			public MonthlyChart() : base() { }

			public void Update()
			{
/*
				DateTime updated1 = new FileInfo(this.GetAbsolutePath(this.TemperatureCsvPath)).LastWriteTime;
				DateTime updated2 = new FileInfo(this.GetAbsolutePath(this.TrinityCsvPath)).LastWriteTime;

				var latestData = updated1 > updated2 ? updated1 : updated2;
				if (latestData > _current)
				{
					Gnuplot.GenerateGraph(this);
				}
				_current = latestData;
*/
			}
			DateTime _current;

			public void Configure(System.Xml.Linq.XElement config)
			{
				foreach (var attribute in config.Attributes())
				{
					switch (attribute.Name.LocalName)
					{
						//case "TemperatureSource":
						//	this.TemperatureCsvPath = attribute.Value;
						//	break;
						//case "TrinitySource":
						//	this.TrinityCsvPath = attribute.Value;
						//	break;
						case "Destination":
							this.ChartDestination = attribute.Value;
							break;
						case "DataRoot":
							this.RootPath = attribute.Value;
							break;
					}
				}
				var element = config.Element("ChartFormat");
				if (element != null)
				{
					foreach (var attribute in element.Attributes())
					{
						switch (attribute.Name.LocalName)
						{
							case "Width":
								this.Width = (int)attribute;
								break;
							case "Height":
								this.Height = (int)attribute;
								break;
							//case "FontSize":
							//	this.FontSize = (int)attribute;
							//	break;
						}
					}
				}
			}

			#endregion

			// (1.3.8) 試しに出力してみるメソッド．
			public void OtameshiPlt(DateTime date, string destination)
			{
				this.CurrentDate = date;
				using (var writer = new StreamWriter(File.Open(destination, FileMode.Create, FileAccess.Write)))
				{
					this.Generate(writer);
				}
			}


		}
		#endregion

	}
}
