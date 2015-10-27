using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Helpers;

	namespace Legacy
	{
		// (1.3.6)
		#region MonthlyChartクラス
		public class MonthlyChart : PltFileGeneratorBase, IPlugin
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

			// (1.3.12)
			/// <summary>
			/// 表示する月間合計のチャンネルを取得／設定します．
			/// </summary>
			public int[] MonthlyTotalChannels { get; set; }

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
			/// ソースファイル内の系列の列番号を取得／設定します(最初の列が1，ただし最初の列は時刻が入ると想定している)．
			/// </summary>
			public int SeriesNo { get; set; }

			/// <summary>
			/// CSVファイルのルートとなるディレクトリを，絶対パスまたはRootPathからの相対パスで取得／設定します．
			/// </summary>
			public string SourceRootPath { get; set; }
			// ※とりあえずグラフの出力先のルートも↑と同じにしておく．

			// (1.3.13)
			/// <summary>
			/// 点線(たぶん)でボーダーラインを表示する値を取得／設定します．
			/// </summary>
			public int? BorderLine { get; set; }

			// (1.3.18)
			/// <summary>
			/// 'pngcairo'で出力するかどうかの値を取得／設定します．
			/// falseであれば，'png'で出力します．
			/// </summary>
			public bool UseCairo { get; set; }

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

			// とりあえずoverrideは無視して，理想的な引数を与えましょう．
			// →で，もう継承を解除するか，継承元を変更するか...
			public override void Generate(StreamWriter writer, DateTime hour)
			{
				// hour.Min should be 0!
				//var hour = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);

				// ↑この処理は呼び出し元で行うべきだよなぁ．

				// time       => month_origin
				// 8/31 23:00 => 8/01 00:00
				// 9/01 00:00 => 8/01 00:00
				// 9/01 00:50 => 8/01 00:00 (Nothing to output!)
				// 9/01 01:00 => 9/01 00:00

				DateTime month_origin;
				if (hour.Day == 1 && hour.Hour == 0)
				{
					month_origin = hour.AddMonths(-1);
				}
				else
				{
					month_origin = new DateTime(hour.Year, hour.Month, 1);
				}


				// pngcairo...レイアウトやカラーインデックスがかなり変わってしまうので，
				// set terminal だけ修正すれば使えるというものでもないようです．
				writer.WriteLine("set terminal {2} size {0},{1}", this.Width, this.Height, this.UseCairo ? "pngcairo" : "png medium");
				writer.WriteLine("set output '{0}'",
						Path.Combine(
							GetAbsolutePath(SourceRootPath),	// 出力先をSourceRootPathに固定している！しかもRootPathを考慮していない！
							MonthlyChartDestinationGenerator.Generate(month_origin, this.SeriesName)
						)
				);
				writer.WriteLine("set datafile separator ','");
				writer.WriteLine("set key outside");
				// もともとはこんな感じだった．何がどう作用しているのかは全くわからん．
				writer.WriteLine("set key outside Right box linetype -2 linewidth 1 samplen 4 spacing 1 width 3 height -2 autotitles");

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
					int mytics = 5;
					if (range < 200)
					{
						span = 20;
						mytics = 4;
					}
					else if (range < 400)
					{
						span = 50;
					}
					writer.WriteLine("set ytics border mirror norotate {0},{2},{1}", this.Minimum, this.Maximum, span);
					
					// (1.3.13.1)補助目盛(主目盛りを何等分するかということ)．
					// とりあえず常に出力する設定にしておく．
					writer.WriteLine("set mytics {0}", mytics);
				}



				// (1.3.13)ボーダーライン
				if (this.BorderLine.HasValue)
				{
					writer.WriteLine("set y2tics border mirror norotate (\"\" {0})", this.BorderLine.Value);
					writer.WriteLine("set grid y2tics noxtics");
				}

				// タイトル
				// (1.3.12)月間合計チャンネルの決め打ちを解消．
				var title = string.Format("Power Consumption ({0}, {1})", month_origin.ToString("yyyy.M"), this.SeriesName);
				if (MonthlyTotalChannels != null)
				{
					title += string.Format("   Monthly Total {0} kWh", _consumptionTable.GetTotal(month_origin, hour, MonthlyTotalChannels));
				}
				writer.WriteLine("set title '{0}'", title);


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

				var date = month_origin;
				// hourが00:00ならば，その前日までしかループされない．
				while (date.Date < hour)
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

			// (1.3.18)UseCairoの設定を反映．
			// (1.3.6)とりあえず決め打ちだらけ．
			#region *系列の書式文字列を生成(GenerateFromatString)
			public string GenerateFormatString(DateTime date)
			{
				// マーカを週毎にする．
				// [＋，＊，△，○，□](環境依存)
				// MARKERS = [1, 3, 8, 6, 4].freeze
				int marker = 4;
				if (date.Date == DateTime.Today)
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

			// (1.3.11)やっぱり復活．
			// (1.3.8.2)引数付きコンストラクタをコメントアウト．(うーん，このあたり混乱してるかもなぁ．)
			// (1.3.3.2)とりあえずのコンストラクタ．
			public MonthlyChart(string databaseFile)
				: base()
			{
				this._consumptionTable = new Data.ConsumptionData(databaseFile);
			}
			//public MonthlyChart() : base() { }

			readonly Data.ConsumptionData _consumptionTable;


			public DateTime Update(DateTime lastUpdate)
			{
				var latest_data = _consumptionTable.GetLatestDataTime();

				// hour.Min should be 0!
				var latest_hour = new DateTime(latest_data.Year, latest_data.Month, latest_data.Day, latest_data.Hour, 0, 0);

				// 最初の呼び出し．いつのデータまで描画されたかを知る手段はないので，とりあえず出力する．
				if (lastUpdate.Ticks == 0)
				{
					this.DrawChart(latest_hour);
					return latest_hour;
					// 理想的にはこれだけの話．
				}

				if (lastUpdate >= latest_hour)
				{
					return lastUpdate;
				}
				else
				{
					// 7/31 23:00 -> 8/01 00:00
					//   7月分だけ描画すればよい．これはAで行われる．
					// 8/01 00:00 => 8/01 01:00
					//   8月分だけ描画すればよい．これはBで行われる．

					// Yearも一応チェックしておく．
					while (lastUpdate.Month != latest_hour.Month || lastUpdate.Year != latest_hour.Year)
					{
						var next = new DateTime(lastUpdate.Year, lastUpdate.Month, 1).AddMonths(1);
						DrawChart(next);	// A
						lastUpdate = next;
					}

					if (latest_hour.Day != 1 || latest_hour.Hour != 0)
					{
						DrawChart(latest_hour);	// B
					}
					return latest_hour;
				}
			}
			
			// ↓こういうのはPluginTickerのレイヤーで覚えていてもらう．
			//DateTime _current;


			// とりあえずDataRootは使わず，SourceRootPathに絶対パスを記述する運用にする．
			// <Config source_root="～" use_cairo="true">
			//   <ChartFormat width="640" height="480" />
			//   <Series no="2" name="riko" />
			//   <YValue max="800" min="0" border="600" />
			//   <MonthlyTotal ch="1+2" />
			// </Config>

			// (1.3.18)UseCairoプロパティの設定を追加．
			// (1.3.14)本格的に整備．
			#region *XMLから設定(Configure)
			public void Configure(XElement config)
			{
				foreach (var attribute in config.Attributes())
				{
					switch (attribute.Name.LocalName)
					{
						//case "DataRoot":
						//	this.RootPath = attribute.Value;
						//	break;
						case "source_root":
							this.SourceRootPath = attribute.Value;
							break;
						case "use_cairo":
							this.UseCairo = ((bool?)attribute).Value;
							break;
					}
				}
				var element = config.Element("ChartFormat");
				if (element != null)
				{
					ConfigureChartFormat(element);
				}
				element = config.Element("Series");
				if (element != null)
				{
					ConfigureSeries(element);
				}
				element = config.Element("YValue");
				if (element != null)
				{
					ConfigureYValue(element);
				}
				element = config.Element("MonthlyTotal");
				if (element != null)
				{
					ConfigureMonthlyTotal(element);
				}
			}

			protected void ConfigureChartFormat(XElement format)
			{
				foreach (var attribute in format.Attributes())
				{
					switch (attribute.Name.LocalName)
					{
						case "width":
							this.Width = (int)attribute;
							break;
						case "height":
							this.Height = (int)attribute;
							break;
						//case "FontSize":
						//	this.FontSize = (int)attribute;
						//	break;
					}
				}
			}

			protected void ConfigureSeries(XElement series)
			{
				foreach (var attribute in series.Attributes())
				{
					switch (attribute.Name.LocalName)
					{
						case "no":
							this.SeriesNo = (int)attribute;
							break;
						case "name":
							this.SeriesName = attribute.Value;
							break;
					}
				}
			}

			protected void ConfigureYValue(XElement yValue)
			{
				foreach (var attribute in yValue.Attributes())
				{
					switch (attribute.Name.LocalName)
					{
						case "max":
							this.Maximum = (int)attribute;
							break;
						case "min":
							this.Minimum = (int)attribute;
							break;
						case "border":
							this.BorderLine = (int)attribute;
							break;
					}
				}
			}

			protected void ConfigureMonthlyTotal(XElement monthlyTotal)
			{
				foreach (var attribute in monthlyTotal.Attributes())
				{
					switch (attribute.Name.LocalName)
					{
						case "ch":
							this.MonthlyTotalChannels = attribute.Value.Split('+',',').Select(d => int.Parse(d)).ToArray();
							break;
					}
				}
			}

			#endregion

			#endregion

			// (1.3.11)
			protected void DrawChart(DateTime time)
			{
				// tempファイルのwriterをつくってGeneratePlt(writer, time)してgnuplot実行！！
				// ...ということを，このメソッド1つでやってくれるんだった．
				Gnuplot.GenerateChart(this, time);
			}

			#region テスト用メソッド

			// (1.3.8) 試しに出力してみるメソッド．
			public void OtameshiPlt(DateTime date, string destination)
			{
				//this.CurrentDate = date;
				using (var writer = new StreamWriter(File.Open(destination, FileMode.Create, FileAccess.Write)))
				{
					this.Generate(writer, new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0));
				}
			}

			// (1.3.11)
			#region *グラフを試しに出力(DrawChartTest)
			/// <summary>
			/// DrawChartメソッドのお試し実行をします．
			/// timeのHourより下の情報はメソッド内で切り捨てられます．
			/// </summary>
			/// <param name="time"></param>
			public void DrawChartTest(DateTime time)
			{
				var hour = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
				DrawChart(hour);
			}
			#endregion


			#endregion


		}
		#endregion


		// (1.3.11)
		#region [static]MonthlyChartDestinationGeneratorクラス
		public static class MonthlyChartDestinationGenerator
		{
			// 2015年7月2日を表すDateTimeから， "public/data/Y2015_07/D02_01.csv"のような文字列を生成する．

			// ルートの処理はどこで行う？

			public static string Generate(DateTime date, string name)
			{
				// "g" -> "西暦"
				return string.Format(date.ToString(@"\Yyyyy_MM/yyyyMM_{0}.pn\g"), name);
			}

			public static string GenerateDirectory(DateTime month)
			{
				return month.ToString(@"\Yyyyy_MM");
			}
		}
		#endregion

	}
}
