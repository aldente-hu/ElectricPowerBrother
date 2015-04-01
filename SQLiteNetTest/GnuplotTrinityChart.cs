using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Helpers;

	/// <summary>
	/// かつてのGnuplotTrinityChartクラスに対応しています．
	/// </summary>
	public class GnuplotTrinityChart : PltFileGeneratorBase, IUpdatingPlugin
	{
		/// <summary>
		/// 出力するグラフの横幅を取得／設定します．
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// 出力するグラフの高さを取得／設定します．
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// 出力するグラフのフォントサイズを取得／設定します．
		/// </summary>
		public int FontSize { get; set; }

		/// <summary>
		/// Trinityデータのファイルパスを，絶対パスまたは(RootPathからの)相対パスで指定します．
		/// </summary>
		public string TrinityCsvPath { get; set; }

		/// <summary>
		/// 気温データのファイルパスを，絶対パスまたは(RootPathからの)相対パスで指定します．
		/// </summary>
		public string TemperatureCsvPath { get; set; }

		public override void Generate(StreamWriter writer)
		{
			// ※決め打ちだらけだけど，まあとりあえず．

			// (1.1.2.0)温度の範囲をプローブする．
			decimal? max_temp = null;
			decimal? min_temp = null;
			var path = this.GetAbsolutePath(TemperatureCsvPath);
			using (var reader = new System.IO.StreamReader(path))	// ←これ相対パスで大丈夫だっけ？←ダメっぽい．
			{
				while (!reader.EndOfStream)
				{
					decimal temp;
					var cols = reader.ReadLine().Split(',');
					if (cols.Length > 1 && Decimal.TryParse(cols[1], out temp))
					{
						if (!max_temp.HasValue || max_temp < temp) { max_temp = temp; }
						if (!min_temp.HasValue || min_temp > temp) { min_temp = temp; }
					}
				}
			}

			max_temp = max_temp.HasValue ? Decimal.Ceiling(max_temp.Value) : 2;
			min_temp = min_temp.HasValue ? Decimal.Floor(min_temp.Value) : -4;

			int step_temp = 1;
			var diff = max_temp - min_temp;
			while (diff > 8 * step_temp)
			{
				step_temp++;
			}
			max_temp = min_temp + 8 * step_temp;

			if (string.IsNullOrEmpty(this.RootPath))
			{
				writer.WriteLine(string.Format("cd '{0}'", RootPath));
			}
			writer.WriteLine(string.Format("set terminal svg enhanced size {0},{1} fsize {2}", this.Width, this.Height, this.FontSize));
			writer.WriteLine(string.Format("set output '{0}'", ChartDestination));	// ※とりあえず決め打ち．
			writer.WriteLine("set encoding utf8");
			writer.WriteLine("set title \"電力消費量 (理工学部＋α)\"");	// 出力フォーマットによっては全角の"＋"は使わない方がよい．

			// 各軸の設定
			writer.WriteLine("set xlabel '時刻 [Hour]'");
			writer.WriteLine("set xrange [ 0.0 : 24.0 ]");
			writer.WriteLine("set xtics border mirror norotate 0,1,24");

			var y1_max = 120;	// (1.1.2.1) 40-120 に変更．
			var y1_min = 40;
			writer.WriteLine("set ylabel '10分間電力消費量 [kWh]'");
			writer.WriteLine(string.Format("set yrange [ {0} : {1} ]", y1_min, y1_max));
			writer.WriteLine(string.Format("set ytics border {0},10,{1}", y1_min, y1_max));

			writer.WriteLine("set y2label '気温 [℃]'");
			writer.WriteLine(string.Format("set y2range [ {0} : {1} ]", min_temp, max_temp));
			writer.WriteLine(string.Format("set y2tics border {0},{2},{1}", min_temp, max_temp, step_temp));

			// レイアウトの設定
			writer.WriteLine("set grid y2tics");
			writer.WriteLine("set key left top autotitle columnheader");	// これを使うと文字エンコーディングがおかしくなる？

			// プロットするデータの設定
			writer.WriteLine("set datafile separator ','");
			writer.WriteLine(string.Format(
				"plot '{0}' using 1:2 w lines lw 3 lc rgbcolor '#FF0000', '{0}' using 1:3 w lines lc rgbcolor '#FF3399', '{0}' using 1:4 w lines lc rgbcolor '#FF99CC', '{1}' using 1:2 w lines lw 3 lc rgbcolor '#0000CC' axis x1y2",
				TrinityCsvPath,
				TemperatureCsvPath)
			);

			writer.WriteLine("set output");
			writer.WriteLine("exit");

		}



		/*
cd 'C:\Users\den\Documents\EP-5.5\ElectricPower'
set terminal svg enhanced size 900,600
set output 'public/himichu/today_test.svg'
#load 'var/common_j.plt'

set title '電力消費量 (理工学部＋α)'

set xlabel "時刻 [Hour]"
set xrange [ 0.00000 : 24.0000 ]
set xtics border mirror norotate 0.000,1,24.000
set ylabel "10分間電力消費量 [kWh]"
set yrange [ 50.000 : 130.000 ] noreverse nowriteback
set ytics border mirror norotate 50.000,10,130.000
set y2label "気温 [℃]"
set y2range [ -8.000 : 8.000 ] noreverse nowriteback
set y2tics border mirror norotate -8.000,2,8.000


set grid y2tics
set key left top

set datafile separator ','
plot 'public/himichu/trinity.csv' using 1:2 w lines lw 3 lc rgbcolor "#FF0000" title 'tanuki', 'public/himichu/trinity.csv' using 1:3 w lines lc rgbcolor "#FF3399" title 'kitsune', 'public/himichu/trinity.csv' using 1:4 w lines lc rgbcolor "#FF99CC" title 'usagi', 'public/himichu/today_temperature.csv' using 1:2 w lines lw 3 lc rgbcolor "#0000CC" axis x1y2 title '気温'


set output
		 */

		#region (1.3.3)プラグイン化

		// (1.3.3.2)とりあえずのコンストラクタ．
		public GnuplotTrinityChart(string databaseFile) : base()
		{ }
		public GnuplotTrinityChart() : base() { }

		public void Update()
		{
			DateTime updated1 = new FileInfo(this.TemperatureCsvPath).LastWriteTime;
			DateTime updated2 = new FileInfo(this.TrinityCsvPath).LastWriteTime;

			var latestData = updated1 > updated2 ? updated1 : updated2;
			if (latestData > _current)
			{
				GnuplotChartBase.GenerateGraph(this);
			}
			_current = latestData;
		}
		DateTime _current;

		public void Configure(System.Xml.Linq.XElement config)
		{
			foreach (var attribute in config.Attributes())
			{
				switch(attribute.Name.LocalName)
				{
					case "TemperatureSource":
						this.TemperatureCsvPath = attribute.Value;
						break;
					case "TrinitySource":
						this.TrinityCsvPath = attribute.Value;
						break;
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
					switch(attribute.Name.LocalName)
					{
						case "Width":
							this.Width = (int)attribute;
							break;
						case "Height":
							this.Height = (int)attribute;
							break;
						case "FontSize":
							this.FontSize = (int)attribute;
							break;
					}
				}
			}
		}

		#endregion


	}



}
